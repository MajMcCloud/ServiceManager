namespace ServiceManager.UI.Forms
{
    partial class FrmProgress
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
            this.lblMessage = new System.Windows.Forms.Label();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.lblPercent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblMessage.Location = new System.Drawing.Point(12, 9);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(380, 20);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "Preparing...";
            // 
            // pbProgress
            // 
            this.pbProgress.Location = new System.Drawing.Point(12, 34);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(340, 20);
            this.pbProgress.TabIndex = 1;
            // 
            // lblPercent
            // 
            this.lblPercent.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPercent.Location = new System.Drawing.Point(358, 34);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(34, 20);
            this.lblPercent.TabIndex = 2;
            this.lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FrmProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 66);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.pbProgress);
            this.Controls.Add(this.lblPercent);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmProgress";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmProgress_FormClosing);
            this.Load += new System.EventHandler(this.FrmProgress_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Label lblPercent;
    }
}
