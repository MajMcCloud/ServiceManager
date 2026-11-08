namespace ServiceManager.Extensions.SmtpNotifier.Forms
{
    partial class FrmConfig
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkStartTls = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtThrottle = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPickup = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.bnTest = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 28);
            this.label1.Name = "label1";
            this.label1.TabIndex = 0;
            this.label1.Text = "SMTP server:";
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(190, 25);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(250, 20);
            this.txtServer.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 56);
            this.label2.Name = "label2";
            this.label2.TabIndex = 2;
            this.label2.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(190, 53);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(70, 20);
            this.txtPort.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 84);
            this.label3.Name = "label3";
            this.label3.TabIndex = 4;
            this.label3.Text = "Encryption:";
            // 
            // chkStartTls
            // 
            this.chkStartTls.AutoSize = true;
            this.chkStartTls.Location = new System.Drawing.Point(190, 82);
            this.chkStartTls.Name = "chkStartTls";
            this.chkStartTls.Size = new System.Drawing.Size(90, 17);
            this.chkStartTls.TabIndex = 5;
            this.chkStartTls.Text = "Use STARTTLS";
            this.chkStartTls.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 112);
            this.label4.Name = "label4";
            this.label4.TabIndex = 6;
            this.label4.Text = "Username:";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(190, 109);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(250, 20);
            this.txtUser.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 140);
            this.label5.Name = "label5";
            this.label5.TabIndex = 8;
            this.label5.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(190, 137);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(250, 20);
            this.txtPassword.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 168);
            this.label6.Name = "label6";
            this.label6.TabIndex = 10;
            this.label6.Text = "Sender:";
            // 
            // txtFrom
            // 
            this.txtFrom.Location = new System.Drawing.Point(190, 165);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(250, 20);
            this.txtFrom.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 196);
            this.label7.Name = "label7";
            this.label7.TabIndex = 12;
            this.label7.Text = "Recipient(s):";
            // 
            // txtTo
            // 
            this.txtTo.Location = new System.Drawing.Point(190, 193);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(330, 20);
            this.txtTo.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 224);
            this.label8.Name = "label8";
            this.label8.TabIndex = 14;
            this.label8.Text = "Subject prefix:";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(190, 221);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(250, 20);
            this.txtPrefix.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(30, 252);
            this.label9.Name = "label9";
            this.label9.TabIndex = 16;
            this.label9.Text = "Timeout seconds:";
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new System.Drawing.Point(190, 249);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(60, 20);
            this.txtTimeout.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(30, 280);
            this.label10.Name = "label10";
            this.label10.TabIndex = 18;
            this.label10.Text = "Seconds between identical mails:";
            // 
            // txtThrottle
            // 
            this.txtThrottle.Location = new System.Drawing.Point(190, 277);
            this.txtThrottle.Name = "txtThrottle";
            this.txtThrottle.Size = new System.Drawing.Size(60, 20);
            this.txtThrottle.TabIndex = 19;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(30, 308);
            this.label11.Name = "label11";
            this.label11.TabIndex = 20;
            this.label11.Text = "Pickup directory:";
            // 
            // txtPickup
            // 
            this.txtPickup.Location = new System.Drawing.Point(190, 305);
            this.txtPickup.Name = "txtPickup";
            this.txtPickup.Size = new System.Drawing.Size(330, 20);
            this.txtPickup.TabIndex = 21;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(30, 336);
            this.label12.Name = "label12";
            this.label12.TabIndex = 22;
            this.label12.Text = "Several recipients separated by comma. A pickup directory writes the mail as a file instead of sending it.";
            // 
            // bnTest
            // 
            this.bnTest.Location = new System.Drawing.Point(30, 365);
            this.bnTest.Name = "bnTest";
            this.bnTest.Size = new System.Drawing.Size(150, 26);
            this.bnTest.TabIndex = 23;
            this.bnTest.Text = "Send test mail";
            this.bnTest.UseVisualStyleBackColor = true;
            this.bnTest.Click += new System.EventHandler(this.bnTest_Click);
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(350, 365);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(90, 26);
            this.bnSave.TabIndex = 24;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(450, 365);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 26);
            this.bnCancel.TabIndex = 25;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // FrmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 410);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkStartTls);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtPrefix);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtThrottle);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtPickup);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.bnTest);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config E-Mail (SMTP)";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkStartTls;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtTo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtThrottle;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPickup;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button bnTest;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
