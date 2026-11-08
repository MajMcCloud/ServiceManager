using ServiceManager.Base;
using ServiceManager.Base.data;
using ServiceManager.Base.wcf;
using ServiceManager.Base.wcf.models;
using ServiceManager.UI.forms;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Verifide.ServiceUtils;
using static ServiceManager.Base.data.ServiceAnalytics;

namespace ServiceManager.UI
{
    public partial class frmMain : Form
    {
        public Base.ServiceManager Manager { get; set; }


        public int WatchingProcessID { get; set; }

        public Channel WatchConnection { get; set; }

        public List<ServiceItem> Services { get; set; }

        private static object _sync = new object();

        public frmMain()
        {
            InitializeComponent();

            var rowImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            rowImages.Images.Add("led_on", new Bitmap(Properties.Resources.led_on, 16, 16));
            lsvServices.SmallImageList = rowImages;
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            //this.WatchingProcessID = Process.GetCurrentProcess().Id;

            tmRefresh_Tick(sender, e);


            if (Program.Parameters.ContainsKey("-c"))
            {
                StartWatching(Program.Parameters["host"], int.Parse(Program.Parameters["port"]));
            }

            var version = Application.ProductVersion;
            lblVersion.Text = $"Version {version}";
        }


        private void lsvServices_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var dataItem = lsvServices.HitTest(e.Location)?.Item;

            if (dataItem == null) return;

            dataItem.Selected = true;

            if (dataItem.SubItems[3].Text == "running")
            {
                tsmiMenu_Restart.Text = "Restart service";
            }
            else
            {
                tsmiMenu_Restart.Text = "Start service";
            }


            cmsMenu.Show(lsvServices.PointToScreen(e.Location));
        }

        private void lsvServices_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (lsvServices.HitTest(e.Location)?.Item == null)
                return;

