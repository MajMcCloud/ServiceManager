using ServiceManager.Base.data;
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


        Channel Connection { get; set; }

        public List<Guid> LiveLogs { get; set; }

        public List<Process> RunningProcesses { get; set; }


        public ServiceManager()
        {
            LiveLogs = new List<Guid>();

            RunningProcesses = new List<Process>();
        }


        public void Start()
        {
            var cfg = ServiceConfig.load();
            Config = cfg;

            this.IsRunning = true;

            Analytics = new Analytics();

            switch (this.Config.RuntimeSettings.Mode)
            {
                case data.settings.Runtime.eMode.NetPipes:
                    Connection = new Channel(true, "serviceman" + Process.GetCurrentProcess().Id.ToString());
                    break;

                case data.settings.Runtime.eMode.Tcp:
                    Connection = new Channel(true, "localhost", this.Config.RuntimeSettings.TCPPort);
                    break;

                default:

                    throw new ArgumentOutOfRangeException(nameof(this.Config.RuntimeSettings.Mode));
            }

            try
            {
                Connection.Start();
            }
            catch
            {
                return;
            }



            Connection.Server.Manager = this;

            foreach (var c in cfg.ServiceList)
            {
                PrepareService(c);

                //Don't start
                if (!c.Enabled)
                    continue;

                ProcessStart(c);
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
        }

        /// <summary>
        /// Starts the service and add some event hooks.
        /// </summary>
        /// <param name="item"></param>
        public void StartService(ServiceItem item)
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

                Connection?.Try(a => a.ServiceExited(item.ID, item, sa));

                Connection?.Try(a => a.ServiceUpdated(item.ID, item, sa));

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
                sa.Error += "SERVICE MANAGER: START FAILED\r\n";
                sa.ProcessID = 0;
                sa.Started = null;
                sa.Exited = DateTime.Now;
                sa.Exiting = null;

                sa.Status = ServiceAnalytics.eStatus.start_failed;

                Connection?.Try(a => a.ServiceUpdated(item.ID, item, sa));

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

            Connection?.Try(a => a.ServiceStarted(item.ID, item, sa));
        }

        public void ServiceEnd(ServiceItem item)
        {
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == item.ID) ?? new ServiceAnalytics();

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
                Connection?.Try(a => a.ServiceExiting(item.ID, item, sa));
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
                sa.Output += input + "\r\n";

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
            Process proc = sender as Process;
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == proc.Id) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            if (e.Data == null)
                return;

            sa.Error += (e.Data ?? "") + "\r\n";
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            Process proc = sender as Process;
            var sa = this.Analytics.Services.FirstOrDefault(a => a.ProcessID == proc.Id) ?? new ServiceAnalytics();
            if (sa == null)
                return;

            if (e.Data == null)
                return;

            //Save to harddrive
            if (sa.LastSaved.Date != DateTime.Today)
            {
                var service = this.Config.ServiceList.FirstOrDefault(a => a.ID == sa.ServiceID);
                if (service.LogConsoleOutput && SaveToLogs(service, sa.Output))
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

            sa.Output += e.Data + "\r\n";

            if (Connection.ServerChannel == null || Connection.Failed)
                return;


            var t = new Task(() =>
            {
                if (DateTime.Now.Subtract(sa.LastPing).TotalSeconds > 5)
                {
                    Connection?.Try(a => a.ActivityPing(sa.ServiceID));
                    sa.LastPing = DateTime.Now;
                }


                if (LiveLogs.Contains(sa.ServiceID))
                {
                    Connection?.Try(a => a.LiveLogsUpdate(sa.ServiceID, e.Data));
                }

            });

            t.Start();
        }

        public bool SaveToLogs(ServiceItem service, String content)
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

            sa.Status = ServiceAnalytics.eStatus.offline;



            //Save to harddrive
            if (service.LogConsoleOutput && SaveToLogs(service, sa.Output))
            {
                sa.Output = "";
                sa.LastSaved = DateTime.Now;
            }

            //if (LiveLogs.Contains(sa.ServiceID))
            //{
            //    Connection.ServerChannel.LiveLogsUpdate(sa.ServiceID, "SERVICE MANAGER: Service has been ended");
            //}

            if (!service.ForceRestart)
                return;

            if (!this.IsRunning)
                return;

            sa.Restarts++;

            StartService(service);
        }

        public void Shutdown()
        {

        }

        public void Stop()
        {

            this.Connection?.Try(a => a.ShutdownStarted());

            this.IsRunning = false;


            List<Task> Exits = new List<Task>();


            foreach (var service in this.Config.ServiceList)
            {
                var sa = this.Analytics.Services.FirstOrDefault(a => a.ServiceID == service.ID);
                if (sa == null || !sa.IsRunning)
                    continue;

                sa.Exiting = DateTime.Now;

                var ex = new Task(() =>
                {
                    var proc = RunningProcesses.FirstOrDefault(a => a.Id == sa.ProcessID);
                    if (proc == null)
                        return;

                    sa.Status = ServiceAnalytics.eStatus.shutting_down;
                    var start = DateTime.Now;

                    this.Connection?.Try(a => a.ServiceExiting(service.ID, service, sa));

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

                    Connection?.Try(a => a.ServiceExited(service.ID, service, sa));

                    sa.Exited = DateTime.Now;

                    //Save to harddrive
                    if (sa.LastSaved.Date != DateTime.Today)
                    {
                        if (service.LogConsoleOutput && SaveToLogs(service, sa.Output))
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
