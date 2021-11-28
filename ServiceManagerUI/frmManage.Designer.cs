namespace ServiceManager.UI
{
    partial class frmManage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lsvServices = new System.Windows.Forms.ListView();
            this.cmID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiMenu_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.rpv = new Telerik.WinControls.UI.RadPageView();
            this.radPageViewPage2 = new Telerik.WinControls.UI.RadPageViewPage();
            this.bnAddService = new Telerik.WinControls.UI.RadButton();
            this.radPageViewPage1 = new Telerik.WinControls.UI.RadPageViewPage();
            this.bnSettings_Save = new Telerik.WinControls.UI.RadButton();
            this.nudStartup_Delay = new System.Windows.Forms.NumericUpDown();
            this.radLabel3 = new Telerik.WinControls.UI.RadLabel();
            this.radLabel2 = new Telerik.WinControls.UI.RadLabel();
            this.nud_TCPPort = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new Telerik.WinControls.UI.RadLabel();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.rrbMode_TCP = new Telerik.WinControls.UI.RadRadioButton();
            this.rrbMode_NetPipes = new Telerik.WinControls.UI.RadRadioButton();
            this.radPageViewPage3 = new Telerik.WinControls.UI.RadPageViewPage();
            this.txtNotifications_InstanceName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkNotifications_OnConsoleError = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnConsoleOutput = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnStop = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnRestart = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnServerShutdown = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnServerStart = new Telerik.WinControls.UI.RadCheckBox();
            this.chkNotifications_OnStart = new Telerik.WinControls.UI.RadCheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbNotification_Extensions = new Telerik.WinControls.UI.RadDropDownList();
            this.rbnNotifications_Save = new Telerik.WinControls.UI.RadButton();
            this.bnNotifications_Configure = new Telerik.WinControls.UI.RadButton();
            this.cmsMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpv)).BeginInit();
            this.rpv.SuspendLayout();
            this.radPageViewPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnAddService)).BeginInit();
            this.radPageViewPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnSettings_Save)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartup_Delay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_TCP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_NetPipes)).BeginInit();
            this.radPageViewPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnConsoleError)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnConsoleOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnRestart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnServerShutdown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnServerStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbNotification_Extensions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rbnNotifications_Save)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnNotifications_Configure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // lsvServices
            // 
            this.lsvServices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cmID,
            this.cmEnabled,
            this.cmDescription,
            this.cmTitle,
            this.cmPath});
            this.lsvServices.FullRowSelect = true;
            this.lsvServices.GridLines = true;
            this.lsvServices.HideSelection = false;
            this.lsvServices.Location = new System.Drawing.Point(3, 37);
            this.lsvServices.Name = "lsvServices";
            this.lsvServices.Size = new System.Drawing.Size(739, 339);
            this.lsvServices.TabIndex = 3;
            this.lsvServices.UseCompatibleStateImageBehavior = false;
            this.lsvServices.View = System.Windows.Forms.View.Details;
            this.lsvServices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseClick);
            this.lsvServices.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseDoubleClick);
            // 
            // cmID
            // 
            this.cmID.Text = "ID";
            this.cmID.Width = 120;
            // 
            // cmEnabled
            // 
            this.cmEnabled.Text = "Enabled";
            // 
            // cmDescription
            // 
            this.cmDescription.Text = "Description";
            this.cmDescription.Width = 150;
            // 
            // cmTitle
            // 
            this.cmTitle.Text = "Title";
            this.cmTitle.Width = 120;
            // 
            // cmPath
            // 
            this.cmPath.Text = "Path";
            this.cmPath.Width = 240;
            // 
            // cmsMenu
            // 
            this.cmsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiMenu_Delete});
            this.cmsMenu.Name = "cmsMenu";
            this.cmsMenu.Size = new System.Drawing.Size(147, 26);
            // 
            // tsmiMenu_Delete
            // 
            this.tsmiMenu_Delete.Name = "tsmiMenu_Delete";
            this.tsmiMenu_Delete.Size = new System.Drawing.Size(146, 22);
            this.tsmiMenu_Delete.Text = "Delete service";
            this.tsmiMenu_Delete.Click += new System.EventHandler(this.tsmiMenu_Delete_Click);
            // 
            // rpv
            // 
            this.rpv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rpv.Controls.Add(this.radPageViewPage2);
            this.rpv.Controls.Add(this.radPageViewPage1);
            this.rpv.Controls.Add(this.radPageViewPage3);
            this.rpv.Location = new System.Drawing.Point(12, 12);
            this.rpv.Name = "rpv";
            this.rpv.SelectedPage = this.radPageViewPage3;
            this.rpv.Size = new System.Drawing.Size(766, 427);
            this.rpv.TabIndex = 4;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.rpv.GetChildAt(0))).StripButtons = Telerik.WinControls.UI.StripViewButtons.None;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.rpv.GetChildAt(0))).ShowItemCloseButton = false;
            // 
            // radPageViewPage2
            // 
            this.radPageViewPage2.Controls.Add(this.bnAddService);
            this.radPageViewPage2.Controls.Add(this.lsvServices);
            this.radPageViewPage2.ItemSize = new System.Drawing.SizeF(56F, 28F);
            this.radPageViewPage2.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage2.Name = "radPageViewPage2";
            this.radPageViewPage2.Size = new System.Drawing.Size(745, 379);
            this.radPageViewPage2.Text = "Services";
            // 
            // bnAddService
            // 
            this.bnAddService.Location = new System.Drawing.Point(3, 3);
            this.bnAddService.Name = "bnAddService";
            this.bnAddService.Size = new System.Drawing.Size(110, 28);
            this.bnAddService.TabIndex = 4;
            this.bnAddService.Text = "Add service";
            this.bnAddService.Click += new System.EventHandler(this.bnAddServer_Click);
            // 
            // radPageViewPage1
            // 
            this.radPageViewPage1.Controls.Add(this.bnSettings_Save);
            this.radPageViewPage1.Controls.Add(this.nudStartup_Delay);
            this.radPageViewPage1.Controls.Add(this.radLabel3);
            this.radPageViewPage1.Controls.Add(this.radLabel2);
            this.radPageViewPage1.Controls.Add(this.nud_TCPPort);
            this.radPageViewPage1.Controls.Add(this.lblPort);
            this.radPageViewPage1.Controls.Add(this.radLabel1);
            this.radPageViewPage1.Controls.Add(this.rrbMode_TCP);
            this.radPageViewPage1.Controls.Add(this.rrbMode_NetPipes);
            this.radPageViewPage1.ItemSize = new System.Drawing.SizeF(56F, 28F);
            this.radPageViewPage1.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage1.Name = "radPageViewPage1";
            this.radPageViewPage1.Size = new System.Drawing.Size(745, 379);
            this.radPageViewPage1.Text = "Settings";
            // 
            // bnSettings_Save
            // 
            this.bnSettings_Save.Location = new System.Drawing.Point(30, 325);
            this.bnSettings_Save.Name = "bnSettings_Save";
            this.bnSettings_Save.Size = new System.Drawing.Size(110, 24);
            this.bnSettings_Save.TabIndex = 3;
            this.bnSettings_Save.Text = "Save";
            this.bnSettings_Save.Click += new System.EventHandler(this.bnSettings_Save_Click);
            // 
            // nudStartup_Delay
            // 
            this.nudStartup_Delay.Location = new System.Drawing.Point(158, 140);
            this.nudStartup_Delay.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.nudStartup_Delay.Name = "nudStartup_Delay";
            this.nudStartup_Delay.Size = new System.Drawing.Size(75, 20);
            this.nudStartup_Delay.TabIndex = 2;
            // 
            // radLabel3
            // 
            this.radLabel3.Location = new System.Drawing.Point(239, 141);
            this.radLabel3.Name = "radLabel3";
            this.radLabel3.Size = new System.Drawing.Size(119, 18);
            this.radLabel3.TabIndex = 1;
            this.radLabel3.Text = "seconds (0 = no delay)";
            // 
            // radLabel2
            // 
            this.radLabel2.Location = new System.Drawing.Point(32, 140);
            this.radLabel2.Name = "radLabel2";
            this.radLabel2.Size = new System.Drawing.Size(120, 18);
            this.radLabel2.TabIndex = 1;
            this.radLabel2.Text = "Delay services start by ";
            // 
            // nud_TCPPort
            // 
            this.nud_TCPPort.Enabled = false;
            this.nud_TCPPort.Location = new System.Drawing.Point(158, 89);
            this.nud_TCPPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nud_TCPPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_TCPPort.Name = "nud_TCPPort";
            this.nud_TCPPort.Size = new System.Drawing.Size(75, 20);
            this.nud_TCPPort.TabIndex = 2;
            this.nud_TCPPort.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // lblPort
            // 
            this.lblPort.Enabled = false;
            this.lblPort.Location = new System.Drawing.Point(111, 90);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 18);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port:";
            // 
            // radLabel1
            // 
            this.radLabel1.Location = new System.Drawing.Point(70, 18);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(58, 18);
            this.radLabel1.TabIndex = 1;
            this.radLabel1.Text = "Run Mode";
            // 
            // rrbMode_TCP
            // 
            this.rrbMode_TCP.Location = new System.Drawing.Point(88, 66);
            this.rrbMode_TCP.Name = "rrbMode_TCP";
            this.rrbMode_TCP.Size = new System.Drawing.Size(39, 18);
            this.rrbMode_TCP.TabIndex = 0;
            this.rrbMode_TCP.Text = "TCP";
            this.rrbMode_TCP.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.rrbMode_TCP_ToggleStateChanged);
            // 
            // rrbMode_NetPipes
            // 
            this.rrbMode_NetPipes.Location = new System.Drawing.Point(88, 42);
            this.rrbMode_NetPipes.Name = "rrbMode_NetPipes";
            this.rrbMode_NetPipes.Size = new System.Drawing.Size(68, 18);
            this.rrbMode_NetPipes.TabIndex = 0;
            this.rrbMode_NetPipes.Text = "Net Pipes";
            this.rrbMode_NetPipes.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.rrbMode_NetPipes_ToggleStateChanged);
            // 
            // radPageViewPage3
            // 
            this.radPageViewPage3.Controls.Add(this.bnNotifications_Configure);
            this.radPageViewPage3.Controls.Add(this.txtNotifications_InstanceName);
            this.radPageViewPage3.Controls.Add(this.label4);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnConsoleError);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnConsoleOutput);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnStop);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnRestart);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnServerShutdown);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnServerStart);
            this.radPageViewPage3.Controls.Add(this.chkNotifications_OnStart);
            this.radPageViewPage3.Controls.Add(this.label2);
            this.radPageViewPage3.Controls.Add(this.label1);
            this.radPageViewPage3.Controls.Add(this.cmbNotification_Extensions);
            this.radPageViewPage3.Controls.Add(this.rbnNotifications_Save);
            this.radPageViewPage3.ItemSize = new System.Drawing.SizeF(80F, 28F);
            this.radPageViewPage3.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage3.Name = "radPageViewPage3";
            this.radPageViewPage3.Size = new System.Drawing.Size(745, 379);
            this.radPageViewPage3.Text = "Notifications";
            // 
            // txtNotifications_InstanceName
            // 
            this.txtNotifications_InstanceName.Location = new System.Drawing.Point(31, 33);
            this.txtNotifications_InstanceName.Name = "txtNotifications_InstanceName";
            this.txtNotifications_InstanceName.Size = new System.Drawing.Size(256, 20);
            this.txtNotifications_InstanceName.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Instance Name:";
            // 
            // chkNotifications_OnConsoleError
            // 
            this.chkNotifications_OnConsoleError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnConsoleError.Location = new System.Drawing.Point(31, 246);
            this.chkNotifications_OnConsoleError.Name = "chkNotifications_OnConsoleError";
            this.chkNotifications_OnConsoleError.Size = new System.Drawing.Size(106, 18);
            this.chkNotifications_OnConsoleError.TabIndex = 10;
            this.chkNotifications_OnConsoleError.Text = "On Console Error";
            this.chkNotifications_OnConsoleError.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnConsoleOutput
            // 
            this.chkNotifications_OnConsoleOutput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnConsoleOutput.Location = new System.Drawing.Point(31, 222);
            this.chkNotifications_OnConsoleOutput.Name = "chkNotifications_OnConsoleOutput";
            this.chkNotifications_OnConsoleOutput.Size = new System.Drawing.Size(117, 18);
            this.chkNotifications_OnConsoleOutput.TabIndex = 10;
            this.chkNotifications_OnConsoleOutput.Text = "On Console Output";
            this.chkNotifications_OnConsoleOutput.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnStop
            // 
            this.chkNotifications_OnStop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnStop.Location = new System.Drawing.Point(31, 174);
            this.chkNotifications_OnStop.Name = "chkNotifications_OnStop";
            this.chkNotifications_OnStop.Size = new System.Drawing.Size(61, 18);
            this.chkNotifications_OnStop.TabIndex = 10;
            this.chkNotifications_OnStop.Text = "On Stop";
            this.chkNotifications_OnStop.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnRestart
            // 
            this.chkNotifications_OnRestart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnRestart.Location = new System.Drawing.Point(31, 198);
            this.chkNotifications_OnRestart.Name = "chkNotifications_OnRestart";
            this.chkNotifications_OnRestart.Size = new System.Drawing.Size(73, 18);
            this.chkNotifications_OnRestart.TabIndex = 10;
            this.chkNotifications_OnRestart.Text = "On Restart";
            this.chkNotifications_OnRestart.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnServerShutdown
            // 
            this.chkNotifications_OnServerShutdown.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnServerShutdown.Location = new System.Drawing.Point(302, 174);
            this.chkNotifications_OnServerShutdown.Name = "chkNotifications_OnServerShutdown";
            this.chkNotifications_OnServerShutdown.Size = new System.Drawing.Size(123, 18);
            this.chkNotifications_OnServerShutdown.TabIndex = 10;
            this.chkNotifications_OnServerShutdown.Text = "On Server Shutdown";
            this.chkNotifications_OnServerShutdown.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnServerStart
            // 
            this.chkNotifications_OnServerStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnServerStart.Location = new System.Drawing.Point(302, 150);
            this.chkNotifications_OnServerStart.Name = "chkNotifications_OnServerStart";
            this.chkNotifications_OnServerStart.Size = new System.Drawing.Size(96, 18);
            this.chkNotifications_OnServerStart.TabIndex = 10;
            this.chkNotifications_OnServerStart.Text = "On Server Start";
            this.chkNotifications_OnServerStart.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkNotifications_OnStart
            // 
            this.chkNotifications_OnStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnStart.Location = new System.Drawing.Point(31, 150);
            this.chkNotifications_OnStart.Name = "chkNotifications_OnStart";
            this.chkNotifications_OnStart.Size = new System.Drawing.Size(62, 18);
            this.chkNotifications_OnStart.TabIndex = 10;
            this.chkNotifications_OnStart.Text = "On Start";
            this.chkNotifications_OnStart.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Notification On";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Notification Extension";
            // 
            // cmbNotification_Extensions
            // 
            this.cmbNotification_Extensions.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            this.cmbNotification_Extensions.Location = new System.Drawing.Point(30, 87);
            this.cmbNotification_Extensions.Name = "cmbNotification_Extensions";
            this.cmbNotification_Extensions.Size = new System.Drawing.Size(239, 20);
            this.cmbNotification_Extensions.TabIndex = 5;
            // 
            // rbnNotifications_Save
            // 
            this.rbnNotifications_Save.Location = new System.Drawing.Point(30, 325);
            this.rbnNotifications_Save.Name = "rbnNotifications_Save";
            this.rbnNotifications_Save.Size = new System.Drawing.Size(110, 24);
            this.rbnNotifications_Save.TabIndex = 4;
            this.rbnNotifications_Save.Text = "Save";
            this.rbnNotifications_Save.Click += new System.EventHandler(this.rbnNotifications_Save_Click);
            // 
            // bnNotifications_Configure
            // 
            this.bnNotifications_Configure.Location = new System.Drawing.Point(288, 82);
            this.bnNotifications_Configure.Name = "bnNotifications_Configure";
            this.bnNotifications_Configure.Size = new System.Drawing.Size(110, 28);
            this.bnNotifications_Configure.TabIndex = 13;
            this.bnNotifications_Configure.Text = "Configure";
            this.bnNotifications_Configure.Click += new System.EventHandler(this.bnNotifications_Configure_Click);
            // 
            // frmManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 451);
            this.Controls.Add(this.rpv);
            this.Name = "frmManage";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage";
            this.Load += new System.EventHandler(this.frmManage_Load);
            this.cmsMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rpv)).EndInit();
            this.rpv.ResumeLayout(false);
            this.radPageViewPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bnAddService)).EndInit();
            this.radPageViewPage1.ResumeLayout(false);
            this.radPageViewPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnSettings_Save)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartup_Delay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_TCP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_NetPipes)).EndInit();
            this.radPageViewPage3.ResumeLayout(false);
            this.radPageViewPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnConsoleError)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnConsoleOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnRestart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnServerShutdown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnServerStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkNotifications_OnStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbNotification_Extensions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rbnNotifications_Save)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnNotifications_Configure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvServices;
        private System.Windows.Forms.ColumnHeader cmID;
        private System.Windows.Forms.ColumnHeader cmEnabled;
        private System.Windows.Forms.ColumnHeader cmDescription;
        private System.Windows.Forms.ColumnHeader cmTitle;
        private System.Windows.Forms.ColumnHeader cmPath;
        private System.Windows.Forms.ContextMenuStrip cmsMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Delete;
        private Telerik.WinControls.UI.RadPageView rpv;
        private Telerik.WinControls.UI.RadPageViewPage radPageViewPage1;
        private Telerik.WinControls.UI.RadPageViewPage radPageViewPage2;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadRadioButton rrbMode_NetPipes;
        private Telerik.WinControls.UI.RadRadioButton rrbMode_TCP;
        private System.Windows.Forms.NumericUpDown nud_TCPPort;
        private Telerik.WinControls.UI.RadLabel lblPort;
        private Telerik.WinControls.UI.RadButton bnAddService;
        private Telerik.WinControls.UI.RadButton bnSettings_Save;
        private System.Windows.Forms.NumericUpDown nudStartup_Delay;
        private Telerik.WinControls.UI.RadLabel radLabel3;
        private Telerik.WinControls.UI.RadLabel radLabel2;
        private Telerik.WinControls.UI.RadPageViewPage radPageViewPage3;
        private Telerik.WinControls.UI.RadButton rbnNotifications_Save;
        private Telerik.WinControls.UI.RadDropDownList cmbNotification_Extensions;
        
        private System.Windows.Forms.Label label1;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnStart;
        private System.Windows.Forms.Label label2;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnConsoleError;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnConsoleOutput;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnStop;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnRestart;
        private System.Windows.Forms.TextBox txtNotifications_InstanceName;
        private System.Windows.Forms.Label label4;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnServerShutdown;
        private Telerik.WinControls.UI.RadCheckBox chkNotifications_OnServerStart;
        private Telerik.WinControls.UI.RadButton bnNotifications_Configure;
    }
}