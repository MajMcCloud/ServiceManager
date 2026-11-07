namespace ServiceManager.Extensions.EventLogNotifier.forms
{
    partial class frmConfig
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
            this.chkWriteToEventLog = new System.Windows.Forms.CheckBox();
            this.chkShowBalloon = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtEventLog = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtEventId = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBalloonSeconds = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bnTest = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkWriteToEventLog
            // 
            this.chkWriteToEventLog.AutoSize = true;
            this.chkWriteToEventLog.Location = new System.Drawing.Point(30, 25);
            this.chkWriteToEventLog.Name = "chkWriteToEventLog";
            this.chkWriteToEventLog.Size = new System.Drawing.Size(150, 17);
            this.chkWriteToEventLog.TabIndex = 0;
            this.chkWriteToEventLog.Text = "Write to the event log";
            this.chkWriteToEventLog.UseVisualStyleBackColor = true;
            // 
            // chkShowBalloon
            // 
            this.chkShowBalloon.AutoSize = true;
            this.chkShowBalloon.Location = new System.Drawing.Point(30, 50);
            this.chkShowBalloon.Name = "chkShowBalloon";
            this.chkShowBalloon.Size = new System.Drawing.Size(150, 17);
            this.chkShowBalloon.TabIndex = 1;
            this.chkShowBalloon.Text = "Show a balloon tip";
            this.chkShowBalloon.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 90);
            this.label1.Name = "label1";
            this.label1.TabIndex = 2;
            this.label1.Text = "Log:";
            // 
            // txtEventLog
            // 
            this.txtEventLog.Location = new System.Drawing.Point(150, 87);
            this.txtEventLog.Name = "txtEventLog";
            this.txtEventLog.Size = new System.Drawing.Size(220, 20);
            this.txtEventLog.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 118);
            this.label2.Name = "label2";
            this.label2.TabIndex = 4;
            this.label2.Text = "Source:";
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(150, 115);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(220, 20);
            this.txtSource.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 146);
            this.label3.Name = "label3";
            this.label3.TabIndex = 6;
            this.label3.Text = "First event id:";
            // 
            // txtEventId
            // 
            this.txtEventId.Location = new System.Drawing.Point(150, 143);
            this.txtEventId.Name = "txtEventId";
            this.txtEventId.Size = new System.Drawing.Size(100, 20);
            this.txtEventId.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 174);
            this.label4.Name = "label4";
            this.label4.TabIndex = 8;
            this.label4.Text = "Balloon seconds:";
            // 
            // txtBalloonSeconds
            // 
            this.txtBalloonSeconds.Location = new System.Drawing.Point(150, 171);
            this.txtBalloonSeconds.Name = "txtBalloonSeconds";
            this.txtBalloonSeconds.Size = new System.Drawing.Size(60, 20);
            this.txtBalloonSeconds.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 205);
            this.label5.Name = "label5";
            this.label5.TabIndex = 10;
            this.label5.Text = "A new source has to be registered once, that needs administrator rights.";
            // 
            // bnTest
            // 
            this.bnTest.Location = new System.Drawing.Point(30, 240);
            this.bnTest.Name = "bnTest";
            this.bnTest.Size = new System.Drawing.Size(150, 26);
            this.bnTest.TabIndex = 11;
            this.bnTest.Text = "Send test entries";
            this.bnTest.UseVisualStyleBackColor = true;
            this.bnTest.Click += new System.EventHandler(this.bnTest_Click);
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(300, 240);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(90, 26);
            this.bnSave.TabIndex = 12;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(400, 240);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 26);
            this.bnCancel.TabIndex = 13;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // frmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 285);
            this.Controls.Add(this.chkWriteToEventLog);
            this.Controls.Add(this.chkShowBalloon);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtEventLog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtEventId);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtBalloonSeconds);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.bnTest);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config Windows Event Log";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkWriteToEventLog;
        private System.Windows.Forms.CheckBox chkShowBalloon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtEventLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEventId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBalloonSeconds;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button bnTest;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
