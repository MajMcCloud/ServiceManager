namespace ServiceManager.Extensions.SyslogNotifier.Forms
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
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkTcp = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chk5424 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtHostname = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtApp = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFacility = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMax = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
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
            this.label1.Text = "Syslog host:";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(190, 25);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(250, 20);
            this.txtHost.TabIndex = 1;
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
            this.label3.Text = "Transport:";
            // 
            // chkTcp
            // 
            this.chkTcp.AutoSize = true;
            this.chkTcp.Location = new System.Drawing.Point(190, 82);
            this.chkTcp.Name = "chkTcp";
            this.chkTcp.Size = new System.Drawing.Size(110, 17);
            this.chkTcp.TabIndex = 5;
            this.chkTcp.Text = "Send over TCP";
            this.chkTcp.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 112);
            this.label4.Name = "label4";
            this.label4.TabIndex = 6;
            this.label4.Text = "Format:";
            // 
            // chk5424
            // 
            this.chk5424.AutoSize = true;
            this.chk5424.Location = new System.Drawing.Point(190, 110);
            this.chk5424.Name = "chk5424";
            this.chk5424.Size = new System.Drawing.Size(190, 17);
            this.chk5424.TabIndex = 7;
            this.chk5424.Text = "RFC 5424 instead of RFC 3164";
            this.chk5424.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 140);
            this.label5.Name = "label5";
            this.label5.TabIndex = 8;
            this.label5.Text = "Hostname:";
            // 
            // txtHostname
            // 
            this.txtHostname.Location = new System.Drawing.Point(190, 137);
            this.txtHostname.Name = "txtHostname";
            this.txtHostname.Size = new System.Drawing.Size(250, 20);
            this.txtHostname.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 168);
            this.label6.Name = "label6";
            this.label6.TabIndex = 10;
            this.label6.Text = "App name:";
            // 
            // txtApp
            // 
            this.txtApp.Location = new System.Drawing.Point(190, 165);
            this.txtApp.Name = "txtApp";
            this.txtApp.Size = new System.Drawing.Size(250, 20);
            this.txtApp.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 196);
            this.label7.Name = "label7";
            this.label7.TabIndex = 12;
            this.label7.Text = "Facility:";
            // 
            // txtFacility
            // 
            this.txtFacility.Location = new System.Drawing.Point(190, 193);
            this.txtFacility.Name = "txtFacility";
            this.txtFacility.Size = new System.Drawing.Size(60, 20);
            this.txtFacility.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 224);
            this.label8.Name = "label8";
            this.label8.TabIndex = 14;
            this.label8.Text = "Max bytes:";
            // 
            // txtMax
            // 
            this.txtMax.Location = new System.Drawing.Point(190, 221);
            this.txtMax.Name = "txtMax";
            this.txtMax.Size = new System.Drawing.Size(70, 20);
            this.txtMax.TabIndex = 15;
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
            this.label10.Text = "An empty hostname sends the name of the machine. Facility 16 is local0.";
            // 
            // bnTest
            // 
            this.bnTest.Location = new System.Drawing.Point(30, 305);
            this.bnTest.Name = "bnTest";
            this.bnTest.Size = new System.Drawing.Size(150, 26);
            this.bnTest.TabIndex = 19;
            this.bnTest.Text = "Send test line";
            this.bnTest.UseVisualStyleBackColor = true;
            this.bnTest.Click += new System.EventHandler(this.bnTest_Click);
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(350, 305);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(90, 26);
            this.bnSave.TabIndex = 20;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(450, 305);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 26);
            this.bnCancel.TabIndex = 21;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // FrmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 350);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkTcp);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chk5424);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtHostname);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtApp);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtFacility);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtMax);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.bnTest);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config Syslog";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkTcp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chk5424;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtHostname;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtApp;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFacility;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtMax;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button bnTest;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
