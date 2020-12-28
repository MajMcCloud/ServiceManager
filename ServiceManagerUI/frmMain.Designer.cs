namespace ServiceManager.UI
{
    partial class frmMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn1 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 0", "Activity");
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn2 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 1", "PID");
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn3 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 2", "Title");
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn4 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 3", "Status");
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn5 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 4", "Restarts");
            Telerik.WinControls.UI.ListViewDetailColumn listViewDetailColumn6 = new Telerik.WinControls.UI.ListViewDetailColumn("Column 5", "Auto Restart");
            this.tmRefresh = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.cmsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiMenu_Restart = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMenu_Shutdown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiMenu_Logs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMenu_Livelogs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiMenu_ToggleAutoRestart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiMenu_OpenLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.lsvServices = new Telerik.WinControls.UI.RadListView();
            this.rmiManager = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiManager_OpenConfig = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiManager_Refresh = new Telerik.WinControls.UI.RadMenuItem();
            this.radMenuSeparatorItem1 = new Telerik.WinControls.UI.RadMenuSeparatorItem();
            this.rmiManager_Close = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiTesting = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiTesting_Start = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiTesting_End = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiService = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiService_Install = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiService_Uninstall = new Telerik.WinControls.UI.RadMenuItem();
            this.radMenuSeparatorItem2 = new Telerik.WinControls.UI.RadMenuSeparatorItem();
            this.rmiService_Start = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiService_Restart = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiService_Stop = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiInstances = new Telerik.WinControls.UI.RadMenuItem();
            this.rmiInstances_ConnectTo = new Telerik.WinControls.UI.RadMenuItem();
            this.radMenuSeparatorItem3 = new Telerik.WinControls.UI.RadMenuSeparatorItem();
            this.radMenu1 = new Telerik.WinControls.UI.RadMenu();
            this.radStatusStrip1 = new Telerik.WinControls.UI.RadStatusStrip();
            this.radLabelElement1 = new Telerik.WinControls.UI.RadLabelElement();
            this.lblStatus = new Telerik.WinControls.UI.RadLabelElement();
            this.commandBarSeparator1 = new Telerik.WinControls.UI.CommandBarSeparator();
            this.radLabelElement3 = new Telerik.WinControls.UI.RadLabelElement();
            this.radLabelElement2 = new Telerik.WinControls.UI.RadLabelElement();
            this.lblActiveServiceCount = new Telerik.WinControls.UI.RadLabelElement();
            this.rmServiceRefresh = new System.Windows.Forms.Timer(this.components);
            this.cmsMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lsvServices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radMenu1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // tmRefresh
            // 
            this.tmRefresh.Interval = 10000;
            this.tmRefresh.Tick += new System.EventHandler(this.tmRefresh_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Running services:";
            // 
            // cmsMenu
            // 
            this.cmsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiMenu_Restart,
            this.tsmiMenu_Shutdown,
            this.toolStripSeparator4,
            this.tsmiMenu_Logs,
            this.tsmiMenu_Livelogs,
            this.toolStripSeparator5,
            this.tsmiMenu_ToggleAutoRestart,
            this.toolStripSeparator6,
            this.tsmiMenu_OpenLogs});
            this.cmsMenu.Name = "cmsMenu";
            this.cmsMenu.Size = new System.Drawing.Size(178, 154);
            // 
            // tsmiMenu_Restart
            // 
            this.tsmiMenu_Restart.Image = global::ServiceManager.UI.Properties.Resources.play;
            this.tsmiMenu_Restart.Name = "tsmiMenu_Restart";
            this.tsmiMenu_Restart.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_Restart.Text = "Start service";
            this.tsmiMenu_Restart.Click += new System.EventHandler(this.tsmiMenu_Restart_Click);
            // 
            // tsmiMenu_Shutdown
            // 
            this.tsmiMenu_Shutdown.Image = ((System.Drawing.Image)(resources.GetObject("tsmiMenu_Shutdown.Image")));
            this.tsmiMenu_Shutdown.Name = "tsmiMenu_Shutdown";
            this.tsmiMenu_Shutdown.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_Shutdown.Text = "Shutdown service";
            this.tsmiMenu_Shutdown.Click += new System.EventHandler(this.tsmiMenu_Shutdown_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(174, 6);
            // 
            // tsmiMenu_Logs
            // 
            this.tsmiMenu_Logs.Name = "tsmiMenu_Logs";
            this.tsmiMenu_Logs.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_Logs.Text = "Show logs";
            this.tsmiMenu_Logs.Click += new System.EventHandler(this.tsmiMenu_Logs_Click);
            // 
            // tsmiMenu_Livelogs
            // 
            this.tsmiMenu_Livelogs.Name = "tsmiMenu_Livelogs";
            this.tsmiMenu_Livelogs.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_Livelogs.Text = "Show live logs";
            this.tsmiMenu_Livelogs.Click += new System.EventHandler(this.tsmiMenu_Livelogs_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(174, 6);
            // 
            // tsmiMenu_ToggleAutoRestart
            // 
            this.tsmiMenu_ToggleAutoRestart.Name = "tsmiMenu_ToggleAutoRestart";
            this.tsmiMenu_ToggleAutoRestart.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_ToggleAutoRestart.Text = "Toggle Auto Restart";
            this.tsmiMenu_ToggleAutoRestart.Click += new System.EventHandler(this.tsmiMenu_ToggleAutoRestart_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(174, 6);
            // 
            // tsmiMenu_OpenLogs
            // 
            this.tsmiMenu_OpenLogs.Name = "tsmiMenu_OpenLogs";
            this.tsmiMenu_OpenLogs.Size = new System.Drawing.Size(177, 22);
            this.tsmiMenu_OpenLogs.Text = "Open logs";
            this.tsmiMenu_OpenLogs.Click += new System.EventHandler(this.tsmiMenu_OpenLogs_Click);
            // 
            // lsvServices
            // 
            this.lsvServices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            listViewDetailColumn1.HeaderText = "Activity";
            listViewDetailColumn1.Width = 60F;
            listViewDetailColumn2.HeaderText = "PID";
            listViewDetailColumn2.Width = 60F;
            listViewDetailColumn3.HeaderText = "Title";
            listViewDetailColumn4.HeaderText = "Status";
            listViewDetailColumn4.Width = 150F;
            listViewDetailColumn5.HeaderText = "Restarts";
            listViewDetailColumn5.Width = 60F;
            listViewDetailColumn6.HeaderText = "Auto Restart";
            listViewDetailColumn6.Width = 80F;
            this.lsvServices.Columns.AddRange(new Telerik.WinControls.UI.ListViewDetailColumn[] {
            listViewDetailColumn1,
            listViewDetailColumn2,
            listViewDetailColumn3,
            listViewDetailColumn4,
            listViewDetailColumn5,
            listViewDetailColumn6});
            this.lsvServices.ItemSpacing = -1;
            this.lsvServices.Location = new System.Drawing.Point(13, 59);
            this.lsvServices.MultiSelect = true;
            this.lsvServices.Name = "lsvServices";
            this.lsvServices.Size = new System.Drawing.Size(789, 371);
            this.lsvServices.TabIndex = 5;
            this.lsvServices.ViewType = Telerik.WinControls.UI.ListViewType.DetailsView;
            this.lsvServices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseClick_1);
            this.lsvServices.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseDoubleClick_1);
            // 
            // rmiManager
            // 
            this.rmiManager.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.rmiManager_OpenConfig,
            this.rmiManager_Refresh,
            this.radMenuSeparatorItem1,
            this.rmiManager_Close});
            this.rmiManager.Name = "rmiManager";
            this.rmiManager.Text = "Manager";
            // 
            // rmiManager_OpenConfig
            // 
            this.rmiManager_OpenConfig.Name = "rmiManager_OpenConfig";
            this.rmiManager_OpenConfig.Text = "Open config";
            this.rmiManager_OpenConfig.Click += new System.EventHandler(this.rmiManager_OpenConfig_Click);
            // 
            // rmiManager_Refresh
            // 
            this.rmiManager_Refresh.Enabled = false;
            this.rmiManager_Refresh.Name = "rmiManager_Refresh";
            this.rmiManager_Refresh.Text = "Refresh services";
            this.rmiManager_Refresh.Click += new System.EventHandler(this.rmiManager_Refresh_Click);
            // 
            // radMenuSeparatorItem1
            // 
            this.radMenuSeparatorItem1.Name = "radMenuSeparatorItem1";
            this.radMenuSeparatorItem1.Text = "radMenuSeparatorItem1";
            this.radMenuSeparatorItem1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rmiManager_Close
            // 
            this.rmiManager_Close.Name = "rmiManager_Close";
            this.rmiManager_Close.Text = "Exit";
            this.rmiManager_Close.Click += new System.EventHandler(this.rmiManager_Close_Click);
            // 
            // rmiTesting
            // 
            this.rmiTesting.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.rmiTesting_Start,
            this.rmiTesting_End});
            this.rmiTesting.Name = "rmiTesting";
            this.rmiTesting.Text = "Testing";
            // 
            // rmiTesting_Start
            // 
            this.rmiTesting_Start.Name = "rmiTesting_Start";
            this.rmiTesting_Start.Text = "Start testing";
            this.rmiTesting_Start.Click += new System.EventHandler(this.rmiTesting_Start_Click);
            // 
            // rmiTesting_End
            // 
            this.rmiTesting_End.Enabled = false;
            this.rmiTesting_End.Name = "rmiTesting_End";
            this.rmiTesting_End.Text = "End testing";
            this.rmiTesting_End.Click += new System.EventHandler(this.rmiTesting_End_Click);
            // 
            // rmiService
            // 
            this.rmiService.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.rmiService_Install,
            this.rmiService_Uninstall,
            this.radMenuSeparatorItem2,
            this.rmiService_Start,
            this.rmiService_Restart,
            this.rmiService_Stop});
            this.rmiService.Name = "rmiService";
            this.rmiService.Text = "Service";
            this.rmiService.DropDownOpening += new System.ComponentModel.CancelEventHandler(this.rmiService_DropDownOpening);
            // 
            // rmiService_Install
            // 
            this.rmiService_Install.Name = "rmiService_Install";
            this.rmiService_Install.Text = "Install service";
            this.rmiService_Install.Click += new System.EventHandler(this.rmiService_Install_Click);
            // 
            // rmiService_Uninstall
            // 
            this.rmiService_Uninstall.Name = "rmiService_Uninstall";
            this.rmiService_Uninstall.Text = "Uninstall service";
            this.rmiService_Uninstall.Click += new System.EventHandler(this.rmiService_Uninstall_Click);
            // 
            // radMenuSeparatorItem2
            // 
            this.radMenuSeparatorItem2.Name = "radMenuSeparatorItem2";
            this.radMenuSeparatorItem2.Text = "radMenuSeparatorItem2";
            this.radMenuSeparatorItem2.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rmiService_Start
            // 
            this.rmiService_Start.Name = "rmiService_Start";
            this.rmiService_Start.Text = "Start service";
            this.rmiService_Start.Click += new System.EventHandler(this.rmiService_Start_Click);
            // 
            // rmiService_Restart
            // 
            this.rmiService_Restart.Name = "rmiService_Restart";
            this.rmiService_Restart.Text = "Restart service";
            this.rmiService_Restart.Click += new System.EventHandler(this.rmiService_Restart_Click);
            // 
            // rmiService_Stop
            // 
            this.rmiService_Stop.Name = "rmiService_Stop";
            this.rmiService_Stop.Text = "Stop service";
            this.rmiService_Stop.Click += new System.EventHandler(this.rmiService_Stop_Click);
            // 
            // rmiInstances
            // 
            this.rmiInstances.Image = null;
            this.rmiInstances.ImageAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.rmiInstances.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.rmiInstances_ConnectTo,
            this.radMenuSeparatorItem3});
            this.rmiInstances.Name = "rmiInstances";
            this.rmiInstances.Text = "Instances";
            this.rmiInstances.DropDownOpening += new System.ComponentModel.CancelEventHandler(this.rmiInstances_DropDownOpening);
            // 
            // rmiInstances_ConnectTo
            // 
            this.rmiInstances_ConnectTo.Image = global::ServiceManager.UI.Properties.Resources.space_ship_24px;
            this.rmiInstances_ConnectTo.Name = "rmiInstances_ConnectTo";
            this.rmiInstances_ConnectTo.Text = "Connect to";
            this.rmiInstances_ConnectTo.Click += new System.EventHandler(this.rmiInstances_ConnectTo_Click);
            // 
            // radMenuSeparatorItem3
            // 
            this.radMenuSeparatorItem3.Name = "radMenuSeparatorItem3";
            this.radMenuSeparatorItem3.Text = "radMenuSeparatorItem3";
            this.radMenuSeparatorItem3.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // radMenu1
            // 
            this.radMenu1.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.rmiManager,
            this.rmiTesting,
            this.rmiService,
            this.rmiInstances});
            this.radMenu1.Location = new System.Drawing.Point(0, 0);
            this.radMenu1.Name = "radMenu1";
            this.radMenu1.Size = new System.Drawing.Size(814, 20);
            this.radMenu1.TabIndex = 6;
            // 
            // radStatusStrip1
            // 
            this.radStatusStrip1.Items.AddRange(new Telerik.WinControls.RadItem[] {
            this.radLabelElement1,
            this.lblStatus,
            this.commandBarSeparator1,
            this.radLabelElement3,
            this.radLabelElement2,
            this.lblActiveServiceCount});
            this.radStatusStrip1.Location = new System.Drawing.Point(0, 448);
            this.radStatusStrip1.Name = "radStatusStrip1";
            this.radStatusStrip1.Size = new System.Drawing.Size(814, 26);
            this.radStatusStrip1.TabIndex = 7;
            // 
            // radLabelElement1
            // 
            this.radLabelElement1.Name = "radLabelElement1";
            this.radStatusStrip1.SetSpring(this.radLabelElement1, false);
            this.radLabelElement1.Text = "Status:";
            this.radLabelElement1.TextWrap = true;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.radStatusStrip1.SetSpring(this.lblStatus, false);
            this.lblStatus.Text = "Disconnected";
            this.lblStatus.TextWrap = true;
            // 
            // commandBarSeparator1
            // 
            this.commandBarSeparator1.Name = "commandBarSeparator1";
            this.radStatusStrip1.SetSpring(this.commandBarSeparator1, false);
            this.commandBarSeparator1.VisibleInOverflowMenu = false;
            // 
            // radLabelElement3
            // 
            this.radLabelElement3.AutoSize = true;
            this.radLabelElement3.AutoSizeMode = Telerik.WinControls.RadAutoSizeMode.Auto;
            this.radLabelElement3.Name = "radLabelElement3";
            this.radStatusStrip1.SetSpring(this.radLabelElement3, true);
            this.radLabelElement3.Text = "";
            this.radLabelElement3.TextWrap = true;
            // 
            // radLabelElement2
            // 
            this.radLabelElement2.Name = "radLabelElement2";
            this.radStatusStrip1.SetSpring(this.radLabelElement2, false);
            this.radLabelElement2.Text = "RunningServices: ";
            this.radLabelElement2.TextWrap = true;
            // 
            // lblActiveServiceCount
            // 
            this.lblActiveServiceCount.Name = "lblActiveServiceCount";
            this.radStatusStrip1.SetSpring(this.lblActiveServiceCount, false);
            this.lblActiveServiceCount.Text = "0";
            this.lblActiveServiceCount.TextWrap = true;
            // 
            // rmServiceRefresh
            // 
            this.rmServiceRefresh.Enabled = true;
            this.rmServiceRefresh.Interval = 10000;
            this.rmServiceRefresh.Tick += new System.EventHandler(this.rmServiceRefresh_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 474);
            this.Controls.Add(this.radStatusStrip1);
            this.Controls.Add(this.radMenu1);
            this.Controls.Add(this.lsvServices);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(676, 313);
            this.Name = "frmMain";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Service Manager UI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.cmsMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lsvServices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radMenu1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radStatusStrip1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer tmRefresh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip cmsMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Restart;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Shutdown;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Logs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Livelogs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_ToggleAutoRestart;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_OpenLogs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private Telerik.WinControls.UI.RadListView lsvServices;
        private Telerik.WinControls.UI.RadMenuItem rmiManager;
        private Telerik.WinControls.UI.RadMenuItem rmiTesting;
        private Telerik.WinControls.UI.RadMenuItem rmiService;
        private Telerik.WinControls.UI.RadMenuItem rmiInstances;
        private Telerik.WinControls.UI.RadMenuItem rmiManager_OpenConfig;
        private Telerik.WinControls.UI.RadMenuSeparatorItem radMenuSeparatorItem1;
        private Telerik.WinControls.UI.RadMenuItem rmiManager_Close;
        private Telerik.WinControls.UI.RadMenu radMenu1;
        private Telerik.WinControls.UI.RadMenuItem rmiTesting_Start;
        private Telerik.WinControls.UI.RadMenuItem rmiTesting_End;
        private Telerik.WinControls.UI.RadMenuItem rmiManager_Refresh;
        private Telerik.WinControls.UI.RadMenuItem rmiService_Install;
        private Telerik.WinControls.UI.RadMenuItem rmiService_Uninstall;
        private Telerik.WinControls.UI.RadMenuSeparatorItem radMenuSeparatorItem2;
        private Telerik.WinControls.UI.RadMenuItem rmiService_Start;
        private Telerik.WinControls.UI.RadMenuItem rmiService_Restart;
        private Telerik.WinControls.UI.RadMenuItem rmiService_Stop;
        private Telerik.WinControls.UI.RadMenuItem rmiInstances_ConnectTo;
        private Telerik.WinControls.UI.RadMenuSeparatorItem radMenuSeparatorItem3;
        private Telerik.WinControls.UI.RadStatusStrip radStatusStrip1;
        private Telerik.WinControls.UI.RadLabelElement radLabelElement1;
        private Telerik.WinControls.UI.RadLabelElement lblStatus;
        private Telerik.WinControls.UI.CommandBarSeparator commandBarSeparator1;
        private Telerik.WinControls.UI.RadLabelElement radLabelElement3;
        private Telerik.WinControls.UI.RadLabelElement radLabelElement2;
        private Telerik.WinControls.UI.RadLabelElement lblActiveServiceCount;
        private System.Windows.Forms.Timer rmServiceRefresh;
    }
}

