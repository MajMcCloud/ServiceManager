namespace ServiceManager.UI
{
    partial class FrmManage
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabServices = new System.Windows.Forms.TabPage();
            this.bnAddService = new System.Windows.Forms.Button();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.bnSettings_Save = new System.Windows.Forms.Button();
            this.tabWindowsService = new System.Windows.Forms.TabPage();
            this.bnWindowsService_Save = new System.Windows.Forms.Button();
            this.nudStartup_Delay = new System.Windows.Forms.NumericUpDown();
            this.nudDependency_Wait_Timeout = new System.Windows.Forms.NumericUpDown();
            this.lblDependencyTimeoutUnit = new System.Windows.Forms.Label();
            this.lblDependencyTimeout = new System.Windows.Forms.Label();
            this.lblDelayUnit = new System.Windows.Forms.Label();
            this.lblDelay = new System.Windows.Forms.Label();
            this.nud_TCPPort = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblRunMode = new System.Windows.Forms.Label();
            this.rrbMode_TCP = new System.Windows.Forms.RadioButton();
            this.rrbMode_NetPipes = new System.Windows.Forms.RadioButton();
            this.chkRequireAdministrator = new System.Windows.Forms.CheckBox();
            this.tabNotifications = new System.Windows.Forms.TabPage();
            this.txtNotifications_InstanceName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkNotifications_OnConsoleError = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnConsoleOutput = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnStop = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnRestart = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnServerShutdown = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnServerStart = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnStart = new System.Windows.Forms.CheckBox();
            this.chkNotifications_OnDependencyWait = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbNotification_Extensions = new System.Windows.Forms.ComboBox();
            this.rbnNotifications_Save = new System.Windows.Forms.Button();
            this.bnNotifications_Configure = new System.Windows.Forms.Button();
            this.bnNotifications_Add = new System.Windows.Forms.Button();
            this.bnNotifications_Remove = new System.Windows.Forms.Button();
            this.lstNotifications_Providers = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.grpWindowsService = new System.Windows.Forms.GroupBox();
            this.lblService_StartMode = new System.Windows.Forms.Label();
            this.cmbService_StartMode = new System.Windows.Forms.ComboBox();
            this.lblService_Account = new System.Windows.Forms.Label();
            this.cmbService_Account = new System.Windows.Forms.ComboBox();
            this.lblService_User = new System.Windows.Forms.Label();
            this.txtService_User = new System.Windows.Forms.TextBox();
            this.lblService_Password = new System.Windows.Forms.Label();
            this.txtService_Password = new System.Windows.Forms.TextBox();
            this.lblService_Dependencies = new System.Windows.Forms.Label();
            this.cmbService_Dependencies = new System.Windows.Forms.ComboBox();
            this.lsvService_Dependencies = new System.Windows.Forms.ListView();
            this.colDependency_Title = new System.Windows.Forms.ColumnHeader();
            this.colDependency_Name = new System.Windows.Forms.ColumnHeader();
            this.colDependency_State = new System.Windows.Forms.ColumnHeader();
            this.bnService_Dependencies_Select = new System.Windows.Forms.Button();
            this.lblService_Hint = new System.Windows.Forms.Label();
            this.cmsMenu.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabServices.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabWindowsService.SuspendLayout();
            this.grpWindowsService.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartup_Delay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDependency_Wait_Timeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).BeginInit();
            this.tabNotifications.SuspendLayout();
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
            // tabMain
            // 
            this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabMain.Controls.Add(this.tabServices);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Controls.Add(this.tabNotifications);
            this.tabMain.Controls.Add(this.tabWindowsService);
            this.tabMain.Location = new System.Drawing.Point(12, 12);
            this.tabMain.Name = "tabMain";
            this.tabMain.Size = new System.Drawing.Size(766, 427);
            this.tabMain.TabIndex = 4;
            // 
            // tabServices
            // 
            this.tabServices.Controls.Add(this.bnAddService);
            this.tabServices.Controls.Add(this.lsvServices);
            this.tabServices.TabIndex = 0;
            this.tabServices.Location = new System.Drawing.Point(4, 22);
            this.tabServices.Name = "tabServices";
            this.tabServices.Size = new System.Drawing.Size(758, 401);
            this.tabServices.Text = "Services";
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
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.nudDependency_Wait_Timeout);
            this.tabSettings.Controls.Add(this.lblDependencyTimeoutUnit);
            this.tabSettings.Controls.Add(this.lblDependencyTimeout);
            this.tabSettings.Controls.Add(this.bnSettings_Save);
            this.tabSettings.Controls.Add(this.chkRequireAdministrator);
            this.tabSettings.Controls.Add(this.nudStartup_Delay);
            this.tabSettings.Controls.Add(this.lblDelayUnit);
            this.tabSettings.Controls.Add(this.lblDelay);
            this.tabSettings.Controls.Add(this.nud_TCPPort);
            this.tabSettings.Controls.Add(this.lblPort);
            this.tabSettings.Controls.Add(this.lblRunMode);
            this.tabSettings.Controls.Add(this.rrbMode_TCP);
            this.tabSettings.Controls.Add(this.rrbMode_NetPipes);
            this.tabSettings.TabIndex = 1;
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Size = new System.Drawing.Size(758, 401);
            this.tabSettings.Text = "Settings";
            // 
            // chkRequireAdministrator
            // 
            this.chkRequireAdministrator.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRequireAdministrator.Location = new System.Drawing.Point(32, 180);
            this.chkRequireAdministrator.Name = "chkRequireAdministrator";
            this.chkRequireAdministrator.AutoSize = true;
            this.chkRequireAdministrator.TabIndex = 4;
            this.chkRequireAdministrator.Text = "Only administrators may connect";
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
            // lblDelayUnit
            // 
            this.lblDelayUnit.Location = new System.Drawing.Point(239, 141);
            this.lblDelayUnit.Name = "lblDelayUnit";
            this.lblDelayUnit.AutoSize = true;
            this.lblDelayUnit.TabIndex = 1;
            this.lblDelayUnit.Text = "seconds (0 = no delay)";
            // 
            // lblDelay
            // 
            this.lblDelay.Location = new System.Drawing.Point(32, 140);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.AutoSize = true;
            this.lblDelay.TabIndex = 1;
            this.lblDelay.Text = "Delay services start by ";
            // 
            // lblDependencyTimeout
            // 
            this.lblDependencyTimeout.Location = new System.Drawing.Point(32, 214);
            this.lblDependencyTimeout.Name = "lblDependencyTimeout";
            this.lblDependencyTimeout.AutoSize = true;
            this.lblDependencyTimeout.TabIndex = 1;
            this.lblDependencyTimeout.Text = "Wait for a dependency for ";
            // 
            // lblDependencyTimeoutUnit
            // 
            this.lblDependencyTimeoutUnit.Location = new System.Drawing.Point(239, 215);
            this.lblDependencyTimeoutUnit.Name = "lblDependencyTimeoutUnit";
            this.lblDependencyTimeoutUnit.AutoSize = true;
            this.lblDependencyTimeoutUnit.TabIndex = 1;
            this.lblDependencyTimeoutUnit.Text = "seconds (0 = do not wait)";
            // 
            // nudDependency_Wait_Timeout
            // 
            this.nudDependency_Wait_Timeout.Location = new System.Drawing.Point(158, 214);
            this.nudDependency_Wait_Timeout.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudDependency_Wait_Timeout.Name = "nudDependency_Wait_Timeout";
            this.nudDependency_Wait_Timeout.Size = new System.Drawing.Size(75, 20);
            this.nudDependency_Wait_Timeout.TabIndex = 5;
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
            this.lblPort.AutoSize = true;
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port:";
            // 
            // lblRunMode
            // 
            this.lblRunMode.Location = new System.Drawing.Point(70, 18);
            this.lblRunMode.Name = "lblRunMode";
            this.lblRunMode.AutoSize = true;
            this.lblRunMode.TabIndex = 1;
            this.lblRunMode.Text = "Run Mode";
            // 
            // rrbMode_TCP
            // 
            this.rrbMode_TCP.Location = new System.Drawing.Point(88, 66);
            this.rrbMode_TCP.Name = "rrbMode_TCP";
            this.rrbMode_TCP.AutoSize = true;
            this.rrbMode_TCP.TabIndex = 0;
            this.rrbMode_TCP.Text = "TCP";
            this.rrbMode_TCP.CheckedChanged += new System.EventHandler(this.rrbMode_TCP_CheckedChanged);
            // 
            // rrbMode_NetPipes
            // 
            this.rrbMode_NetPipes.Location = new System.Drawing.Point(88, 42);
            this.rrbMode_NetPipes.Name = "rrbMode_NetPipes";
            this.rrbMode_NetPipes.AutoSize = true;
            this.rrbMode_NetPipes.TabIndex = 0;
            this.rrbMode_NetPipes.Text = "Net Pipes";
            //
            // tabWindowsService
            //
            this.tabWindowsService.Controls.Add(this.grpWindowsService);
            this.tabWindowsService.Controls.Add(this.bnWindowsService_Save);
            this.tabWindowsService.Location = new System.Drawing.Point(4, 22);
            this.tabWindowsService.Name = "tabWindowsService";
            this.tabWindowsService.Size = new System.Drawing.Size(758, 401);
            this.tabWindowsService.TabIndex = 3;
            this.tabWindowsService.Text = "Windows service";
            //
            // bnWindowsService_Save
            //
            this.bnWindowsService_Save.Location = new System.Drawing.Point(30, 325);
            this.bnWindowsService_Save.Name = "bnWindowsService_Save";
            this.bnWindowsService_Save.Size = new System.Drawing.Size(110, 24);
            this.bnWindowsService_Save.TabIndex = 1;
            this.bnWindowsService_Save.Text = "Save";
            this.bnWindowsService_Save.Click += new System.EventHandler(this.bnWindowsService_Save_Click);
            //
            // grpWindowsService
            //
            this.grpWindowsService.Controls.Add(this.lblService_StartMode);
            this.grpWindowsService.Controls.Add(this.cmbService_StartMode);
            this.grpWindowsService.Controls.Add(this.lblService_Account);
            this.grpWindowsService.Controls.Add(this.cmbService_Account);
            this.grpWindowsService.Controls.Add(this.lblService_User);
            this.grpWindowsService.Controls.Add(this.txtService_User);
            this.grpWindowsService.Controls.Add(this.lblService_Password);
            this.grpWindowsService.Controls.Add(this.txtService_Password);
            this.grpWindowsService.Controls.Add(this.lblService_Dependencies);
            this.grpWindowsService.Controls.Add(this.cmbService_Dependencies);
            this.grpWindowsService.Controls.Add(this.lsvService_Dependencies);
            this.grpWindowsService.Controls.Add(this.bnService_Dependencies_Select);
            this.grpWindowsService.Controls.Add(this.lblService_Hint);
            this.grpWindowsService.Location = new System.Drawing.Point(30, 12);
            this.grpWindowsService.Name = "grpWindowsService";
            this.grpWindowsService.Size = new System.Drawing.Size(700, 300);
            this.grpWindowsService.TabIndex = 0;
            this.grpWindowsService.TabStop = false;
            this.grpWindowsService.Text = "Windows service";
            // 
            // lblService_StartMode
            // 
            this.lblService_StartMode.AutoSize = true;
            this.lblService_StartMode.Location = new System.Drawing.Point(16, 27);
            this.lblService_StartMode.Name = "lblService_StartMode";
            this.lblService_StartMode.TabIndex = 0;
            this.lblService_StartMode.Text = "Start mode:";
            // 
            // cmbService_StartMode
            // 
            this.cmbService_StartMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbService_StartMode.FormattingEnabled = true;
            this.cmbService_StartMode.Location = new System.Drawing.Point(130, 24);
            this.cmbService_StartMode.Name = "cmbService_StartMode";
            this.cmbService_StartMode.Size = new System.Drawing.Size(210, 21);
            this.cmbService_StartMode.TabIndex = 1;
            // 
            // lblService_Account
            // 
            this.lblService_Account.AutoSize = true;
            this.lblService_Account.Location = new System.Drawing.Point(16, 60);
            this.lblService_Account.Name = "lblService_Account";
            this.lblService_Account.TabIndex = 2;
            this.lblService_Account.Text = "Log on as:";
            // 
            // cmbService_Account
            // 
            this.cmbService_Account.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbService_Account.FormattingEnabled = true;
            this.cmbService_Account.Location = new System.Drawing.Point(130, 57);
            this.cmbService_Account.Name = "cmbService_Account";
            this.cmbService_Account.Size = new System.Drawing.Size(210, 21);
            this.cmbService_Account.TabIndex = 3;
            this.cmbService_Account.SelectedIndexChanged += new System.EventHandler(this.cmbService_Account_SelectedIndexChanged);
            // 
            // lblService_User
            // 
            this.lblService_User.AutoSize = true;
            this.lblService_User.Location = new System.Drawing.Point(16, 93);
            this.lblService_User.Name = "lblService_User";
            this.lblService_User.TabIndex = 4;
            this.lblService_User.Text = "User name:";
            // 
            // txtService_User
            // 
            this.txtService_User.Location = new System.Drawing.Point(130, 90);
            this.txtService_User.Name = "txtService_User";
            this.txtService_User.Size = new System.Drawing.Size(210, 20);
            this.txtService_User.TabIndex = 5;
            // 
            // lblService_Password
            // 
            this.lblService_Password.AutoSize = true;
            this.lblService_Password.Location = new System.Drawing.Point(16, 122);
            this.lblService_Password.Name = "lblService_Password";
            this.lblService_Password.TabIndex = 6;
            this.lblService_Password.Text = "New password:";
            // 
            // txtService_Password
            // 
            this.txtService_Password.Location = new System.Drawing.Point(130, 119);
            this.txtService_Password.Name = "txtService_Password";
            this.txtService_Password.PasswordChar = '*';
            this.txtService_Password.Size = new System.Drawing.Size(210, 20);
            this.txtService_Password.TabIndex = 7;
            // 
            // lblService_Dependencies
            // 
            this.lblService_Dependencies.AutoSize = true;
            this.lblService_Dependencies.Location = new System.Drawing.Point(16, 155);
            this.lblService_Dependencies.Name = "lblService_Dependencies";
            this.lblService_Dependencies.TabIndex = 8;
            this.lblService_Dependencies.Text = "Dependencies:";
            // 
            // cmbService_Dependencies
            // 
            this.cmbService_Dependencies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbService_Dependencies.FormattingEnabled = true;
            this.cmbService_Dependencies.Location = new System.Drawing.Point(130, 152);
            this.cmbService_Dependencies.Name = "cmbService_Dependencies";
            this.cmbService_Dependencies.Size = new System.Drawing.Size(210, 21);
            this.cmbService_Dependencies.TabIndex = 9;
            this.cmbService_Dependencies.SelectedIndexChanged += new System.EventHandler(this.cmbService_Dependencies_SelectedIndexChanged);
            // 
            // lsvService_Dependencies
            // 
            this.lsvService_Dependencies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDependency_Title,
            this.colDependency_Name,
            this.colDependency_State});
            this.lsvService_Dependencies.FullRowSelect = true;
            this.lsvService_Dependencies.HideSelection = false;
            this.lsvService_Dependencies.Location = new System.Drawing.Point(19, 183);
            this.lsvService_Dependencies.MultiSelect = false;
            this.lsvService_Dependencies.Name = "lsvService_Dependencies";
            this.lsvService_Dependencies.Size = new System.Drawing.Size(480, 95);
            this.lsvService_Dependencies.TabIndex = 10;
            this.lsvService_Dependencies.UseCompatibleStateImageBehavior = false;
            this.lsvService_Dependencies.View = System.Windows.Forms.View.Details;
            // 
            // colDependency_Title
            // 
            this.colDependency_Title.Text = "Display name";
            this.colDependency_Title.Width = 200;
            // 
            // colDependency_Name
            // 
            this.colDependency_Name.Text = "Service name";
            this.colDependency_Name.Width = 160;
            // 
            // colDependency_State
            // 
            this.colDependency_State.Text = "Status";
            this.colDependency_State.Width = 100;
            // 
            // bnService_Dependencies_Select
            // 
            this.bnService_Dependencies_Select.Location = new System.Drawing.Point(507, 183);
            this.bnService_Dependencies_Select.Name = "bnService_Dependencies_Select";
            this.bnService_Dependencies_Select.Size = new System.Drawing.Size(90, 23);
            this.bnService_Dependencies_Select.TabIndex = 11;
            this.bnService_Dependencies_Select.Text = "Select...";
            this.bnService_Dependencies_Select.UseVisualStyleBackColor = true;
            this.bnService_Dependencies_Select.Click += new System.EventHandler(this.bnService_Dependencies_Select_Click);
            // 
            // lblService_Hint
            // 
            this.lblService_Hint.ForeColor = System.Drawing.Color.Firebrick;
            this.lblService_Hint.Location = new System.Drawing.Point(365, 27);
            this.lblService_Hint.Name = "lblService_Hint";
            this.lblService_Hint.Size = new System.Drawing.Size(315, 120);
            this.lblService_Hint.TabIndex = 12;
            this.lblService_Hint.Text = "Dependencies can delay startup at boot. Stopping a dependency may also require stopping ServiceManager and its managed applications. Custom: use Select... to pick the services.";
            // 
            // tabNotifications
            // 
            this.tabNotifications.Controls.Add(this.bnNotifications_Add);
            this.tabNotifications.Controls.Add(this.bnNotifications_Remove);
            this.tabNotifications.Controls.Add(this.bnNotifications_Configure);
            this.tabNotifications.Controls.Add(this.lstNotifications_Providers);
            this.tabNotifications.Controls.Add(this.label3);
            this.tabNotifications.Controls.Add(this.txtNotifications_InstanceName);
            this.tabNotifications.Controls.Add(this.label4);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnConsoleError);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnConsoleOutput);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnStop);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnRestart);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnServerShutdown);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnServerStart);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnStart);
            this.tabNotifications.Controls.Add(this.chkNotifications_OnDependencyWait);
            this.tabNotifications.Controls.Add(this.label2);
            this.tabNotifications.Controls.Add(this.label1);
            this.tabNotifications.Controls.Add(this.cmbNotification_Extensions);
            this.tabNotifications.Controls.Add(this.rbnNotifications_Save);
            this.tabNotifications.TabIndex = 2;
            this.tabNotifications.Location = new System.Drawing.Point(4, 22);
            this.tabNotifications.Name = "tabNotifications";
            this.tabNotifications.Size = new System.Drawing.Size(758, 401);
            this.tabNotifications.Text = "Notifications";
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
            this.chkNotifications_OnConsoleError.Location = new System.Drawing.Point(190, 316);
            this.chkNotifications_OnConsoleError.Name = "chkNotifications_OnConsoleError";
            this.chkNotifications_OnConsoleError.AutoSize = true;
            this.chkNotifications_OnConsoleError.TabIndex = 10;
            this.chkNotifications_OnConsoleError.Text = "On Console Error";
            // 
            // chkNotifications_OnConsoleOutput
            // 
            this.chkNotifications_OnConsoleOutput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnConsoleOutput.Location = new System.Drawing.Point(190, 292);
            this.chkNotifications_OnConsoleOutput.Name = "chkNotifications_OnConsoleOutput";
            this.chkNotifications_OnConsoleOutput.AutoSize = true;
            this.chkNotifications_OnConsoleOutput.TabIndex = 10;
            this.chkNotifications_OnConsoleOutput.Text = "On Console Output";
            // 
            // chkNotifications_OnStop
            // 
            this.chkNotifications_OnStop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnStop.Location = new System.Drawing.Point(31, 316);
            this.chkNotifications_OnStop.Name = "chkNotifications_OnStop";
            this.chkNotifications_OnStop.AutoSize = true;
            this.chkNotifications_OnStop.TabIndex = 10;
            this.chkNotifications_OnStop.Text = "On Stop";
            // 
            // chkNotifications_OnRestart
            // 
            this.chkNotifications_OnRestart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnRestart.Location = new System.Drawing.Point(31, 340);
            this.chkNotifications_OnRestart.Name = "chkNotifications_OnRestart";
            this.chkNotifications_OnRestart.AutoSize = true;
            this.chkNotifications_OnRestart.TabIndex = 10;
            this.chkNotifications_OnRestart.Text = "On Restart";
            // 
            // chkNotifications_OnServerShutdown
            // 
            this.chkNotifications_OnServerShutdown.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnServerShutdown.Location = new System.Drawing.Point(380, 316);
            this.chkNotifications_OnServerShutdown.Name = "chkNotifications_OnServerShutdown";
            this.chkNotifications_OnServerShutdown.AutoSize = true;
            this.chkNotifications_OnServerShutdown.TabIndex = 10;
            this.chkNotifications_OnServerShutdown.Text = "On Server Shutdown";
            // 
            // chkNotifications_OnServerStart
            // 
            this.chkNotifications_OnServerStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnServerStart.Location = new System.Drawing.Point(380, 292);
            this.chkNotifications_OnServerStart.Name = "chkNotifications_OnServerStart";
            this.chkNotifications_OnServerStart.AutoSize = true;
            this.chkNotifications_OnServerStart.TabIndex = 10;
            this.chkNotifications_OnServerStart.Text = "On Server Start";
            // 
            // chkNotifications_OnStart
            // 
            this.chkNotifications_OnStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnStart.Location = new System.Drawing.Point(31, 292);
            this.chkNotifications_OnStart.Name = "chkNotifications_OnStart";
            this.chkNotifications_OnStart.AutoSize = true;
            this.chkNotifications_OnStart.TabIndex = 10;
            this.chkNotifications_OnStart.Text = "On Start";
            // 
            // chkNotifications_OnDependencyWait
            // 
            this.chkNotifications_OnDependencyWait.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNotifications_OnDependencyWait.Location = new System.Drawing.Point(190, 340);
            this.chkNotifications_OnDependencyWait.Name = "chkNotifications_OnDependencyWait";
            this.chkNotifications_OnDependencyWait.AutoSize = true;
            this.chkNotifications_OnDependencyWait.TabIndex = 10;
            this.chkNotifications_OnDependencyWait.Text = "On Dependency Wait";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 268);
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
            this.cmbNotification_Extensions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNotification_Extensions.Location = new System.Drawing.Point(30, 87);
            this.cmbNotification_Extensions.Name = "cmbNotification_Extensions";
            this.cmbNotification_Extensions.FormattingEnabled = true;
            this.cmbNotification_Extensions.Size = new System.Drawing.Size(250, 21);
            this.cmbNotification_Extensions.TabIndex = 5;
            // 
            // rbnNotifications_Save
            // 
            this.rbnNotifications_Save.Location = new System.Drawing.Point(30, 372);
            this.rbnNotifications_Save.Name = "rbnNotifications_Save";
            this.rbnNotifications_Save.Size = new System.Drawing.Size(110, 24);
            this.rbnNotifications_Save.TabIndex = 4;
            this.rbnNotifications_Save.Text = "Save";
            this.rbnNotifications_Save.Click += new System.EventHandler(this.rbnNotifications_Save_Click);
            // 
            // bnNotifications_Configure
            // 
            this.bnNotifications_Configure.Location = new System.Drawing.Point(288, 181);
            this.bnNotifications_Configure.Name = "bnNotifications_Configure";
            this.bnNotifications_Configure.Size = new System.Drawing.Size(75, 28);
            this.bnNotifications_Configure.TabIndex = 15;
            this.bnNotifications_Configure.Text = "Configure";
            this.bnNotifications_Configure.Click += new System.EventHandler(this.bnNotifications_Configure_Click);
            // 
            // bnNotifications_Add
            // 
            this.bnNotifications_Add.Location = new System.Drawing.Point(288, 82);
            this.bnNotifications_Add.Name = "bnNotifications_Add";
            this.bnNotifications_Add.Size = new System.Drawing.Size(75, 28);
            this.bnNotifications_Add.TabIndex = 13;
            this.bnNotifications_Add.Text = "Add";
            this.bnNotifications_Add.UseVisualStyleBackColor = true;
            this.bnNotifications_Add.Click += new System.EventHandler(this.bnNotifications_Add_Click);
            // 
            // bnNotifications_Remove
            // 
            this.bnNotifications_Remove.Location = new System.Drawing.Point(288, 145);
            this.bnNotifications_Remove.Name = "bnNotifications_Remove";
            this.bnNotifications_Remove.Size = new System.Drawing.Size(75, 28);
            this.bnNotifications_Remove.TabIndex = 14;
            this.bnNotifications_Remove.Text = "Remove";
            this.bnNotifications_Remove.UseVisualStyleBackColor = true;
            this.bnNotifications_Remove.Click += new System.EventHandler(this.bnNotifications_Remove_Click);
            // 
            // lstNotifications_Providers
            // 
            this.lstNotifications_Providers.FormattingEnabled = true;
            this.lstNotifications_Providers.Location = new System.Drawing.Point(30, 145);
            this.lstNotifications_Providers.Name = "lstNotifications_Providers";
            this.lstNotifications_Providers.Size = new System.Drawing.Size(250, 108);
            this.lstNotifications_Providers.TabIndex = 16;
            this.lstNotifications_Providers.SelectedIndexChanged += new System.EventHandler(this.lstNotifications_Providers_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 125);
            this.label3.Name = "label3";
            this.label3.TabIndex = 17;
            this.label3.Text = "Active providers (select one to edit its events):";
            // 
            // FrmManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 451);
            this.Controls.Add(this.tabMain);
            this.Name = "FrmManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage";
            this.Load += new System.EventHandler(this.frmManage_Load);
            this.cmsMenu.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabServices.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.tabWindowsService.ResumeLayout(false);
            this.grpWindowsService.ResumeLayout(false);
            this.grpWindowsService.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartup_Delay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDependency_Wait_Timeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).EndInit();
            this.tabNotifications.ResumeLayout(false);
            this.tabNotifications.PerformLayout();
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
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabServices;
        private System.Windows.Forms.Label lblRunMode;
        private System.Windows.Forms.RadioButton rrbMode_NetPipes;
        private System.Windows.Forms.RadioButton rrbMode_TCP;
        private System.Windows.Forms.NumericUpDown nud_TCPPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Button bnAddService;
        private System.Windows.Forms.Button bnSettings_Save;
        private System.Windows.Forms.NumericUpDown nudStartup_Delay;
        private System.Windows.Forms.Label lblDelayUnit;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown nudDependency_Wait_Timeout;
        private System.Windows.Forms.Label lblDependencyTimeoutUnit;
        private System.Windows.Forms.Label lblDependencyTimeout;
        private System.Windows.Forms.CheckBox chkRequireAdministrator;
        private System.Windows.Forms.TabPage tabWindowsService;
        private System.Windows.Forms.Button bnWindowsService_Save;
        private System.Windows.Forms.GroupBox grpWindowsService;
        private System.Windows.Forms.Label lblService_StartMode;
        private System.Windows.Forms.ComboBox cmbService_StartMode;
        private System.Windows.Forms.Label lblService_Account;
        private System.Windows.Forms.ComboBox cmbService_Account;
        private System.Windows.Forms.Label lblService_User;
        private System.Windows.Forms.TextBox txtService_User;
        private System.Windows.Forms.Label lblService_Password;
        private System.Windows.Forms.TextBox txtService_Password;
        private System.Windows.Forms.Label lblService_Dependencies;
        private System.Windows.Forms.ComboBox cmbService_Dependencies;
        private System.Windows.Forms.ListView lsvService_Dependencies;
        private System.Windows.Forms.ColumnHeader colDependency_Title;
        private System.Windows.Forms.ColumnHeader colDependency_Name;
        private System.Windows.Forms.ColumnHeader colDependency_State;
        private System.Windows.Forms.Button bnService_Dependencies_Select;
        private System.Windows.Forms.Label lblService_Hint;
        private System.Windows.Forms.TabPage tabNotifications;
        private System.Windows.Forms.Button rbnNotifications_Save;
        private System.Windows.Forms.ComboBox cmbNotification_Extensions;
        
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkNotifications_OnStart;
        private System.Windows.Forms.CheckBox chkNotifications_OnDependencyWait;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkNotifications_OnConsoleError;
        private System.Windows.Forms.CheckBox chkNotifications_OnConsoleOutput;
        private System.Windows.Forms.CheckBox chkNotifications_OnStop;
        private System.Windows.Forms.CheckBox chkNotifications_OnRestart;
        private System.Windows.Forms.TextBox txtNotifications_InstanceName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkNotifications_OnServerShutdown;
        private System.Windows.Forms.CheckBox chkNotifications_OnServerStart;
        private System.Windows.Forms.Button bnNotifications_Configure;
        private System.Windows.Forms.Button bnNotifications_Add;
        private System.Windows.Forms.Button bnNotifications_Remove;
        private System.Windows.Forms.ListBox lstNotifications_Providers;
        private System.Windows.Forms.Label label3;
    }
}