            tsmiMenu_Livelogs_Click(sender, e);
        }

        private void rmServiceRefresh_Tick(object sender, EventArgs e)
        {
            //Check service
            ServiceController[] services = ServiceController.GetServices();
            ServiceController sc = services.FirstOrDefault(a => a.ServiceName == Program.ServiceName);

            if (sc != null)
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    var b = new Bitmap(Properties.Resources.led_on, 16, 16);
                    rmiService.Image = b;
                }

                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    var b = new Bitmap(Properties.Resources.led_off, 16, 16);
                    rmiService.Image = b;
                }
            }
            else
            {
                rmiService.Image = null;

                try
                {
                    var p = Process.GetProcessById(WatchingProcessID);
                }
                catch
                {
                    lsvServices.Items.Clear();
                }

            }

        }

        private void tmRefresh_Tick(object sender, EventArgs e)
        {
            //Continue when watching
            if (this.WatchConnection == null)
                return;

            WatchConnection.Async(a => a.GetServicesAnalytics(), b => UpdateServiceList(b));
        }

        private void UpdateServiceList(GetServicesAnalyticsResponse analytics)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() =>
                {
                    UpdateServiceList(analytics);
                }));
                return;
            }

            lock (_sync)
            {
                var selected = lsvServices.SelectedItems.Cast<ListViewItem>().FirstOrDefault();

                lsvServices.BeginUpdate();
                lsvServices.Items.Clear();

                int active = 0;

                this.Services = analytics.Services;

                foreach (var item in analytics.Services)
                {
                    var a = analytics.Analytics.FirstOrDefault(b => b.ServiceID == item.ID);
                    if (a == null)
                        continue;

                    var itm = new ListViewItem("");
                    if (!a.IsRunning)
                    {
                        itm.ImageIndex = -1;
                    }

                    itm.SubItems.Add((a.IsRunning ? a.ProcessID.ToString() : "-"));
                    itm.SubItems.Add(item.Title);
                    itm.Tag = item.ID;
                    lsvServices.Items.Add(itm);

                    if (a.IsRunning)
                        active++;

                    //if (a.IsRunning && a.Exiting != null)
                    //{
                    //    itm.SubItems.Add("shutting down");
                    //}
                    //else
                    //{
                    //    itm.SubItems.Add(a.IsRunning ? "running" : "offline");
                    //}

                    itm.SubItems.Add(a.Status.ToString());

                    itm.SubItems.Add(a.Restarts.ToString());
                    itm.SubItems.Add(item.ForceRestart ? "yes" : "no");

                }

                if (selected != null)
                {
                    var item = lsvServices.Items.Cast<ListViewItem>().FirstOrDefault(a => a.Tag.ToString() == selected.Tag.ToString());
                    if (item != null)
                        item.Selected = true;
                }

                lblActiveServiceCount.Text = active.ToString();

                lsvServices.EndUpdate();
            }
        }

        private void loadConnections()
        {
            var connections = ServiceManager.Base.ServiceManager.GetProcesses();

            while (rmiInstances.DropDownItems.Count > 2)
            {
                rmiInstances.DropDownItems.RemoveAt(2);
            }

            ToolStripMenuItem item = null;
            if (connections.Count == 0)
            {
                item = new ToolStripMenuItem("There are no running instances.");
                rmiInstances.DropDownItems.Add(item);
                return;
            }

            foreach (var c in connections)
            {


                if (c.Id == Process.GetCurrentProcess().Id)
                {
                    item = new ToolStripMenuItem("PID " + c.Id.ToString() + " (this application)");
                    item.Click += (s, en) =>
                    {
                        MessageBox.Show("Start Test instead of connecting directly.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };
                }
                else
                {


                    if (NetPipesTest(c.Id))
                    {
                        item = new ToolStripMenuItem("PID " + c.Id.ToString() + " (NetPipes)");
                        item.CheckOnClick = true;
                        item.Tag = new { id = c.Id, type = "netpipes" };
                        item.Image = Properties.Resources.rohr_24px;
                    }
                    else
                    {
                        item = new ToolStripMenuItem("IP " + c.Id.ToString() + " (TCP)");
                        item.CheckOnClick = true;
                        item.Tag = new { id = c.Id, type = "tcp" };
                        item.Image = Properties.Resources.antenne_24px;
                    }
                }


                if (c.Id == this.WatchingProcessID)
                {
                    item.Checked = true;
                }

                item.CheckOnClick = true;
                item.CheckedChanged += Item_CheckedChanged;
                //item.CheckStateChanged += Item_CheckStateChanged;

                rmiInstances.DropDownItems.Add(item);
            }
        }

        private void rmiInstances_ConnectTo_Click(object sender, EventArgs e)
        {
            frmConnectTo frm = new frmConnectTo();

            if (frm.ShowDialog() != DialogResult.OK)
                return;



            StartWatching(frm.IPOrHost, frm.Port);
        }

        private void Item_CheckedChanged(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            dynamic t = item.Tag;

            var processId = t.id;
            var type = t.type;

            if (!item.Checked)
            {
                if (processId == WatchingProcessID)
                {
                    if (MessageBox.Show("Disconnect ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        item.Checked = true;
                        return;
                    }

                    WatchConnection.Stop();

                    lsvServices.BeginUpdate();
                    lsvServices.Items.Clear();
                    lsvServices.EndUpdate();
                }

                return;
            }


            if (processId == Process.GetCurrentProcess().Id)
            {
                item.Checked = false;
                return;
            }

            if (type == "netpipes")
            {
                StartWatching(processId);

            }
            else if (type == "tcp")
            {
                StartWatching("localhost", 50000);
            }
        }

        private void Item_CheckStateChanged(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            dynamic t = item.Tag;

            var processId = t.id;
            var type = t.type;

            if (!item.Checked)
            {
                if (processId == WatchingProcessID)
                {
                    if (MessageBox.Show("Disconnect ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        item.Checked = true;
                        return;
                    }

                    WatchConnection.Stop();

                    lsvServices.BeginUpdate();
                    lsvServices.Items.Clear();
                    lsvServices.EndUpdate();
                }

                return;
            }


            if (processId == Process.GetCurrentProcess().Id)
            {
                item.Checked = false;
                return;
            }

            if (type == "netpipes")
            {
                StartWatching(processId);

            }
            else if (type == "tcp")
            {
                StartWatching("localhost", 50000);
            }


        }


        private bool NetPipesTest(int processId)
        {
            return Channel.ProbePipes("localhost", "serviceman" + processId);
        }

        private bool NetTcpTest(String HostOrIP, int Port)
        {
            return Channel.ProbeTcp(HostOrIP, Port);
        }

        private void StartWatching(String IPOrHost, int port)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    StartWatching(IPOrHost, port);
                }));
                return;
            }

            //this.WatchingProcessID = processId;
            WatchConnection = new Channel(false, IPOrHost, port);

            tmRefresh.Enabled = true;
            tmPing.Enabled = true;

            WatchConnection.Connected += (s, en) =>
            {
                lblStatus.Text = "Connected";
                lblStatus.Text += $" (via TCP - {IPOrHost}:{port})";
                //tmRefresh.Enabled = true;

                lblStatus.ForeColor = Color.Green;
            };

            PrepareWatcher();
        }

        private void PrepareWatcher()
        {
            WatchConnection.Disconnected += (s, en) =>
            {
                WatchConnection = null;
                WatchingProcessID = 0;

                lblStatus.Text = "Disconnected";
                lblStatus.ForeColor = Color.Black;
                lblActiveServiceCount.Text = "0";
                //tmRefresh.Enabled = false;

                rmiServices.Enabled = false;

                tmRefresh.Enabled = false;
                tmPing.Enabled = false;
            };

            WatchConnection.Faulted += (s, en) =>
            {
                WatchConnection = null;
                WatchingProcessID = 0;

                lblStatus.Text = "Failed";
                lblStatus.ForeColor = Color.Red;
                lblActiveServiceCount.Text = "0";

                rmiServices.Enabled = false;
                tmRefresh.Enabled = false;
                tmPing.Enabled = false;

                MessageBox.Show("The connection to the service failed.\r\n\r\nThe service only accepts connections from members of the Administrators group. Start the application with \"Run as administrator\".",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            };

            WatchConnection.Start();

            if (WatchConnection == null)
            {
                return;
            }

            WatchConnection.ClientCallback.ServerShutdownStarted += (s, en) =>
            {

                //tmRefresh.Enabled = false;

            };

            WatchConnection.ClientCallback.ServerShutdownCompleted += (s, en) =>
            {
                if (this.WatchConnection != null)
                {
                    this.WatchConnection.Stop();
                    this.WatchConnection = null;
                }

                this.WatchingProcessID = 0;

                this.Manager = null;

                lsvServices.BeginUpdate();
                lsvServices.Items.Clear();
                lsvServices.EndUpdate();

                tmRefresh.Enabled = false;
                tmPing.Enabled = false;
            };

            WatchConnection.ClientCallback.ServiceChanged += (s, en) =>
            {
                var item = lsvServices.Items.Cast<ListViewItem>().FirstOrDefault(a => (Guid)a.Tag == en.ServiceId);
                if (item == null)
                    return;

                if (en.Status == ServiceAnalytics.eStatus.offline)
                {
                    item.ImageIndex = -1;
                }



                switch (en.Status)
                {
                    case eStatus.offline:

                        item.SubItems[1].Text = "-";
                        item.SubItems[3].Text = "offline";

                        break;

                    case eStatus.shutting_down:

                        //item.SubItems[1].Text = en.PID.ToString();
                        item.SubItems[3].Text = "shutting down";

                        break;

                    case eStatus.running:

                        item.SubItems[1].Text = en.PID.ToString();
                        item.SubItems[3].Text = "running";

                        break;

                    case eStatus.updated:


                        break;

                    default:

                        item.SubItems[3].Text = en.Status.ToString();

                        break;

                }

                item.SubItems[4].Text = en.Restarts.ToString();
                item.SubItems[5].Text = (en.AutoRestart ? "yes" : "no");

            };

            WatchConnection.ClientCallback.ServiceActivityPing += (s, en) =>
            {

                lsvServices.BeginUpdate();

                foreach (ListViewItem lvi in lsvServices.Items)
                {
                    if (lvi.Tag.ToString() != en.ServiceId.ToString())
                        continue;

                    //Graphics.FromImage(bmp).FillRectangle(Brushes.Black, 0, 0, 16, 16);

                    lvi.ImageKey = "led_on";

                }

                lsvServices.EndUpdate();

            };

            rmiServices.Enabled = true;


            tmRefresh_Tick(null, null);
        }

        private void StartWatching(int processId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                    StartWatching(processId);
                }));
                return;
            }

            this.WatchingProcessID = processId;


            WatchConnection = new Channel(false, "serviceman" + this.WatchingProcessID);

            tmRefresh.Enabled = true;
            tmPing.Enabled = true;

            WatchConnection.Connected += (s, en) =>
            {
                lblStatus.Text = "Connected";

                lblStatus.Text += $" (via NetPipes - {"serviceman" + this.WatchingProcessID})";
                //tmRefresh.Enabled = true;

                lblStatus.ForeColor = Color.Green;
            };

            PrepareWatcher();
        }

        private void rmiTesting_Start_Click(object sender, EventArgs e)
        {
            rmiTesting_Start.Enabled = false;
            rmiTesting_End.Enabled = true;


            var cfg = ServiceConfig.load();

            if (cfg == null)
            {
                MessageBox.Show("No service config found.");
                return;
            }


            this.Manager = new ServiceManager.Base.ServiceManager();

            this.Manager.Start();

            

            switch (cfg.RuntimeSettings.Mode)
            {
                case ServiceManager.Base.data.settings.Runtime.eMode.NetPipes:

                    StartWatching(Process.GetCurrentProcess().Id);

                    break;
                case ServiceManager.Base.data.settings.Runtime.eMode.Tcp:

                    StartWatching("localhost", cfg.RuntimeSettings.TCPPort);

                    break;
                default:
                    return;

            }



            this.BringToFront();
        }

        private void rmiTesting_End_Click(object sender, EventArgs e)
        {
            rmiTesting_Start.Enabled = true;
            rmiTesting_End.Enabled = false;

            if (this.Manager == null)
            {
                return;
            }

            var t = new Task(() =>
            {
                try
                {
                    this.Manager.Shutdown();

                    this.Manager.Stop();
                }
                catch
                {

                }
            });

            t.Start();



            //MessageBox.Show("Test will be finished.", "End", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Manager == null)
                return;

            if (MessageBox.Show("Test is still runnning. Close anyway?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            rmiTesting_End_Click(sender, e);

        }

        #region "Service"


        private void rmiService_Install_Click(object sender, EventArgs e)
        {
            try
            {
                InstallContext install = new InstallContext();
                ServiceProcessInstaller spi = new ServiceProcessInstaller();
                ServiceInstallerEx svi = new ServiceInstallerEx();


                String name = Application.StartupPath + "\\ServiceManager.exe";

                String path = String.Format("/assemblypath=\"{0}\"", name);
                String[] cmdline = { path };

                install = new InstallContext("", cmdline);

                install.Parameters["assemblypath"] += " -s";

                spi.Account = ServiceAccount.LocalSystem;
                svi.StartType = ServiceStartMode.Automatic;

                svi.ServiceName = Program.ServiceName;
                svi.DisplayName = Program.ServiceTitle;
                svi.Description = "Runs and protecting specific applications.";
                svi.Context = install;

                //Nicht verzögern, also so früh wie möglich starten
                svi.DelayedAutoStart = false;
                svi.Parent = spi;

                //Internet wird benötigt
                svi.ServicesDependedOn = new string[] { "Tcpip", "Dhcp", "Dnscache" };

                svi.FailCountResetTime = 60 * 60 * 24;
                svi.FailRebootMsg = "Whitney Houston! We have a problem";
                svi.FailureActions.Add(new FailureAction(RecoverAction.Restart, 60000));
                svi.FailureActions.Add(new FailureAction(RecoverAction.Restart, 60000));
                svi.FailureActions.Add(new FailureAction(RecoverAction.Restart, 60000));

                //svi.StartOnInstall = true;

                System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
                svi.Install(state);

                svi.UpdateServiceConfig(null, null);

                tmRefresh_Tick(null, null);

                rmServiceRefresh_Tick(null, null);

                MessageBox.Show("Service installed.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Service can not be installed.\r\n" + ex.Message, "Error on installation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void rmiService_Uninstall_Click(object sender, EventArgs e)
        {
            try
            {
                ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                InstallContext Context = new InstallContext("", null);
                ServiceInstallerObj.Context = Context;
                ServiceInstallerObj.ServiceName = Program.ServiceName;
                ServiceInstallerObj.Uninstall(null);

                tmRefresh_Tick(null, null);

                rmServiceRefresh_Tick(null, null);

                MessageBox.Show("Service uninstalled.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Service can not be uninstalled.\r\n" + ex.Message, "Error on unistall", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void rmiService_Start_Click(object sender, EventArgs e)
        {
            ServiceController[] services = ServiceController.GetServices();

            bool connect = false;

            try
            {
                ServiceController sc = services.FirstOrDefault(a => a.ServiceName == Program.ServiceName);
                if (sc == null) return;


                sc.Start();

                if (MessageBox.Show("Connect to service ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    connect = true;
                }

                if (connect)
                {
                    sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 30));

                    ManagementObject service = new ManagementObject(@"Win32_service.Name='" + Program.ServiceName + "'");
                    object o = service.GetPropertyValue("ProcessId");
                    int processId = (int)((UInt32)o);

                    if (NetPipesTest(processId))
                    {
                        StartWatching(processId);
                    }
                    else
                    {
                        StartWatching("localhost", 50000);
                    }
                }

                tmRefresh_Tick(null, null);

            }
            catch
            {

            }
        }

        private void rmiService_Restart_Click(object sender, EventArgs e)
        {
            ServiceController[] services = ServiceController.GetServices();

            try
            {
                ServiceController sc = services.FirstOrDefault(a => a.ServiceName == Program.ServiceName);
                if (sc == null) return;

                ManagementObject service = new ManagementObject(@"Win32_service.Name='" + Program.ServiceName + "'");
                object o = service.GetPropertyValue("ProcessId");
                int processId = (int)((UInt32)o);

                sc.Stop();
                sc.Start();

                tmRefresh_Tick(null, null);
            }
            catch
            {

            }
        }

        private void rmiService_Stop_Click(object sender, EventArgs e)
        {
            ServiceController[] services = ServiceController.GetServices();
            int processId = 0;

            try
            {
                ServiceController sc = services.FirstOrDefault(a => a.ServiceName == Program.ServiceName);
                if (sc == null) return;

                ManagementObject service = new ManagementObject(@"Win32_service.Name='" + Program.ServiceName + "'");
                object o = service.GetPropertyValue("ProcessId");
                processId = (int)((UInt32)o);

                sc.Stop();

                tmRefresh_Tick(null, null);
            }
            catch (InvalidOperationException ex)
            {

                if (MessageBox.Show("Service can not be stopped. Kill process ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;


                Process p = Process.GetProcessById(processId);
                if (p != null)
                    p.Kill();


            }
            catch
            {
                MessageBox.Show("Can not be stopped.");
            }
        }


        private void rmiInstances_DropDownOpening(object sender, EventArgs e)
        {
            loadConnections();
        }

        private void rmiService_DropDownOpening(object sender, EventArgs e)
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController sc = services.FirstOrDefault(a => a.ServiceName == Program.ServiceName);

            if (sc == null)
            {
                rmiService_Restart.Enabled = false;
                rmiService_Start.Enabled = false;
                rmiService_Stop.Enabled = false;

                rmiService_Install.Enabled = true;
                rmiService_Uninstall.Enabled = false;
                return;
            }

            tmRefresh_Tick(null, null);

            rmiService_Install.Enabled = false;
            rmiService_Uninstall.Enabled = true;

            if (sc.Status == ServiceControllerStatus.Running)
            {
                rmiService_Restart.Enabled = true;
                rmiService_Start.Enabled = false;
                rmiService_Stop.Enabled = true;
            }

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                rmiService_Restart.Enabled = false;
                rmiService_Start.Enabled = true;
                rmiService_Stop.Enabled = false;
            }
        }


        #endregion


        private void rmiManager_OpenConfig_Click(object sender, EventArgs e)
        {
            var frm = new frmManage();
            frm.ShowDialog();
        }

        private void rmiManager_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void tsmiMenu_Restart_Click(object sender, EventArgs e)
        {
            cmsMenu.Close();

            if (this.WatchConnection == null)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.SelectedItems)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    if (item.SubItems[3].Text == "running")
                    {
                        if (MessageBox.Show($"Restart service {item.SubItems[1].Text} ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            return;
                    }

                    this.WatchConnection.TryCatch(a => a.RestartService(id));
                }
            }
        }

        private void tsmiMenu_Shutdown_Click(object sender, EventArgs e)
        {
            cmsMenu.Close();

            if (this.WatchConnection == null)
                return;

            if (MessageBox.Show("Shutdown service?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.SelectedItems)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    this.WatchConnection.TryCatch(a => a.ShutdownService(id));
                }

            }
        }


        private void tsmiMenu_Logs_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.SelectedItems.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.SelectedItems)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    frmLogs frm = new frmLogs();
                    frm.LiveLogs = false;
                    frm.serviceId = id;
                    frm.Connection = this.WatchConnection;
                    frm.Show();
                }
            }


        }

        private void tsmiMenu_Livelogs_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.SelectedItems.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.SelectedItems)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    frmLogs frm = new frmLogs();
                    frm.LiveLogs = true;
                    frm.serviceId = id;
                    frm.Connection = this.WatchConnection;
                    frm.Show();
                }

            }
        }

        private void tsmiMenu_ToggleAutoRestart_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.SelectedItems.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.SelectedItems)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    this.WatchConnection.TryCatch(a => a.ToggleAutoRestart(id));
                }

            }

        }


        private void tsmiMenu_OpenLogs_Click(object sender, EventArgs e)
        {

            lock (_sync)
            {
                if (lsvServices.SelectedItems.Count == 0)
                    return;

                var id = (Guid)lsvServices.SelectedItems[0].Tag;

                var path = Application.StartupPath + "\\logs\\" + id.ToString() + "\\";


                Process.Start("explorer.exe", path);
            }

        }

        private void rmiServices_RefreshAll_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            WatchConnection.Async(a => a.GetServicesAnalytics(), b => UpdateServiceList(b));
        }

        private void tsmiMenu_OpenExplorer_Click(object sender, EventArgs e)
        {

            if (this.WatchConnection == null)
                return;

            if (WatchConnection.Mode == Base.data.settings.Runtime.eMode.Tcp)
                return;

            foreach (var i in lsvServices.SelectedItems)
            {
                var item = i as ListViewItem;
                var id = (Guid)item.Tag;

                var sv = Services.FirstOrDefault(a => a.ID == id);
                if (sv == null)
                    continue;

                var path = System.IO.Path.GetDirectoryName(sv.Path);

                Process.Start("explorer.exe", path);
            }

        }

        private void rmiServices_StartAll_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.Items.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.Items)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    this.WatchConnection.TryCatch(a => a.StartService(id));
                }

            }
        }

        private void rmiServices_StopAll_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.Items.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.Items)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    this.WatchConnection.TryCatch(a => a.ShutdownService(id));
                }

            }
        }

        private void rmiServices_OpenAllLiveLogs_Click(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            if (lsvServices.Items.Count == 0)
                return;

            lock (_sync)
            {
                foreach (var i in lsvServices.Items)
                {
                    var item = i as ListViewItem;
                    var id = (Guid)item.Tag;

                    frmLogs frm = new frmLogs();
                    frm.LiveLogs = true;
                    frm.serviceId = id;
                    frm.Connection = this.WatchConnection;
                    frm.Show();
                }

            }
        }

        private void tmPing_Tick(object sender, EventArgs e)
        {
            if (this.WatchConnection == null)
                return;

            //Try async ping
            this.WatchConnection.Async(a => a.Ping());
        }
    }
}
