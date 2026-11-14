using ServiceManager.Base.Data;
using ServiceManager.Base.Events;
using ServiceManager.Base.Windows;
using ServiceManager.Extensions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager.Base
{
    public class ServiceManager
    {
        public ServiceConfig Config { get; set; }

        public bool IsRunning { get; set; }

        public Analytics Analytics { get; set; }

        /// <summary>
        /// Who may connect to the endpoint. Left empty, the service follows the settings file.
        /// A test run inside the UI hands OwnUser, so that a developer without elevated rights
        /// can watch its own process.
        /// </summary>
        public Channel.eAccess? Access { get; set; } = null;


        Channel Connection { get; set; }

        /// <summary>
        /// Asks the machine whether a Windows service runs. Replaced by a fake where the dependency wait
        /// is tested without a service control manager.
        /// </summary>
        public IWindowsServiceStatus WindowsStatus { get; set; }

        /// <summary>
        /// Which services were asked to stop. A process that ends without an entry here ended on its own,
        /// which is a crash rather than the stop the user asked for.
        /// </summary>
        public StopRequestTracker StopRequests { get; set; }

        /// <summary>
        /// Where the manager reports what it notices. The WCF push and the provider dispatch are subscribers
        /// here rather than calls at every place where something happens, and a test hands in a recording
        /// fake instead of a channel.
        /// </summary>
        public IServiceEventBus Events { get; set; }

        public List<Guid> LiveLogs { get; set; }

        public List<Process> RunningProcesses { get; set; }


        public ServiceManager()
        {
            LiveLogs = new List<Guid>();

            RunningProcesses = new List<Process>();

            WindowsStatus = new WindowsServiceStatus();

            StopRequests = new StopRequestTracker();

            Events = new EventBus();

            //The order is the one the hand-written calls had: the client is told first, the providers after.
            Events.Subscribe(PushToClients);
            Events.Subscribe(SendToProviders);
        }


        public void Start()
        {
            var cfg = ServiceConfig.load();

            Config = cfg;

            Raise(NotificationKind.ServerStart, this.Config.Notifications.InstanceName + ": Server started");

            this.IsRunning = true;

            Analytics = new Analytics();

            switch (this.Config.RuntimeSettings.Mode)
            {
                case Data.Settings.Runtime.eMode.NetPipes:
                    Connection = new Channel(true, "serviceman" + Process.GetCurrentProcess().Id.ToString());
                    break;

                case Data.Settings.Runtime.eMode.Tcp:
                    Connection = new Channel(true, "localhost", this.Config.RuntimeSettings.TCPPort);
                    break;

                default:

                    throw new ArgumentOutOfRangeException(nameof(this.Config.RuntimeSettings.Mode));
            }

            Connection.Access = this.Access ?? (this.Config.RuntimeSettings.RequireAdministrator ? Channel.eAccess.Administrators : Channel.eAccess.Everyone);

            try
            {
                Connection.Start();
            }
            catch
            {
                return;
            }



            Connection.Server.Manager = this;

            PrepareServices(cfg);

            Thread th = new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(cfg.RuntimeSettings.Startup_Delay * 1000);

                StartServices(cfg);
            }));

            th.Start();

        }

        private void PrepareServices(ServiceConfig cfg)
        {
            foreach (var c in cfg.ServiceList)
            {
                PrepareService(c);
            }
        }

        private void StartServices(ServiceConfig cfg)
        {
            foreach (var c in cfg.ServiceList)
            {
                //Don't start
                if (!c.Enabled)
                    continue;

                StartService(c);
            }
        }

        /// <summary>
        /// Preparing initialization of analytics and co
        /// </summary>
        /// <param name="item"></param>
        public void PrepareService(ServiceItem item)
        {
            if (item == null)
            {
                return;
            }

            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID) ?? new ServiceAnalytics();

            if (!Analytics.Services.Contains(sa))
            {
                sa.ServiceID = item.ID;
                Analytics.Services.Add(sa);
            }

            sa.Status = ServiceAnalytics.eStatus.offline;
            sa.ProcessID = 0;
            sa.Started = null;
            sa.Exited = null;
            sa.Exiting = null;

            StopRequests.Clear(item.ID);
        }

        /// <summary>
        /// Starts the service and add some event hooks.
        /// </summary>
        /// <param name="item"></param>
        public void StartService(ServiceItem item)
        {
            StartService(item, true);
        }

        /// <summary>
        /// Starts the service. A service whose Windows dependencies are not up yet does not start right away,
        /// it waits for them on its own thread rather than holding up the caller.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="respectDependencies">False when an automatic restart carries on without the wait.</param>
        public void StartService(ServiceItem item, bool respectDependencies)
        {
            if (item == null)
            {
                return;
            }

            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID) ?? new ServiceAnalytics();

            if (!Analytics.Services.Contains(sa))
            {
                sa.ServiceID = item.ID;
                Analytics.Services.Add(sa);
            }

            if (respectDependencies)
            {
                var missing = WindowsDependencyWait.FirstMissing(item.WindowsDependencies, this.WindowsStatus);

                if (missing != null)
                {
                    WaitForDependency(item, sa, missing);

                    return;
                }
            }

            StartProcess(item, sa);
        }

        /// <summary>
        /// Puts the service into the waiting state and lets a thread watch the machine until the dependency is
        /// up, the wait has run out, or the user took the service offline again.
        /// </summary>
        private void WaitForDependency(ServiceItem item, ServiceAnalytics sa, String missing)
        {
            //A wait is already running, a second caller has nothing to add
            if (sa.Status == ServiceAnalytics.eStatus.waiting_for_dependency)
            {
                return;
            }

            sa.Status = ServiceAnalytics.eStatus.waiting_for_dependency;
            sa.ProcessID = 0;
            sa.AppendError(WindowsDependencyWait.WaitError(missing));

            Raise(NotificationKind.DependencyWait, eCallback.updated, WindowsDependencyWait.WaitText(this.Config.Notifications.InstanceName, item.Title, missing), item, missing);

            Thread th = new Thread(new ThreadStart(() => WaitForDependencies(item, sa)));

            th.IsBackground = true;
            th.Start();
        }

        private void WaitForDependencies(ServiceItem item, ServiceAnalytics sa)
        {
            var started = DateTime.Now;
            var timeout = this.Config?.RuntimeSettings?.Dependency_Wait_Timeout ?? WindowsDependencyWait.Default_Timeout_Seconds;

            while (true)
            {
                //The manager stopped, or the waiting service was taken offline
                if (!this.IsRunning || sa.Status != ServiceAnalytics.eStatus.waiting_for_dependency)
                {
                    return;
                }

                var missing = WindowsDependencyWait.FirstMissing(item.WindowsDependencies, this.WindowsStatus);

                if (missing == null)
                {
                    StartProcess(item, sa);

                    return;
                }

                if (WindowsDependencyWait.HasTimedOut(started, timeout, DateTime.Now))
                {
                    DependencyWaitFailed(item, sa, missing, timeout);

                    return;
                }

                Thread.Sleep(WindowsDependencyWait.Poll_Milliseconds);
            }
        }

        private void DependencyWaitFailed(ServiceItem item, ServiceAnalytics sa, String missing, int timeout)
        {
            sa.Status = ServiceAnalytics.eStatus.start_failed;
            sa.ProcessID = 0;
            sa.Started = null;
            sa.Exited = DateTime.Now;
            sa.Exiting = null;
            sa.AppendError(WindowsDependencyWait.TimeoutError(missing, timeout));

            Raise(NotificationKind.DependencyWait, eCallback.updated, WindowsDependencyWait.TimeoutText(this.Config.Notifications.InstanceName, item.Title, missing, timeout), item, missing);
        }

        /// <summary>
        /// Starts the process and add some event hooks.
        /// </summary>
        private void StartProcess(ServiceItem item, ServiceAnalytics sa)
        {
            //The wait and a manual start can reach this point at the same time, only one of them gets the process
            if (sa.IsRunning)
            {
                return;
            }

            StopRequests.Clear(item.ID);

            Raise(NotificationKind.ServiceStart, this.Config.Notifications.InstanceName + ": " + item.Title + "\r\nStatus: Started", item);

            Process proc = new Process();

            sa.Status = ServiceAnalytics.eStatus.starting;


            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = item.Path;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;

            proc.EnableRaisingEvents = true;

            proc.Exited += (s, en) =>
            {

                sa.Status = ServiceAnalytics.eStatus.offline;

                Raise(eCallback.exited, item, sa);

                Raise(eCallback.updated, item, sa);

                Process_Exited(item, proc);
            };

            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;

            try
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }
            catch
            {
                item.ForceRestart = false;
                sa.AppendError("SERVICE MANAGER: START FAILED\r\n");
                sa.ProcessID = 0;
                sa.Started = null;
                sa.Exited = DateTime.Now;
                sa.Exiting = null;

                sa.Status = ServiceAnalytics.eStatus.start_failed;

                Raise(eCallback.updated, item, sa);

                return;
            }

            lock (RunningProcesses)
            {
                RunningProcesses.Add(proc);
            }

            sa.ProcessID = proc.Id;
            sa.Started = DateTime.Now;
            sa.Exited = null;
            sa.Exiting = null;

            sa.Status = ServiceAnalytics.eStatus.running;

            Raise(eCallback.started, item, sa);
        }

        public void ServiceEnd(ServiceItem item)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID) ?? new ServiceAnalytics();

            StopRequests.Mark(item.ID);

            //There is no process to end while the service holds back, taking it offline is what stops the wait
            if (sa.Status == ServiceAnalytics.eStatus.waiting_for_dependency)
            {
                sa.Status = ServiceAnalytics.eStatus.offline;

                Raise(eCallback.updated, item, sa);

                return;
            }

            Process proc;

            lock (RunningProcesses)
            {

                proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
                if (proc == null)
                {
                    return;
                }

                sa.Status = ServiceAnalytics.eStatus.shutting_down;

                if (RunningProcesses.Contains(proc))
                    RunningProcesses.Remove(proc);

            }

            if (proc.HasExited)
            {
                return;
            }

            if (IsRunning)
            {
                Raise(eCallback.exiting, item, sa);
            }

            if (item.Shutdown_ENTER_Send)
            {
                proc.StandardInput.WriteLine("\r\n");

                if (item.Shutdown_ENTER_Timeout > 0)
                {
                    Wait(item.Shutdown_ENTER_Timeout, a => proc.HasExited);
                }
            }

            if (!proc.HasExited)
            {
                proc.CloseMainWindow();

                if (item.Shutdown_Timeout > 0)
                {
                    Thread.Sleep(item.Shutdown_Timeout * 1000);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            if (!proc.HasExited)
            {
                proc.Kill();
            }

            sa.Status = ServiceAnalytics.eStatus.offline;
        }

        public void SendInput(ServiceItem item, String input)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID) ?? new ServiceAnalytics();

            var proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
            if (proc == null)
            {
                return;
            }

            try
            {
                sa.AppendOutput(input + "\r\n");

                var t = new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(100);
                    proc.StandardInput.WriteLine(input);
                }));
                t.Start();

            }
            catch
            {

            }
        }

        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Process proc = sender as Process;
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == proc.Id) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            var s = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (s == null)
                return;

            Raise(NotificationKind.ConsoleError, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\nError:\r\n" + e.Data, s, e.Data);

            sa.AppendError((e.Data ?? "") + "\r\n");
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Process proc = sender as Process;
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == proc.Id) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            var s = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (s == null)
                return;

            Raise(NotificationKind.ConsoleOutput, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\n" + e.Data, s, e.Data);

            var service = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (service.LogConsoleOutput)
            {
                //Save to harddrive
                if (sa.LastSaved.Date != DateTime.Today)
                {

                    if (service.LogConsoleOutputToDisk && SaveToDisk(service, sa.Output))
                    {
                        sa.Output = "";
                        sa.LastSaved = DateTime.Now;
                    }
                    else if (service.ResetConsoleOutputDaily)
                    {
                        sa.Output = "";
                        sa.LastSaved = DateTime.Now;
                    }
                }

                sa.AppendOutput(e.Data + "\r\n");
            }

            if (Connection.ServerChannel == null || Connection.Failed)
                return;


            //The ping has to reach the client in front of the line that follows it, so both stay in this task.
            var t = new Task(() =>
            {
                if (DateTime.Now.Subtract(sa.LastPing).TotalSeconds > 10)
                {
                    Connection?.Try(a => a.ActivityPing(sa.ServiceID));
                    sa.LastPing = DateTime.Now;
                }

                Raise(eCallback.logs, s, sa, e.Data);
            });

            t.Start();
        }

        public bool SaveToDisk(ServiceItem service, String content)
        {

            var path = AppContext.BaseDirectory + "\\logs\\" + service.ID.ToString() + "\\";

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.AppendAllText(path + "log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", content);

                return true;
            }
            catch
            {

            }

            return false;
        }

        private void Process_Exited(ServiceItem service, Process proc)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);
            if (sa != null)
            {
                sa.Exited = DateTime.Now;

            }

            var s = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (s == null)
                return;

            sa.Status = ServiceAnalytics.eStatus.offline;

            if (service.LogConsoleOutput)
            {
                //Save to harddrive
                if (service.LogConsoleOutputToDisk && SaveToDisk(service, sa.Output))
                {
                    sa.Output = "";
                    sa.LastSaved = DateTime.Now;
                }
            }

            //if (LiveLogs.Contains(sa.ServiceID))
            //{
            //    Connection.ServerChannel.LiveLogsUpdate(sa.ServiceID, "SERVICE MANAGER: Service has been ended");
            //}

            if (!service.ForceRestart | !this.IsRunning)
            {
                var kind = StopRequests.ReasonOf(service.ID);
                var status = kind == NotificationKind.ServiceCrashed ? "Crashed" : "Stopped";

                Raise(kind, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\nStatus: " + status, s);

                return;
            }


            sa.Restarts++;

            StartService(service, service.Wait_On_Restart);

            Raise(NotificationKind.ServiceRestart, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\nStatus: Restarted", s);
        }

        public void Shutdown()
        {

        }

        public void Stop()
        {
            Raise(NotificationKind.ServerShutdown, this.Config.Notifications.InstanceName + ": Server shutting down");

            this.Connection?.Try(a => a.ShutdownStarted());

            this.IsRunning = false;

            List<Task> Exits = new List<Task>();


            foreach (var service in this.Config.ServiceList)
            {
                var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);
                if (sa == null || !sa.IsRunning)
                    continue;

                sa.Exiting = DateTime.Now;

                StopRequests.Mark(service.ID);

                var ex = new Task(() =>
                {
                    var proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
                    if (proc == null)
                        return;

                    sa.Status = ServiceAnalytics.eStatus.shutting_down;
                    var start = DateTime.Now;

                    Raise(eCallback.exiting, service, sa);

                    if (service.Shutdown_ENTER_Send)
                    {
                        proc.StandardInput.WriteLine("\r\n");

                        if (service.Shutdown_ENTER_Timeout > 0)
                        {
                            Wait(service.Shutdown_ENTER_Timeout, a => proc.HasExited);
                        }
                    }



                    if (!proc.HasExited)
                    {
                        proc.CloseMainWindow();

                        if (service.Shutdown_Timeout > 0)
                        {
                            Wait(service.Shutdown_Timeout, a => proc.HasExited);
                        }
                        else
                        {
                            Wait(10, a => proc.HasExited);
                        }
                    }

                    if (!proc.HasExited)
                        proc.Kill();

                    sa.Status = ServiceAnalytics.eStatus.offline;

                    Raise(eCallback.exited, service, sa);

                    sa.Exited = DateTime.Now;

                    //Save to harddrive
                    if (sa.LastSaved.Date != DateTime.Today)
                    {
                        if (service.LogConsoleOutput && SaveToDisk(service, sa.Output))
                        {
                            sa.Output = "";
                            sa.LastSaved = DateTime.Now;
                        }
                    }

                });

                ex.Start();

                Exits.Add(ex);
            }

            try
            {
                Task.WaitAll(Exits.ToArray());
            }
            catch
            {

            }


            Thread.Sleep(1000);

            this.Connection?.Try(a => a.ShutdownCompleted());

            Thread.Sleep(1000);


            this.Connection?.Stop();


            Raise(NotificationKind.ServerShutdown, this.Config.Notifications.InstanceName + ": Server shutted down");

        }

        /// <summary>
        /// Puts an event on the bus for the notification providers. The text keeps the wording the providers
        /// have always received, the structured fields are for providers that build cards or map an event
        /// to a log level.
        /// </summary>
        private void Raise(NotificationKind kind, String text, ServiceItem item = null, String details = null)
        {
            Raise(kind, eCallback.none, text, item, details);
        }


        /// <summary>
        /// Puts an event on the bus that the providers and the connected clients both read. The kind is what
        /// a provider opts into, the callback the push the client gets.
        /// </summary>
        private void Raise(NotificationKind kind, eCallback callback, String text, ServiceItem item = null, String details = null)
        {
            Events.Publish(new ServiceEvent()
            {
                Kind = kind,
                Callback = callback,
                ServiceId = item == null ? Guid.Empty : item.ID,
                Service = item,
                Title = item == null ? null : item.Title,
                Details = details,
                Text = text
            });
        }


        /// <summary>Puts an event on the bus that only the connected clients are told about.</summary>
        private void Raise(eCallback callback, ServiceItem item, ServiceAnalytics sa, String details = null)
        {
            Events.Publish(new ServiceEvent()
            {
                Callback = callback,
                ServiceId = item.ID,
                Service = item,
                Analytics = sa,
                Title = item.Title,
                Details = details
            });
        }


        /// <summary>
        /// Hands an event to every provider that opted into this kind.
        /// </summary>
        private void SendToProviders(ServiceEvent e)
        {
            if (e.Kind == null)
            {
                return;
            }

            var notifications = this.Config?.Notifications;

            if (notifications == null)
            {
                return;
            }

            var providers = notifications.GetProviders(e.Kind.Value);

            if (providers.Count == 0)
            {
                return;
            }

            Extensions.Extensions.SendNotification(providers, new NotificationMessage()
            {
                Kind = e.Kind.Value,
                Timestamp = e.Timestamp,
                InstanceName = notifications.InstanceName,
                ServiceTitle = e.Title,
                Details = e.Details,
                Text = e.Text
            });
        }


        /// <summary>
        /// Pushes an event to the connected client. The bus is in process, the callback contract is what the
        /// UI has always been given, so the shape of the push does not change with the bus.
        /// </summary>
        private void PushToClients(ServiceEvent e)
        {
            switch (e.Callback)
            {
                case eCallback.none:
                    return;

                case eCallback.updated:
                    Connection?.Try(a => a.ServiceUpdated(e.ServiceId, e.Service, e.Analytics));
                    break;

                case eCallback.started:
                    Connection?.Try(a => a.ServiceStarted(e.ServiceId, e.Service, e.Analytics));
                    break;

                case eCallback.exited:
                    Connection?.Try(a => a.ServiceExited(e.ServiceId, e.Service, e.Analytics));
                    break;

                case eCallback.exiting:
                    Connection?.Try(a => a.ServiceExiting(e.ServiceId, e.Service, e.Analytics));
                    break;

                case eCallback.logs:
                    if (LiveLogs.Contains(e.ServiceId))
                    {
                        Connection?.Try(a => a.LiveLogsUpdate(e.ServiceId, e.Details));
                    }
                    break;
            }
        }


        private void Wait(int seconds, Func<object, bool> HasExited)
        {
            var d = DateTime.Now;
            do
            {
                Thread.Sleep(250);
            } while (!HasExited(null) && DateTime.Now.Subtract(d).TotalSeconds < seconds);
        }


        public static bool IsServiceRunning
        {
            get
            {
                var name = "ServiceManager";
                var p = Process.GetProcessesByName(name);

                return (p.Length > 0);
            }
        }

        public static List<Process> GetProcesses()
        {
            var name = "ServiceManager"; // Process.GetCurrentProcess().ProcessName
            var lst = Process.GetProcessesByName(name).ToList();
            return lst;
        }


    }
}
