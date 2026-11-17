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

        /// <summary>
        /// Reads memory, processor use and threads of the processes behind the analytics. The processor use
        /// is a difference between two reads, so one sampler serves the whole run instead of one per call.
        /// </summary>
        public ProcessSampler Sampler { get; set; }

        public List<Guid> LiveLogs { get; set; }

        public List<Process> RunningProcesses { get; set; }

        /// <summary>
        /// The processes started under another account by token, keyed by their process id. They own the pipes of
        /// the standard streams, which the manager has to hand back once the process is gone.
        /// </summary>
        private readonly Dictionary<int, WindowsUserProcess> m_UserProcesses = new Dictionary<int, WindowsUserProcess>();

        public ServiceManager()
        {
            LiveLogs = new List<Guid>();

            RunningProcesses = new List<Process>();

            WindowsStatus = new WindowsServiceStatus();

            StopRequests = new StopRequestTracker();

            Events = new EventBus();

            Sampler = new ProcessSampler();

            //The client is pushed to and the providers are asked on threads of their own, so a provider that
            //waits for its webhook cannot hold up the push to the client.
            Events.Subscribe(eGroup.clients, PushToClients);
            Events.Subscribe(eGroup.providers, SendToProviders);
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
            var graph = new ServiceDependencyGraph(cfg.ServiceList);

            //A circle in the settings file is reported and then ignored, the services still come up
            if (graph.HasCycle)
            {
                Raise(NotificationKind.DependencyWait, cfg.Notifications.InstanceName + ": Dependency cycle between " + String.Join(", ", graph.Cyclic.Select(a => TitleOf(a))));
            }

            foreach (var c in graph.StartOrder(cfg.ServiceList))
            {
                //Don't start
                if (!c.Enabled)
                    continue;

                StartService(c);
            }
        }

        /// <summary>The title of a service, its id when the settings file no longer knows it.</summary>
        private String TitleOf(Guid id)
        {
            var service = this.Config?.ServiceList.FirstOrDefault(a => a.ID == id);

            return service != null ? service.Title : id.ToString();
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
            sa.ClearMeasurements();
            sa.AppendError(WindowsDependencyWait.TimeoutError(missing, timeout));

            Raise(NotificationKind.DependencyWait, eCallback.updated, WindowsDependencyWait.TimeoutText(this.Config.Notifications.InstanceName, item.Title, missing, timeout), item, missing);

            ApplyStartRules(item.ID);
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

            //The account is read before anything is announced, a service whose settings name an account that
            //cannot be used never tells anyone it was up
            var account = RunAsAccount.Of(item);

            if (account.Error != null)
            {
                Console.WriteLine("ServiceManager: " + item.Title + " could not be started: " + account.Error);

                StartFailed(item, sa, account.ErrorLine());

                return;
            }

            Raise(NotificationKind.ServiceStart, this.Config.Notifications.InstanceName + ": " + item.Title + "\r\nStatus: Started", item);

            sa.Status = ServiceAnalytics.eStatus.starting;

            WindowsUserProcess session = null;
            Process proc;

            //A manager running as LocalSystem is refused the logon of the framework, that token carries no logon
            //identifier. Such a process is created from a token of the account instead
            if (WindowsUserProcess.WantsTokenPath(account, WindowsHostAccount.IsLocalSystem()))
            {
                try
                {
                    session = WindowsUserProcess.Start(account, item.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(WindowsStartFailure.ConsoleLine(item.Title, account, ex));

                    StartFailed(item, sa, WindowsStartFailure.ErrorLine(account, ex));

                    return;
                }

                proc = session.Process;

                //The process is still standing still, so the session is registered before it can end
                lock (m_UserProcesses)
                {
                    m_UserProcesses[proc.Id] = session;
                }
            }
            else
            {
                proc = new Process();

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = item.Path;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;

                account.Apply(proc.StartInfo);
            }

            proc.EnableRaisingEvents = true;

            proc.Exited += (s, en) =>
            {

                sa.Status = ServiceAnalytics.eStatus.offline;

                RemoveRunningProcess(proc);

                Raise(eCallback.exited, item, sa);

                Raise(eCallback.updated, item, sa);

                Process_Exited(item, proc);

                closeSession(proc.Id);
            };

            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;

            try
            {
                if (session == null)
                {
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }
                else
                {
                    //The line handling is on the process before it runs, the process was created standing still
                    session.OutputLine = (line) => handleOutput(proc.Id, line);
                    session.ErrorLine = (line) => handleError(proc.Id, line);

                    session.Resume();
                }
            }
            catch (Exception ex)
            {
                //The operating system knows why the process is not running. Without that reason a wrong
                //password, an account that may not log on and a missing file all look the same
                Console.WriteLine(WindowsStartFailure.ConsoleLine(item.Title, account, ex));

                if (session != null)
                {
                    closeSession(proc.Id);
                }

                StartFailed(item, sa, WindowsStartFailure.ErrorLine(account, ex));

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

        /// <summary>
        /// Takes a service that did not start offline and tells the log, the window and the dependents about it.
        /// A service that cannot be started is not started again and again.
        /// </summary>
        private void StartFailed(ServiceItem item, ServiceAnalytics sa, String error)
        {
            item.ForceRestart = false;
            sa.AppendError(error);
            sa.ProcessID = 0;
            sa.Started = null;
            sa.Exited = DateTime.Now;
            sa.Exiting = null;
            sa.ClearMeasurements();

            sa.Status = ServiceAnalytics.eStatus.start_failed;

            //A service that never got a process leaves no output of its own, so the reason is handed to the
            //live log by hand: the window shows what the error stream already knows
            var line = error.TrimEnd();

            sa.AppendOutput(line + "\r\n");

            Raise(eCallback.logs, item.ID, line);

            Raise(eCallback.updated, item, sa);

            ApplyStartRules(item.ID);
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

            try
            {
                Process proc;

                lock (RunningProcesses)
                {

                    proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
                    if (proc == null)
                    {
                        //The manager knows no process of this service. A service it believes to be running goes
                        //offline here, a service that never started keeps the reason it never started
                        if (sa.Status == ServiceAnalytics.eStatus.running || sa.Status == ServiceAnalytics.eStatus.shutting_down)
                        {
                            sa.Status = ServiceAnalytics.eStatus.offline;
                        }

                        sa.ClearMeasurements();

                        return;
                    }

                    sa.Status = ServiceAnalytics.eStatus.shutting_down;

                    if (RunningProcesses.Contains(proc))
                        RunningProcesses.Remove(proc);

                }

                if (!HasExited(proc))
                {
                    if (IsRunning)
                    {
                        Raise(eCallback.exiting, item, sa);
                    }

                    EndProcess(item, sa, proc, 1);
                }

                sa.Status = ServiceAnalytics.eStatus.offline;
                sa.ClearMeasurements();
            }
            catch (Exception ex)
            {
                //A service that cannot be reached is offline as far as the manager is concerned. Leaving it up
                //would hold the services waiting for it on a dependency that is gone
                sa.Status = ServiceAnalytics.eStatus.offline;
                sa.ClearMeasurements();

                ReportEndFailure(item, sa, ex);
            }
        }

        /// <summary>
        /// The steps that take a process down: the enter key it might be waiting for, its main window and the
        /// process itself when neither of them was enough. Every one of them can be refused, by a pipe that is
        /// closed, by a process of another account or by a process that walked away on its own, so the steps are
        /// taken one after the other and none of them throws the end at whoever asked for it.
        /// </summary>
        /// <param name="windowFallback">Seconds to wait for the window step when the settings name no timeout.</param>
        internal void EndProcess(ServiceItem item, ServiceAnalytics sa, Process proc, int windowFallback)
        {
            if (item.Shutdown_ENTER_Send)
            {
                TryEndStep(item, sa, () => WriteInput(proc, "\r\n"));

                if (item.Shutdown_ENTER_Timeout > 0)
                {
                    Wait(item.Shutdown_ENTER_Timeout, a => HasExited(proc));
                }
            }

            if (!HasExited(proc))
            {
                TryEndStep(item, sa, () => proc.CloseMainWindow());

                if (item.Shutdown_Timeout > 0)
                {
                    Wait(item.Shutdown_Timeout, a => HasExited(proc));
                }
                else
                {
                    Wait(windowFallback, a => HasExited(proc));
                }
            }

            if (!HasExited(proc))
            {
                TryEndStep(item, sa, () => proc.Kill());
            }
        }

        /// <summary>
        /// Takes one step of the end and reports the reason when it was refused. The steps that follow are taken
        /// regardless: an enter key that does not reach the process must not leave the process running.
        /// </summary>
        private void TryEndStep(ServiceItem item, ServiceAnalytics sa, Action step)
        {
            try
            {
                step();
            }
            catch (Exception ex)
            {
                ReportEndFailure(item, sa, ex);
            }
        }

        /// <summary>
        /// Whether a process is gone. A process the framework cannot read anymore is treated as gone, there is
        /// nothing left to end.
        /// </summary>
        private static bool HasExited(Process proc)
        {
            try
            {
                return proc.HasExited;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// What a refused step of an end costs the run. The reason goes to the console, the output of the service
        /// and the live log, so a process that was reached and refused is not mistaken for one that was never
        /// reached at all.
        /// </summary>
        private void ReportEndFailure(ServiceItem item, ServiceAnalytics sa, Exception ex)
        {
            var reason = WindowsStartFailure.Reason(ex);

            Console.WriteLine("ServiceManager: " + item.Title + " could not be ended: " + reason);

            if (sa == null)
            {
                return;
            }

            var line = "SERVICE MANAGER: COULD NOT END " + item.Title + ": " + reason;

            sa.AppendError(line + "\r\n");
            sa.AppendOutput(line + "\r\n");

            Raise(eCallback.logs, item.ID, line);
        }

        /// <summary>
        /// Hands back the process of a service that ended. A process that stays in the list is looked up again by
        /// the rules of the services waiting for it, and reading it then throws over a handle that is closed.
        /// </summary>
        private void RemoveRunningProcess(Process proc)
        {
            lock (RunningProcesses)
            {
                RunningProcesses.Remove(proc);
            }
        }

        /// <summary>
        /// Restarts one service the way the window asks for it. The rules of the services that wait for it are
        /// applied here as well: a restart through the menu replaced the process, and a dependent that keeps its
        /// old one talks to something that is gone.
        /// </summary>
        public void RestartService(ServiceItem item)
        {
            if (item == null)
            {
                return;
            }

            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID);

            if (sa != null && sa.IsRunning)
            {
                sa.Exiting = DateTime.Now;
                ServiceEnd(item);
            }
            else if (sa != null && sa.Status == ServiceAnalytics.eStatus.waiting_for_dependency)
            {
                //The wait has to end first, otherwise the start below is ignored as a duplicate
                ServiceEnd(item);
            }

            //Does not restart automatically
            if (!item.ForceRestart)
            {
                StartService(item);

                //With the flag on the own exit path of the service restarts it and carries the rules from there,
                //applying them here as well would race with it.
                ApplyRestartRules(item.ID);
            }
        }

        /// <summary>
        /// Stops one service the way the window asks for it. The rules of the services that wait for it are applied
        /// as well: a dependent that keeps running talks to a service that is gone, so every one of them can be
        /// told to stay up or to go with its dependency.
        /// </summary>
        public void StopService(ServiceItem item)
        {
            if (item == null)
            {
                return;
            }

            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID);

            //A service that is only waiting has no process, ServiceEnd ends the wait for it
            if (sa != null && sa.IsRunning)
            {
                sa.Exiting = DateTime.Now;
            }

            ServiceEnd(item);

            ApplyStopRules(item.ID);
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
                    WriteInput(proc, input);
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
            if (proc == null)
                return;

            handleError(proc.Id, e.Data);
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Process proc = sender as Process;
            if (proc == null)
                return;

            handleOutput(proc.Id, e.Data);
        }

        /// <summary>
        /// What the error stream of a process reported, wherever the line came from. The framework hands it in
        /// through an event, a process started under another account by token hands it in from its pipe.
        /// </summary>
        private void handleError(int processId, String data)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == processId) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            var s = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (s == null)
                return;

            Raise(NotificationKind.ConsoleError, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\nError:\r\n" + data, s, data);

            sa.AppendError((data ?? "") + "\r\n");
        }

        /// <summary>
        /// What the output stream of a process reported, wherever the line came from.
        /// </summary>
        private void handleOutput(int processId, String data)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == processId) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            var s = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
            if (s == null)
                return;

            Raise(NotificationKind.ConsoleOutput, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\n" + data, s, data);

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

                sa.AppendOutput(data + "\r\n");
            }

            if (Connection.ServerChannel == null || Connection.Failed)
                return;

            //The ping has to reach the client in front of the line that follows it. Both travel through the
            //client queue, which keeps that order and keeps the output thread off the WCF call.
            if (DateTime.Now.Subtract(sa.LastPing).TotalSeconds > 10)
            {
                sa.LastPing = DateTime.Now;

                Raise(eCallback.ping, s, sa);
            }

            if (LiveLogs.Contains(sa.ServiceID))
            {
                Raise(eCallback.logs, s, sa, data);
            }
        }

        /// <summary>
        /// Writes one line to the standard input of a process. A process started under another account by token
        /// has no stream of the framework, its input travels through a pipe owned by the session.
        /// </summary>
        private void WriteInput(Process proc, String line)
        {
            WindowsUserProcess session;

            lock (m_UserProcesses)
            {
                m_UserProcesses.TryGetValue(proc.Id, out session);
            }

            if (session != null)
            {
                session.WriteInput(line);

                return;
            }

            proc.StandardInput.WriteLine(line);
        }

        /// <summary>
        /// Hands back the pipes of a process that is gone. A process of the framework owns no session, the call
        /// then costs nothing.
        /// </summary>
        private void closeSession(int processId)
        {
            WindowsUserProcess session;

            lock (m_UserProcesses)
            {
                if (!m_UserProcesses.TryGetValue(processId, out session))
                {
                    return;
                }

                m_UserProcesses.Remove(processId);
            }

            session.Dispose();
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

        /// <summary>
        /// The process of a service ended. It is internal because a test cannot hand over a process that walked
        /// away on its own, it has to call this handler itself.
        /// </summary>
        internal void Process_Exited(ServiceItem service, Process proc)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);
            if (sa != null)
            {
                sa.Exited = DateTime.Now;

                //The process is gone, and so is everything that was measured on it
                sa.ClearMeasurements();
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

                //A service that ends on its own leaves the services waiting for it behind with a dependency that
                //is gone, so their stop rule decides whether they go with it. A stop the manager asked for
                //carries its own rules, and the shutdown of the instance follows the stop levels instead.
                if (kind == NotificationKind.ServiceCrashed && this.IsRunning)
                {
                    ApplyStopRules(s.ID);
                }

                return;
            }


            sa.Restarts++;

            StartService(service, service.Wait_On_Restart);

            Raise(NotificationKind.ServiceRestart, this.Config.Notifications.InstanceName + ": " + s.Title + "\r\nStatus: Restarted", s);

            ApplyRestartRules(service.ID);
        }

        /// <summary>
        /// Applies the restart rule of every service that depends on the given one.
        /// </summary>
        private void ApplyRestartRules(Guid id)
        {
            if (this.Config == null)
            {
                return;
            }

            ApplyRestartRules(new ServiceDependencyGraph(this.Config.ServiceList), id, new HashSet<Guid> { id });
        }

        /// <summary>
        /// The services already visited are skipped, that is what keeps a circle in the settings file from
        /// running around in circles.
        /// </summary>
        private void ApplyRestartRules(ServiceDependencyGraph graph, Guid id, HashSet<Guid> visited)
        {
            foreach (var dependent in graph.DependentsOf(id))
            {
                if (!visited.Add(dependent.ID))
                {
                    continue;
                }

                switch (dependent.DependencyRestartRule)
                {
                    case ServiceItem.eDependencyRestartRule.Restart:
                        ServiceEnd(dependent);

                        //A service that restarts on its own is left to its own restart path, which carries
                        //the rules further down from there. Restarting it here as well would race with it.
                        if (!dependent.ForceRestart)
                        {
                            StartService(dependent, dependent.Wait_On_Restart);

                            Raise(NotificationKind.ServiceRestart, this.Config.Notifications.InstanceName + ": " + dependent.Title + "\r\nStatus: Restarted with a dependency", dependent);

                            ApplyRestartRules(graph, dependent.ID, visited);
                        }
                        break;

                    case ServiceItem.eDependencyRestartRule.Stop:
                        //Without the flag the own restart path of the service would bring it straight back up
                        dependent.ForceRestart = false;

                        ServiceEnd(dependent);

                        Raise(NotificationKind.ServiceStop, this.Config.Notifications.InstanceName + ": " + dependent.Title + "\r\nStatus: Stopped with a dependency", dependent);

                        ApplyRestartRules(graph, dependent.ID, visited);
                        break;
                }
            }
        }

        /// <summary>
        /// Takes the services that must not start without the given one offline.
        /// </summary>
        private void ApplyStartRules(Guid id)
        {
            if (this.Config == null)
            {
                return;
            }

            ApplyStartRules(new ServiceDependencyGraph(this.Config.ServiceList), id, new HashSet<Guid> { id });
        }

        /// <summary>
        /// A service that starts anyway is left alone, and a service below a service that was taken offline
        /// is taken offline as well. The visited set is what keeps a circle from running forever.
        /// </summary>
        private void ApplyStartRules(ServiceDependencyGraph graph, Guid id, HashSet<Guid> visited)
        {
            foreach (var dependent in graph.DependentsOf(id))
            {
                if (dependent.DependencyStartRule != ServiceItem.eDependencyStartRule.DoNotStart)
                {
                    continue;
                }

                if (!visited.Add(dependent.ID))
                {
                    continue;
                }

                dependent.ForceRestart = false;

                ServiceEnd(dependent);

                Raise(NotificationKind.DependencyWait, this.Config.Notifications.InstanceName + ": " + dependent.Title + "\r\nStatus: Not started, " + TitleOf(id) + " did not start", dependent);

                ApplyStartRules(graph, dependent.ID, visited);
            }
        }

        /// <summary>
        /// Applies the stop rule of every service that depends on the given one.
        /// </summary>
        private void ApplyStopRules(Guid id)
        {
            if (this.Config == null)
            {
                return;
            }

            ApplyStopRules(new ServiceDependencyGraph(this.Config.ServiceList), id, new HashSet<Guid> { id });
        }

        /// <summary>
        /// A dependent that is meant to stay running is left alone, and the rule carries further down: a service
        /// below a service that was stopped goes the same way. The visited set is what keeps a circle from
        /// running forever.
        /// </summary>
        private void ApplyStopRules(ServiceDependencyGraph graph, Guid id, HashSet<Guid> visited)
        {
            foreach (var dependent in graph.DependentsOf(id))
            {
                if (dependent.DependencyStopRule != ServiceItem.eDependencyStopRule.Stop)
                {
                    continue;
                }

                if (!visited.Add(dependent.ID))
                {
                    continue;
                }

                //Without the flag the own restart path of the service would bring it straight back up
                dependent.ForceRestart = false;

                ServiceEnd(dependent);

                Raise(NotificationKind.ServiceStop, this.Config.Notifications.InstanceName + ": " + dependent.Title + "\r\nStatus: Stopped with a dependency", dependent);

                ApplyStopRules(graph, dependent.ID, visited);
            }
        }

        public void Shutdown()
        {

        }

        public void Stop()
        {
            Raise(NotificationKind.ServerShutdown, this.Config.Notifications.InstanceName + ": Server shutting down");

            this.Connection?.Try(a => a.ShutdownStarted());

            this.IsRunning = false;

            var graph = new ServiceDependencyGraph(this.Config.ServiceList);
            var running = this.Config.ServiceList.Where(a => HasLiveProcess(a)).ToList();

            //The services nothing else depends on go down first, then their parents. Everything inside one
            //level is stopped at the same time.
            foreach (var level in graph.StopLevels(running))
            {
                List<Task> Exits = new List<Task>();

                foreach (var service in level)
                {
                    var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);
                    if (sa == null)
                        continue;

                    sa.Exiting = DateTime.Now;

                    StopRequests.Mark(service.ID);

                    var ex = new Task(() => StopRunningService(service, sa));

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
            }

            //The pushes of the services that just ended have to reach the client in front of the handshake.
            Events.Flush();

            Thread.Sleep(1000);

            this.Connection?.Try(a => a.ShutdownCompleted());

            Thread.Sleep(1000);


            this.Connection?.Stop();


            Raise(NotificationKind.ServerShutdown, this.Config.Notifications.InstanceName + ": Server shutted down");

            Events.Flush();

            Events.Complete();
        }

        /// <summary>True when the analytics know a process of the service that is still alive.</summary>
        private bool HasLiveProcess(ServiceItem service)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);

            return sa != null && sa.IsRunning;
        }

        /// <summary>
        /// Brings one service down: the enter key it might be waiting for, its main window, and the process
        /// itself when neither of them was enough.
        /// </summary>
        private void StopRunningService(ServiceItem service, ServiceAnalytics sa)
        {
            Process proc;

            lock (RunningProcesses)
            {
                proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
            }

            if (proc == null)
                return;

            sa.Status = ServiceAnalytics.eStatus.shutting_down;

            Raise(eCallback.exiting, service, sa);

            EndProcess(service, sa, proc, 10);

            sa.Status = ServiceAnalytics.eStatus.offline;
            sa.ClearMeasurements();

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


        /// <summary>
        /// Puts an event on the bus that only the connected clients are told about. A WCF operation that
        /// changed something on its own raises it here instead of pushing over the channel itself.
        /// </summary>
        public void Raise(eCallback callback, ServiceItem item, ServiceAnalytics sa, String details = null)
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
        /// Puts an event on the bus that only the connected clients are told about, for a service the caller
        /// names by its id. A log line has the id at hand and no reason to look the item up.
        /// </summary>
        public void Raise(eCallback callback, Guid serviceId, String details = null)
        {
            Events.Publish(new ServiceEvent()
            {
                Callback = callback,
                ServiceId = serviceId,
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
                    Connection?.Try(a => a.LiveLogsUpdate(e.ServiceId, e.Details));
                    break;

                case eCallback.ping:
                    Connection?.Try(a => a.ActivityPing(e.ServiceId));
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
