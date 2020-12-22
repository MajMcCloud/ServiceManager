namespace ServiceManager.UI.forms
{
    partial class frmConnectTo
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
            this.txtHost = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.bnConnect = new Telerik.WinControls.UI.RadButton();
            this.txtPort = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel2 = new Telerik.WinControls.UI.RadLabel();
            ((System.ComponentModel.ISupportInitialize)(this.txtHost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnConnect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).BeginInit();
            this.SuspendLayout();
            // 
            // txtHost
            // 
            this.txtHost.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtHost.Location = new System.Drawing.Point(119, 33);
            this.txtHost.Name = "txtHost";
            this.txtHost.SelectionStart = 9;
            this.txtHost.Size = new System.Drawing.Size(222, 23);
            this.txtHost.TabIndex = 0;
            this.txtHost.Text = "localhost";
            // 
            // radLabel1
            // 
            this.radLabel1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.radLabel1.Location = new System.Drawing.Point(12, 34);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(101, 21);
            this.radLabel1.TabIndex = 1;
            this.radLabel1.Text = "IP or Hostname";
            this.radLabel1.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            // 
            // bnConnect
            // 
            this.bnConnect.Location = new System.Drawing.Point(119, 100);
            this.bnConnect.Name = "bnConnect";
            this.bnConnect.Size = new System.Drawing.Size(110, 24);
            this.bnConnect.TabIndex = 2;
            this.bnConnect.Text = "Connect";
            this.bnConnect.Click += new System.EventHandler(this.bnConnect_Click);
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPort.Location = new System.Drawing.Point(119, 62);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(74, 23);
            this.txtPort.TabIndex = 0;
            this.txtPort.Text = "50000";
            // 
            // radLabel2
            // 
            this.radLabel2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.radLabel2.Location = new System.Drawing.Point(81, 63);
            this.radLabel2.Name = "radLabel2";
            this.radLabel2.Size = new System.Drawing.Size(32, 21);
            this.radLabel2.TabIndex = 1;
            this.radLabel2.Text = "Port";
            this.radLabel2.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
            // 
            // frmConnectTo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 136);
            this.Controls.Add(this.bnConnect);
            this.Controls.Add(this.radLabel2);
            this.Controls.Add(this.radLabel1);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConnectTo";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to";
            ((System.ComponentModel.ISupportInitialize)(this.txtHost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bnConnect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.UI.RadTextBox txtHost;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadButton bnConnect;
        private Telerik.WinControls.UI.RadTextBox txtPort;
        private Telerik.WinControls.UI.RadLabel radLabel2;
    }
}