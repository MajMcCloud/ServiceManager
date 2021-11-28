using Telerik.WinControls.UI;

namespace ServiceManager.UI.forms
{
    partial class frmNewService
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
            this.bnSave = new Telerik.WinControls.UI.RadButton();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ofdOpen = new System.Windows.Forms.OpenFileDialog();
            this.bnSelectPath = new Telerik.WinControls.UI.RadButton();
            this.radPageView1 = new Telerik.WinControls.UI.RadPageView();
            this.rpvBasic = new Telerik.WinControls.UI.RadPageViewPage();
            this.rpbStart = new Telerik.WinControls.UI.RadPageViewPage();
            this.chkEnabled = new Telerik.WinControls.UI.RadCheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkAutoRestart = new Telerik.WinControls.UI.RadCheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rpvShutdown = new Telerik.WinControls.UI.RadPageViewPage();
            this.chkShutdown_SendENTER = new Telerik.WinControls.UI.RadCheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.nudShutdown_ENTER_Timeout = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.nudShutdown_Timeout = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.radPageViewPage1 = new Telerik.WinControls.UI.RadPageViewPage();
            this.chkLogs_ResetDaily = new Telerik.WinControls.UI.RadCheckBox();
            this.chkLogs_ToMemory = new Telerik.WinControls.UI.RadCheckBox();
            this.chkLogs_ToDisk = new Telerik.WinControls.UI.RadCheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.bnSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnSelectPath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPageView1)).BeginInit();
            this.radPageView1.SuspendLayout();
            this.rpvBasic.SuspendLayout();
            this.rpbStart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnabled)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoRestart)).BeginInit();
            this.rpvShutdown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShutdown_SendENTER)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_ENTER_Timeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_Timeout)).BeginInit();
            this.radPageViewPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ResetDaily)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ToMemory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ToDisk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
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
            // radPageView1
            // 
            this.radPageView1.Controls.Add(this.rpvBasic);
            this.radPageView1.Controls.Add(this.rpbStart);
            this.radPageView1.Controls.Add(this.rpvShutdown);
            this.radPageView1.Controls.Add(this.radPageViewPage1);
            this.radPageView1.DefaultPage = this.rpvBasic;
            this.radPageView1.Location = new System.Drawing.Point(12, 12);
            this.radPageView1.Name = "radPageView1";
            this.radPageView1.SelectedPage = this.rpbStart;
            this.radPageView1.Size = new System.Drawing.Size(691, 419);
            this.radPageView1.TabIndex = 6;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).StripButtons = Telerik.WinControls.UI.StripViewButtons.None;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).ItemAlignment = Telerik.WinControls.UI.StripViewItemAlignment.Near;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).ItemFitMode = Telerik.WinControls.UI.StripViewItemFitMode.None;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.radPageView1.GetChildAt(0))).StripAlignment = Telerik.WinControls.UI.StripViewAlignment.Top;
            // 
            // rpvBasic
            // 
            this.rpvBasic.Controls.Add(this.label1);
            this.rpvBasic.Controls.Add(this.lblID);
            this.rpvBasic.Controls.Add(this.label2);
            this.rpvBasic.Controls.Add(this.bnSelectPath);
            this.rpvBasic.Controls.Add(this.txtTitle);
            this.rpvBasic.Controls.Add(this.txtPath);
            this.rpvBasic.Controls.Add(this.label4);
            this.rpvBasic.Controls.Add(this.txtDescription);
            this.rpvBasic.Controls.Add(this.label3);
            this.rpvBasic.ItemSize = new System.Drawing.SizeF(41F, 28F);
            this.rpvBasic.Location = new System.Drawing.Point(10, 37);
            this.rpvBasic.Name = "rpvBasic";
            this.rpvBasic.Size = new System.Drawing.Size(670, 371);
            this.rpvBasic.Text = "Basic";
            // 
            // rpbStart
            // 
            this.rpbStart.Controls.Add(this.chkEnabled);
            this.rpbStart.Controls.Add(this.label5);
            this.rpbStart.Controls.Add(this.chkAutoRestart);
            this.rpbStart.Controls.Add(this.label6);
            this.rpbStart.ItemSize = new System.Drawing.SizeF(40F, 28F);
            this.rpbStart.Location = new System.Drawing.Point(10, 37);
            this.rpbStart.Name = "rpbStart";
            this.rpbStart.Size = new System.Drawing.Size(670, 371);
            this.rpbStart.Text = "Start";
            // 
            // chkEnabled
            // 
            this.chkEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnabled.Location = new System.Drawing.Point(285, 37);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(15, 15);
            this.chkEnabled.TabIndex = 9;
            this.chkEnabled.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
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
            this.chkAutoRestart.Location = new System.Drawing.Point(285, 76);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(15, 15);
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
            // rpvShutdown
            // 
            this.rpvShutdown.Controls.Add(this.chkShutdown_SendENTER);
            this.rpvShutdown.Controls.Add(this.label9);
            this.rpvShutdown.Controls.Add(this.nudShutdown_ENTER_Timeout);
            this.rpvShutdown.Controls.Add(this.label11);
            this.rpvShutdown.Controls.Add(this.nudShutdown_Timeout);
            this.rpvShutdown.Controls.Add(this.label10);
            this.rpvShutdown.Controls.Add(this.label8);
            this.rpvShutdown.Controls.Add(this.label7);
            this.rpvShutdown.ItemSize = new System.Drawing.SizeF(67F, 28F);
            this.rpvShutdown.Location = new System.Drawing.Point(10, 37);
            this.rpvShutdown.Name = "rpvShutdown";
            this.rpvShutdown.Size = new System.Drawing.Size(670, 371);
            this.rpvShutdown.Text = "Shutdown";
            // 
            // chkShutdown_SendENTER
            // 
            this.chkShutdown_SendENTER.Location = new System.Drawing.Point(285, 82);
            this.chkShutdown_SendENTER.Name = "chkShutdown_SendENTER";
            this.chkShutdown_SendENTER.Size = new System.Drawing.Size(15, 15);
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
            // radPageViewPage1
            // 
            this.radPageViewPage1.Controls.Add(this.chkLogs_ResetDaily);
            this.radPageViewPage1.Controls.Add(this.chkLogs_ToMemory);
            this.radPageViewPage1.Controls.Add(this.chkLogs_ToDisk);
            this.radPageViewPage1.Controls.Add(this.label13);
            this.radPageViewPage1.Controls.Add(this.label14);
            this.radPageViewPage1.Controls.Add(this.label12);
            this.radPageViewPage1.ItemSize = new System.Drawing.SizeF(57F, 28F);
            this.radPageViewPage1.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage1.Name = "radPageViewPage1";
            this.radPageViewPage1.Size = new System.Drawing.Size(670, 371);
            this.radPageViewPage1.Text = "Logging";
            // 
            // chkLogs_ResetDaily
            // 
            this.chkLogs_ResetDaily.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogs_ResetDaily.Location = new System.Drawing.Point(285, 74);
            this.chkLogs_ResetDaily.Name = "chkLogs_ResetDaily";
            this.chkLogs_ResetDaily.Size = new System.Drawing.Size(15, 15);
            this.chkLogs_ResetDaily.TabIndex = 11;
            this.chkLogs_ResetDaily.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            // 
            // chkLogs_ToMemory
            // 
            this.chkLogs_ToMemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogs_ToMemory.Location = new System.Drawing.Point(285, 21);
            this.chkLogs_ToMemory.Name = "chkLogs_ToMemory";
            this.chkLogs_ToMemory.Size = new System.Drawing.Size(15, 15);
            this.chkLogs_ToMemory.TabIndex = 11;
            this.chkLogs_ToMemory.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            this.chkLogs_ToMemory.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.chkLogs_ToMemory_ToggleStateChanged);
            // 
            // chkLogs_ToDisk
            // 
            this.chkLogs_ToDisk.Location = new System.Drawing.Point(285, 47);
            this.chkLogs_ToDisk.Name = "chkLogs_ToDisk";
            this.chkLogs_ToDisk.Size = new System.Drawing.Size(15, 15);
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
            // frmNewService
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 472);
            this.Controls.Add(this.radPageView1);
            this.Controls.Add(this.bnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNewService";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "New Service";
            this.Load += new System.EventHandler(this.frmNewService_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmNewService_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmNewService_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.bnSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnSelectPath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radPageView1)).EndInit();
            this.radPageView1.ResumeLayout(false);
            this.rpvBasic.ResumeLayout(false);
            this.rpvBasic.PerformLayout();
            this.rpbStart.ResumeLayout(false);
            this.rpbStart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnabled)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoRestart)).EndInit();
            this.rpvShutdown.ResumeLayout(false);
            this.rpvShutdown.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShutdown_SendENTER)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_ENTER_Timeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudShutdown_Timeout)).EndInit();
            this.radPageViewPage1.ResumeLayout(false);
            this.radPageViewPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ResetDaily)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ToMemory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLogs_ToDisk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private RadButton bnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.OpenFileDialog ofdOpen;
        private RadButton bnSelectPath;
        private Telerik.WinControls.UI.RadPageView radPageView1;
        private Telerik.WinControls.UI.RadPageViewPage rpvBasic;
        private Telerik.WinControls.UI.RadPageViewPage rpvShutdown;
        private RadPageViewPage rpbStart;
        private RadCheckBox chkAutoRestart;
        private System.Windows.Forms.Label label6;
        private RadCheckBox chkEnabled;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudShutdown_Timeout;
        private System.Windows.Forms.Label label8;
        private RadCheckBox chkShutdown_SendENTER;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudShutdown_ENTER_Timeout;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private RadPageViewPage radPageViewPage1;
        private RadCheckBox chkLogs_ToDisk;
        private System.Windows.Forms.Label label12;
        private RadCheckBox chkLogs_ResetDaily;
        private System.Windows.Forms.Label label13;
        private RadCheckBox chkLogs_ToMemory;
        private System.Windows.Forms.Label label14;
    }
}