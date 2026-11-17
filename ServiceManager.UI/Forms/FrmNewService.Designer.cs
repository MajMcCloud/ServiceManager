namespace ServiceManager.UI.Forms
{
    partial class FrmNewService
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
            this.bnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ofdOpen = new System.Windows.Forms.OpenFileDialog();
            this.bnSelectPath = new System.Windows.Forms.Button();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabBasic = new System.Windows.Forms.TabPage();
            this.tabStart = new System.Windows.Forms.TabPage();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkAutoRestart = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabShutdown = new System.Windows.Forms.TabPage();
            this.chkShutdown_SendENTER = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.nudShutdown_ENTER_Timeout = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.nudShutdown_Timeout = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tabLogging = new System.Windows.Forms.TabPage();
            this.chkLogs_ResetDaily = new System.Windows.Forms.CheckBox();
            this.chkLogs_ToMemory = new System.Windows.Forms.CheckBox();
            this.chkLogs_ToDisk = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tabDependencies = new System.Windows.Forms.TabPage();
            this.lsvDependencies = new System.Windows.Forms.ListView();
            this.colDependency_Title = new System.Windows.Forms.ColumnHeader();
            this.colDependency_Name = new System.Windows.Forms.ColumnHeader();
            this.colDependency_State = new System.Windows.Forms.ColumnHeader();
            this.bnDependencies_Select = new System.Windows.Forms.Button();
            this.chkDependencies_WaitOnRestart = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tabServiceDependencies = new System.Windows.Forms.TabPage();
            this.lsvServiceDependencies = new System.Windows.Forms.ListView();
            this.colServiceDependency_Title = new System.Windows.Forms.ColumnHeader();
            this.colServiceDependency_Path = new System.Windows.Forms.ColumnHeader();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.cmbDependency_Restart = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.cmbDependency_Start = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.cmbDependency_Stop = new System.Windows.Forms.ComboBox();
            this.tabRunAs = new System.Windows.Forms.TabPage();
            this.lblRunAs_Account = new System.Windows.Forms.Label();
            this.cmbRunAs = new System.Windows.Forms.ComboBox();
            this.lblRunAs_User = new System.Windows.Forms.Label();
            this.txtRunAs_User = new System.Windows.Forms.TextBox();
            this.lblRunAs_Domain = new System.Windows.Forms.Label();
            this.txtRunAs_Domain = new System.Windows.Forms.TextBox();
            this.lblRunAs_Password = new System.Windows.Forms.Label();
            this.txtRunAs_Password = new System.Windows.Forms.TextBox();
            this.bnRunAs_Select = new System.Windows.Forms.Button();
            this.lblRunAs_Hint = new System.Windows.Forms.Label();
            this.lblRunAs_Right = new System.Windows.Forms.Label();
            this.lblRunAs_Profile = new System.Windows.Forms.Label();
            this.tabMain.SuspendLayout();
            this.tabBasic.SuspendLayout();
            this.tabStart.SuspendLayout();
            this.tabRunAs.SuspendLayout();
            this.tabShutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_ENTER_Timeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_Timeout)).BeginInit();
            this.tabLogging.SuspendLayout();
            this.tabDependencies.SuspendLayout();
            this.tabServiceDependencies.SuspendLayout();
            this.SuspendLayout();
            // 
            // bnSave
            // 
            this.bnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnSave.Location = new System.Drawing.Point(612, 437);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(91, 23);
            this.bnSave.TabIndex = 0;
            this.bnSave.Text = "Save";
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "ID";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(285, 104);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(256, 52);
            this.txtDescription.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 107);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Description";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 178);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Path";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(285, 178);
            this.txtPath.Multiline = true;
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(256, 52);
            this.txtPath.TabIndex = 2;
            this.txtPath.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtPath_MouseDoubleClick);
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(282, 17);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(18, 13);
            this.lblID.TabIndex = 1;
            this.lblID.Text = "ID";
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(285, 58);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(256, 20);
            this.txtTitle.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Title";
            // 
            // ofdOpen
            // 
            this.ofdOpen.Filter = "All programs *.*|*.*";
            this.ofdOpen.Title = "Choose program";
            // 
            // bnSelectPath
            // 
            this.bnSelectPath.Location = new System.Drawing.Point(285, 247);
            this.bnSelectPath.Name = "bnSelectPath";
            this.bnSelectPath.Size = new System.Drawing.Size(75, 23);
            this.bnSelectPath.TabIndex = 4;
            this.bnSelectPath.Text = "Select";
            this.bnSelectPath.Click += new System.EventHandler(this.bnSelectPath_Click);
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabBasic);
            this.tabMain.Controls.Add(this.tabStart);
            this.tabMain.Controls.Add(this.tabRunAs);
            this.tabMain.Controls.Add(this.tabShutdown);
            this.tabMain.Controls.Add(this.tabLogging);
            this.tabMain.Controls.Add(this.tabDependencies);
            this.tabMain.Controls.Add(this.tabServiceDependencies);
            this.tabMain.Location = new System.Drawing.Point(12, 12);
            this.tabMain.Name = "tabMain";
            this.tabMain.Size = new System.Drawing.Size(691, 419);
            this.tabMain.TabIndex = 6;
            // 
            // tabBasic
            // 
            this.tabBasic.Controls.Add(this.label1);
            this.tabBasic.Controls.Add(this.lblID);
            this.tabBasic.Controls.Add(this.label2);
            this.tabBasic.Controls.Add(this.bnSelectPath);
            this.tabBasic.Controls.Add(this.txtTitle);
            this.tabBasic.Controls.Add(this.txtPath);
            this.tabBasic.Controls.Add(this.label4);
            this.tabBasic.Controls.Add(this.txtDescription);
            this.tabBasic.Controls.Add(this.label3);
            this.tabBasic.Location = new System.Drawing.Point(4, 22);
            this.tabBasic.Name = "tabBasic";
            this.tabBasic.Size = new System.Drawing.Size(683, 393);
            this.tabBasic.TabIndex = 0;
            this.tabBasic.Text = "Basic";
            // 
            // tabStart
            // 
            this.tabStart.Controls.Add(this.chkEnabled);
            this.tabStart.Controls.Add(this.label5);
            this.tabStart.Controls.Add(this.chkAutoRestart);
            this.tabStart.Controls.Add(this.label6);
            this.tabStart.Location = new System.Drawing.Point(4, 22);
            this.tabStart.Name = "tabStart";
            this.tabStart.Size = new System.Drawing.Size(683, 393);
            this.tabStart.TabIndex = 1;
            this.tabStart.Text = "Start";
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Checked = true;
            this.chkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnabled.Location = new System.Drawing.Point(285, 37);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 38);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Start on system startup";
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(285, 76);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Automatic restart";
            // 
            // tabRunAs
            // 
            this.tabRunAs.Controls.Add(this.lblRunAs_Account);
            this.tabRunAs.Controls.Add(this.cmbRunAs);
            this.tabRunAs.Controls.Add(this.lblRunAs_User);
            this.tabRunAs.Controls.Add(this.txtRunAs_User);
            this.tabRunAs.Controls.Add(this.lblRunAs_Domain);
            this.tabRunAs.Controls.Add(this.txtRunAs_Domain);
            this.tabRunAs.Controls.Add(this.lblRunAs_Password);
            this.tabRunAs.Controls.Add(this.txtRunAs_Password);
            this.tabRunAs.Controls.Add(this.bnRunAs_Select);
            this.tabRunAs.Controls.Add(this.lblRunAs_Hint);
            this.tabRunAs.Controls.Add(this.lblRunAs_Right);
            this.tabRunAs.Controls.Add(this.lblRunAs_Profile);
            this.tabRunAs.Location = new System.Drawing.Point(4, 22);
            this.tabRunAs.Name = "tabRunAs";
            this.tabRunAs.Size = new System.Drawing.Size(683, 393);
            this.tabRunAs.TabIndex = 2;
            this.tabRunAs.Text = "Run as";
            // 
            // lblRunAs_Account
            // 
            this.lblRunAs_Account.AutoSize = true;
            this.lblRunAs_Account.Location = new System.Drawing.Point(22, 26);
            this.lblRunAs_Account.Name = "lblRunAs_Account";
            this.lblRunAs_Account.Size = new System.Drawing.Size(50, 13);
            this.lblRunAs_Account.TabIndex = 0;
            this.lblRunAs_Account.Text = "Account";
            // 
            // cmbRunAs
            // 
            this.cmbRunAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRunAs.Location = new System.Drawing.Point(285, 23);
            this.cmbRunAs.Name = "cmbRunAs";
            this.cmbRunAs.Size = new System.Drawing.Size(180, 21);
            this.cmbRunAs.TabIndex = 1;
            this.cmbRunAs.SelectedIndexChanged += new System.EventHandler(this.cmbRunAs_SelectedIndexChanged);
            // 
            // lblRunAs_User
            // 
            this.lblRunAs_User.AutoSize = true;
            this.lblRunAs_User.Location = new System.Drawing.Point(22, 59);
            this.lblRunAs_User.Name = "lblRunAs_User";
            this.lblRunAs_User.Size = new System.Drawing.Size(60, 13);
            this.lblRunAs_User.TabIndex = 2;
            this.lblRunAs_User.Text = "User name";
            // 
            // txtRunAs_User
            // 
            this.txtRunAs_User.Location = new System.Drawing.Point(285, 56);
            this.txtRunAs_User.Name = "txtRunAs_User";
            this.txtRunAs_User.Size = new System.Drawing.Size(220, 20);
            this.txtRunAs_User.TabIndex = 3;
            // 
            // lblRunAs_Domain
            // 
            this.lblRunAs_Domain.AutoSize = true;
            this.lblRunAs_Domain.Location = new System.Drawing.Point(22, 89);
            this.lblRunAs_Domain.Name = "lblRunAs_Domain";
            this.lblRunAs_Domain.Size = new System.Drawing.Size(45, 13);
            this.lblRunAs_Domain.TabIndex = 4;
            this.lblRunAs_Domain.Text = "Domain";
            // 
            // txtRunAs_Domain
            // 
            this.txtRunAs_Domain.Location = new System.Drawing.Point(285, 86);
            this.txtRunAs_Domain.Name = "txtRunAs_Domain";
            this.txtRunAs_Domain.Size = new System.Drawing.Size(220, 20);
            this.txtRunAs_Domain.TabIndex = 5;
            // 
            // lblRunAs_Password
            // 
            this.lblRunAs_Password.AutoSize = true;
            this.lblRunAs_Password.Location = new System.Drawing.Point(22, 119);
            this.lblRunAs_Password.Name = "lblRunAs_Password";
            this.lblRunAs_Password.Size = new System.Drawing.Size(56, 13);
            this.lblRunAs_Password.TabIndex = 6;
            this.lblRunAs_Password.Text = "Password";
            // 
            // txtRunAs_Password
            // 
            this.txtRunAs_Password.Location = new System.Drawing.Point(285, 116);
            this.txtRunAs_Password.Name = "txtRunAs_Password";
            this.txtRunAs_Password.PasswordChar = '*';
            this.txtRunAs_Password.Size = new System.Drawing.Size(220, 20);
            this.txtRunAs_Password.TabIndex = 7;
            // 
            // bnRunAs_Select
            // 
            this.bnRunAs_Select.Location = new System.Drawing.Point(515, 55);
            this.bnRunAs_Select.Name = "bnRunAs_Select";
            this.bnRunAs_Select.Size = new System.Drawing.Size(110, 23);
            this.bnRunAs_Select.TabIndex = 8;
            this.bnRunAs_Select.Text = "Select account";
            this.bnRunAs_Select.UseVisualStyleBackColor = true;
            this.bnRunAs_Select.Click += new System.EventHandler(this.bnRunAs_Select_Click);
            // 
            // lblRunAs_Hint
            // 
            this.lblRunAs_Hint.AutoSize = true;
            this.lblRunAs_Hint.Location = new System.Drawing.Point(22, 152);
            this.lblRunAs_Hint.Name = "lblRunAs_Hint";
            this.lblRunAs_Hint.Size = new System.Drawing.Size(380, 13);
            this.lblRunAs_Hint.TabIndex = 9;
            this.lblRunAs_Hint.Text = "Without an account the process runs under the account of the ServiceManager.";
            // 
            // lblRunAs_Right
            // 
            this.lblRunAs_Right.AutoSize = true;
            this.lblRunAs_Right.Location = new System.Drawing.Point(22, 172);
            this.lblRunAs_Right.Name = "lblRunAs_Right";
            this.lblRunAs_Right.Size = new System.Drawing.Size(330, 13);
            this.lblRunAs_Right.TabIndex = 10;
            this.lblRunAs_Right.Text = "The account has to be allowed to sign in on this computer.";
            // 
            // lblRunAs_Profile
            // 
            this.lblRunAs_Profile.AutoSize = true;
            this.lblRunAs_Profile.Location = new System.Drawing.Point(22, 192);
            this.lblRunAs_Profile.Name = "lblRunAs_Profile";
            this.lblRunAs_Profile.Size = new System.Drawing.Size(250, 13);
            this.lblRunAs_Profile.TabIndex = 11;
            this.lblRunAs_Profile.Text = "The profile of the account is loaded for the process.";
            // 
            // tabShutdown
            // 
            this.tabShutdown.Controls.Add(this.chkShutdown_SendENTER);
            this.tabShutdown.Controls.Add(this.label9);
            this.tabShutdown.Controls.Add(this.nudShutdown_ENTER_Timeout);
            this.tabShutdown.Controls.Add(this.label11);
            this.tabShutdown.Controls.Add(this.nudShutdown_Timeout);
            this.tabShutdown.Controls.Add(this.label10);
            this.tabShutdown.Controls.Add(this.label8);
            this.tabShutdown.Controls.Add(this.label7);
            this.tabShutdown.Location = new System.Drawing.Point(4, 22);
            this.tabShutdown.Name = "tabShutdown";
            this.tabShutdown.Size = new System.Drawing.Size(683, 393);
            this.tabShutdown.TabIndex = 2;
            this.tabShutdown.Text = "Shutdown";
            // 
            // chkShutdown_SendENTER
            // 
            this.chkShutdown_SendENTER.AutoSize = true;
            this.chkShutdown_SendENTER.Location = new System.Drawing.Point(285, 82);
            this.chkShutdown_SendENTER.Name = "chkShutdown_SendENTER";
            this.chkShutdown_SendENTER.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(22, 83);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(193, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Send ENTER to close the application";
            // 
            // nudShutdown_ENTER_Timeout
            // 
            this.nudShutdown_ENTER_Timeout.Location = new System.Drawing.Point(285, 112);
            this.nudShutdown_ENTER_Timeout.Name = "nudShutdown_ENTER_Timeout";
            this.nudShutdown_ENTER_Timeout.Size = new System.Drawing.Size(120, 20);
            this.nudShutdown_ENTER_Timeout.TabIndex = 11;
            this.nudShutdown_ENTER_Timeout.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(411, 114);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(49, 13);
            this.label11.TabIndex = 9;
            this.label11.Text = "seconds";
            // 
            // nudShutdown_Timeout
            // 
            this.nudShutdown_Timeout.Location = new System.Drawing.Point(285, 36);
            this.nudShutdown_Timeout.Name = "nudShutdown_Timeout";
            this.nudShutdown_Timeout.Size = new System.Drawing.Size(120, 20);
            this.nudShutdown_Timeout.TabIndex = 11;
            this.nudShutdown_Timeout.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(22, 114);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Timeout";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(411, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "seconds";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Timeout";
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.chkLogs_ResetDaily);
            this.tabLogging.Controls.Add(this.chkLogs_ToMemory);
            this.tabLogging.Controls.Add(this.chkLogs_ToDisk);
            this.tabLogging.Controls.Add(this.label13);
            this.tabLogging.Controls.Add(this.label14);
            this.tabLogging.Controls.Add(this.label12);
            this.tabLogging.Location = new System.Drawing.Point(4, 22);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Size = new System.Drawing.Size(683, 393);
            this.tabLogging.TabIndex = 3;
            this.tabLogging.Text = "Logging";
            // 
            // chkLogs_ResetDaily
            // 
            this.chkLogs_ResetDaily.AutoSize = true;
            this.chkLogs_ResetDaily.Checked = true;
            this.chkLogs_ResetDaily.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogs_ResetDaily.Location = new System.Drawing.Point(285, 74);
            this.chkLogs_ResetDaily.Name = "chkLogs_ResetDaily";
            this.chkLogs_ResetDaily.TabIndex = 11;
            // 
            // chkLogs_ToMemory
            // 
            this.chkLogs_ToMemory.AutoSize = true;
            this.chkLogs_ToMemory.Checked = true;
            this.chkLogs_ToMemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogs_ToMemory.Location = new System.Drawing.Point(285, 21);
            this.chkLogs_ToMemory.Name = "chkLogs_ToMemory";
            this.chkLogs_ToMemory.TabIndex = 11;
            this.chkLogs_ToMemory.CheckedChanged += new System.EventHandler(this.chkLogs_ToMemory_CheckedChanged);
            // 
            // chkLogs_ToDisk
            // 
            this.chkLogs_ToDisk.AutoSize = true;
            this.chkLogs_ToDisk.Location = new System.Drawing.Point(285, 47);
            this.chkLogs_ToDisk.Name = "chkLogs_ToDisk";
            this.chkLogs_ToDisk.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(22, 75);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(147, 13);
            this.label13.TabIndex = 10;
            this.label13.Text = "Clear console outputs daily";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(22, 22);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(170, 13);
            this.label14.TabIndex = 10;
            this.label14.Text = "Log console outputs to memory";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(22, 48);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(151, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "Log console outputs to disk";
            // 
            // tabDependencies
            // 
            this.tabDependencies.Controls.Add(this.lsvDependencies);
            this.tabDependencies.Controls.Add(this.bnDependencies_Select);
            this.tabDependencies.Controls.Add(this.chkDependencies_WaitOnRestart);
            this.tabDependencies.Controls.Add(this.label15);
            this.tabDependencies.Location = new System.Drawing.Point(4, 22);
            this.tabDependencies.Name = "tabDependencies";
            this.tabDependencies.Size = new System.Drawing.Size(683, 393);
            this.tabDependencies.TabIndex = 4;
            this.tabDependencies.Text = "Dependencies";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(22, 22);
            this.label15.Name = "label15";
            this.label15.TabIndex = 9;
            this.label15.Text = "Windows services that have to run first";
            // 
            // lsvDependencies
            // 
            this.lsvDependencies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDependency_Title,
            this.colDependency_Name,
            this.colDependency_State});
            this.lsvDependencies.FullRowSelect = true;
            this.lsvDependencies.HideSelection = false;
            this.lsvDependencies.Location = new System.Drawing.Point(22, 45);
            this.lsvDependencies.MultiSelect = false;
            this.lsvDependencies.Name = "lsvDependencies";
            this.lsvDependencies.Size = new System.Drawing.Size(639, 250);
            this.lsvDependencies.TabIndex = 10;
            this.lsvDependencies.UseCompatibleStateImageBehavior = false;
            this.lsvDependencies.View = System.Windows.Forms.View.Details;
            // 
            // colDependency_Title
            // 
            this.colDependency_Title.Text = "Display name";
            this.colDependency_Title.Width = 240;
            // 
            // colDependency_Name
            // 
            this.colDependency_Name.Text = "Service name";
            this.colDependency_Name.Width = 200;
            // 
            // colDependency_State
            // 
            this.colDependency_State.Text = "Status";
            this.colDependency_State.Width = 120;
            // 
            // bnDependencies_Select
            // 
            this.bnDependencies_Select.Location = new System.Drawing.Point(22, 301);
            this.bnDependencies_Select.Name = "bnDependencies_Select";
            this.bnDependencies_Select.Size = new System.Drawing.Size(75, 23);
            this.bnDependencies_Select.TabIndex = 11;
            this.bnDependencies_Select.Text = "Select";
            this.bnDependencies_Select.Click += new System.EventHandler(this.bnDependencies_Select_Click);
            // 
            // chkDependencies_WaitOnRestart
            // 
            this.chkDependencies_WaitOnRestart.AutoSize = true;
            this.chkDependencies_WaitOnRestart.Checked = true;
            this.chkDependencies_WaitOnRestart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDependencies_WaitOnRestart.Location = new System.Drawing.Point(110, 304);
            this.chkDependencies_WaitOnRestart.Name = "chkDependencies_WaitOnRestart";
            this.chkDependencies_WaitOnRestart.TabIndex = 12;
            this.chkDependencies_WaitOnRestart.Text = "Wait for the dependencies after an automatic restart";
            // 
            // tabServiceDependencies
            // 
            this.tabServiceDependencies.Controls.Add(this.lsvServiceDependencies);
            this.tabServiceDependencies.Controls.Add(this.label16);
            this.tabServiceDependencies.Controls.Add(this.label17);
            this.tabServiceDependencies.Controls.Add(this.cmbDependency_Restart);
            this.tabServiceDependencies.Controls.Add(this.label18);
            this.tabServiceDependencies.Controls.Add(this.cmbDependency_Start);
            this.tabServiceDependencies.Controls.Add(this.label19);
            this.tabServiceDependencies.Controls.Add(this.cmbDependency_Stop);
            this.tabServiceDependencies.Location = new System.Drawing.Point(4, 22);
            this.tabServiceDependencies.Name = "tabServiceDependencies";
            this.tabServiceDependencies.Size = new System.Drawing.Size(683, 393);
            this.tabServiceDependencies.TabIndex = 5;
            this.tabServiceDependencies.Text = "Depends on";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(22, 22);
            this.label16.Name = "label16";
            this.label16.TabIndex = 9;
            this.label16.Text = "Other services that have to run first";
            // 
            // lsvServiceDependencies
            // 
            this.lsvServiceDependencies.CheckBoxes = true;
            this.lsvServiceDependencies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colServiceDependency_Title,
            this.colServiceDependency_Path});
            this.lsvServiceDependencies.FullRowSelect = true;
            this.lsvServiceDependencies.HideSelection = false;
            this.lsvServiceDependencies.Location = new System.Drawing.Point(22, 45);
            this.lsvServiceDependencies.Name = "lsvServiceDependencies";
            this.lsvServiceDependencies.Size = new System.Drawing.Size(639, 250);
            this.lsvServiceDependencies.TabIndex = 10;
            this.lsvServiceDependencies.UseCompatibleStateImageBehavior = false;
            this.lsvServiceDependencies.View = System.Windows.Forms.View.Details;
            // 
            // colServiceDependency_Title
            // 
            this.colServiceDependency_Title.Text = "Title";
            this.colServiceDependency_Title.Width = 240;
            // 
            // colServiceDependency_Path
            // 
            this.colServiceDependency_Path.Text = "Path";
            this.colServiceDependency_Path.Width = 380;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(22, 308);
            this.label17.Name = "label17";
            this.label17.TabIndex = 11;
            this.label17.Text = "When a dependency restarts";
            // 
            // cmbDependency_Restart
            // 
            this.cmbDependency_Restart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDependency_Restart.Location = new System.Drawing.Point(285, 305);
            this.cmbDependency_Restart.Name = "cmbDependency_Restart";
            this.cmbDependency_Restart.Size = new System.Drawing.Size(180, 21);
            this.cmbDependency_Restart.TabIndex = 12;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(22, 338);
            this.label18.Name = "label18";
            this.label18.TabIndex = 13;
            this.label18.Text = "When a dependency did not start";
            // 
            // cmbDependency_Start
            // 
            this.cmbDependency_Start.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDependency_Start.Location = new System.Drawing.Point(285, 335);
            this.cmbDependency_Start.Name = "cmbDependency_Start";
            this.cmbDependency_Start.Size = new System.Drawing.Size(180, 21);
            this.cmbDependency_Start.TabIndex = 14;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(22, 368);
            this.label19.Name = "label19";
            this.label19.TabIndex = 15;
            this.label19.Text = "When a dependency stops";
            // 
            // cmbDependency_Stop
            // 
            this.cmbDependency_Stop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDependency_Stop.Location = new System.Drawing.Point(285, 365);
            this.cmbDependency_Stop.Name = "cmbDependency_Stop";
            this.cmbDependency_Stop.Size = new System.Drawing.Size(180, 21);
            this.cmbDependency_Stop.TabIndex = 16;
            // 
            // FrmNewService
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 472);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.bnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmNewService";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Service";
            this.Load += new System.EventHandler(this.frmNewService_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmNewService_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmNewService_DragEnter);
            this.tabMain.ResumeLayout(false);
            this.tabBasic.ResumeLayout(false);
            this.tabBasic.PerformLayout();
            this.tabStart.ResumeLayout(false);
            this.tabStart.PerformLayout();
            this.tabRunAs.ResumeLayout(false);
            this.tabRunAs.PerformLayout();
            this.tabShutdown.ResumeLayout(false);
            this.tabShutdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_ENTER_Timeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_Timeout)).EndInit();
            this.tabLogging.ResumeLayout(false);
            this.tabLogging.PerformLayout();
            this.tabDependencies.ResumeLayout(false);
            this.tabDependencies.PerformLayout();
            this.tabServiceDependencies.ResumeLayout(false);
            this.tabServiceDependencies.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.OpenFileDialog ofdOpen;
        private System.Windows.Forms.Button bnSelectPath;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabBasic;
        private System.Windows.Forms.TabPage tabShutdown;
        private System.Windows.Forms.TabPage tabStart;
        private System.Windows.Forms.CheckBox chkAutoRestart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudShutdown_Timeout;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkShutdown_SendENTER;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudShutdown_ENTER_Timeout;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TabPage tabLogging;
        private System.Windows.Forms.CheckBox chkLogs_ToDisk;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox chkLogs_ResetDaily;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox chkLogs_ToMemory;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabPage tabDependencies;
        private System.Windows.Forms.ListView lsvDependencies;
        private System.Windows.Forms.ColumnHeader colDependency_Title;
        private System.Windows.Forms.ColumnHeader colDependency_Name;
        private System.Windows.Forms.ColumnHeader colDependency_State;
        private System.Windows.Forms.Button bnDependencies_Select;
        private System.Windows.Forms.CheckBox chkDependencies_WaitOnRestart;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TabPage tabServiceDependencies;
        private System.Windows.Forms.ListView lsvServiceDependencies;
        private System.Windows.Forms.ColumnHeader colServiceDependency_Title;
        private System.Windows.Forms.ColumnHeader colServiceDependency_Path;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmbDependency_Restart;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ComboBox cmbDependency_Start;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ComboBox cmbDependency_Stop;
        private System.Windows.Forms.TabPage tabRunAs;
        private System.Windows.Forms.Label lblRunAs_Account;
        private System.Windows.Forms.ComboBox cmbRunAs;
        private System.Windows.Forms.Label lblRunAs_User;
        private System.Windows.Forms.TextBox txtRunAs_User;
        private System.Windows.Forms.Label lblRunAs_Domain;
        private System.Windows.Forms.TextBox txtRunAs_Domain;
        private System.Windows.Forms.Label lblRunAs_Password;
        private System.Windows.Forms.TextBox txtRunAs_Password;
        private System.Windows.Forms.Button bnRunAs_Select;
        private System.Windows.Forms.Label lblRunAs_Hint;
        private System.Windows.Forms.Label lblRunAs_Right;
        private System.Windows.Forms.Label lblRunAs_Profile;
    }
}