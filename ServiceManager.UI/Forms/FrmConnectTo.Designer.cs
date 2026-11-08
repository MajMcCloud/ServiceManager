namespace ServiceManager.UI.Forms
{
    partial class FrmConnectTo
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
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.bnConnect = new System.Windows.Forms.Button();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtHost
            // 
            this.txtHost.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtHost.Location = new System.Drawing.Point(119, 33);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(222, 26);
            this.txtHost.TabIndex = 0;
            this.txtHost.Text = "localhost";
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHost.Location = new System.Drawing.Point(12, 36);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(101, 23);
            this.lblHost.TabIndex = 1;
            this.lblHost.Text = "IP or Hostname";
            // 
            // bnConnect
            // 
            this.bnConnect.Location = new System.Drawing.Point(119, 100);
            this.bnConnect.Name = "bnConnect";
            this.bnConnect.Size = new System.Drawing.Size(110, 26);
            this.bnConnect.TabIndex = 2;
            this.bnConnect.Text = "Connect";
            this.bnConnect.UseVisualStyleBackColor = true;
            this.bnConnect.Click += new System.EventHandler(this.bnConnect_Click);
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPort.Location = new System.Drawing.Point(119, 62);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(74, 26);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "50000";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPort.Location = new System.Drawing.Point(81, 65);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(32, 23);
            this.lblPort.TabIndex = 4;
            this.lblPort.Text = "Port";
            // 
            // FrmConnectTo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 136);
            this.Controls.Add(this.bnConnect);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lblHost);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConnectTo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.Button bnConnect;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
    }
}