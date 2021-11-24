namespace ServiceManager.UI.forms
{
    partial class frmLogs
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
            this.txtContent = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiStatus = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiService_Restart = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiService_Shutdown = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLogs_Live = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLogs_Autoscroll = new System.Windows.Forms.ToolStripMenuItem();
            this.txtSearch = new System.Windows.Forms.ToolStripTextBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // txtContent
            // 
            this.txtContent.AllowDrop = true;
            this.txtContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContent.BackColor = System.Drawing.Color.Black;
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtContent.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.txtContent.ForeColor = System.Drawing.Color.Silver;
            this.txtContent.Location = new System.Drawing.Point(0, 24);
            this.txtContent.Name = "txtContent";
            this.txtContent.ReadOnly = true;
            this.txtContent.Size = new System.Drawing.Size(800, 400);
            this.txtContent.TabIndex = 0;
            this.txtContent.Text = "";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiStatus,
            this.tsmiLogs,
            this.txtSearch});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiStatus
            // 
            this.tsmiStatus.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiService_Restart,
            this.tsmiService_Shutdown});
            this.tsmiStatus.ForeColor = System.Drawing.Color.White;
            this.tsmiStatus.Image = global::ServiceManager.UI.Properties.Resources.led_off;
            this.tsmiStatus.Name = "tsmiStatus";
            this.tsmiStatus.Size = new System.Drawing.Size(128, 20);
            this.tsmiStatus.Text = "Service is running";
            // 
            // tsmiService_Restart
            // 
            this.tsmiService_Restart.BackColor = System.Drawing.Color.White;
            this.tsmiService_Restart.ForeColor = System.Drawing.Color.Black;
            this.tsmiService_Restart.Image = global::ServiceManager.UI.Properties.Resources.play;
            this.tsmiService_Restart.Name = "tsmiService_Restart";
            this.tsmiService_Restart.Size = new System.Drawing.Size(128, 22);
            this.tsmiService_Restart.Text = "Restart";
            this.tsmiService_Restart.Click += new System.EventHandler(this.tsmiService_Restart_Click);
            // 
            // tsmiService_Shutdown
            // 
            this.tsmiService_Shutdown.BackColor = System.Drawing.SystemColors.Control;
            this.tsmiService_Shutdown.ForeColor = System.Drawing.Color.Black;
            this.tsmiService_Shutdown.Image = global::ServiceManager.UI.Properties.Resources.stop;
            this.tsmiService_Shutdown.Name = "tsmiService_Shutdown";
            this.tsmiService_Shutdown.Size = new System.Drawing.Size(128, 22);
            this.tsmiService_Shutdown.Text = "Shutdown";
            this.tsmiService_Shutdown.Click += new System.EventHandler(this.tsmiService_Shutdown_Click);
            // 
            // tsmiLogs
            // 
            this.tsmiLogs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiLogs_Live,
            this.tsmiLogs_Autoscroll});
            this.tsmiLogs.ForeColor = System.Drawing.Color.White;
            this.tsmiLogs.Name = "tsmiLogs";
            this.tsmiLogs.Size = new System.Drawing.Size(44, 20);
            this.tsmiLogs.Text = "Logs";
            // 
            // tsmiLogs_Live
            // 
            this.tsmiLogs_Live.CheckOnClick = true;
            this.tsmiLogs_Live.Name = "tsmiLogs_Live";
            this.tsmiLogs_Live.Size = new System.Drawing.Size(128, 22);
            this.tsmiLogs_Live.Text = "Live";
            this.tsmiLogs_Live.CheckedChanged += new System.EventHandler(this.tsmiLogs_Live_CheckedChanged);
            // 
            // tsmiLogs_Autoscroll
            // 
            this.tsmiLogs_Autoscroll.BackColor = System.Drawing.Color.White;
            this.tsmiLogs_Autoscroll.Checked = true;
            this.tsmiLogs_Autoscroll.CheckOnClick = true;
            this.tsmiLogs_Autoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiLogs_Autoscroll.ForeColor = System.Drawing.Color.Black;
            this.tsmiLogs_Autoscroll.Name = "tsmiLogs_Autoscroll";
            this.tsmiLogs_Autoscroll.Size = new System.Drawing.Size(128, 22);
            this.tsmiLogs_Autoscroll.Text = "Autoscroll";
            this.tsmiLogs_Autoscroll.Click += new System.EventHandler(this.tsmiLogs_Autoscroll_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.ForeColor = System.Drawing.Color.White;
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(150, 20);
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.BackColor = System.Drawing.Color.Black;
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInput.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.txtInput.ForeColor = System.Drawing.Color.White;
            this.txtInput.Location = new System.Drawing.Point(42, 428);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(758, 19);
            this.txtInput.TabIndex = 2;
            this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(0, 424);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 27);
            this.label1.TabIndex = 3;
            this.label1.Text = ">>";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmLogs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.label1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmLogs";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.Text = "Service logs";
            this.Activated += new System.EventHandler(this.frmLogs_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLogs_FormClosing);
            this.Load += new System.EventHandler(this.frmLogs_Load);
            this.Shown += new System.EventHandler(this.frmLogs_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiLogs;
        private System.Windows.Forms.ToolStripMenuItem tsmiLogs_Autoscroll;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.ToolStripMenuItem tsmiStatus;
        private System.Windows.Forms.ToolStripMenuItem tsmiService_Shutdown;
        private System.Windows.Forms.ToolStripMenuItem tsmiService_Restart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripTextBox txtSearch;
        private System.Windows.Forms.ToolStripMenuItem tsmiLogs_Live;
        private System.Windows.Forms.RichTextBox txtContent;
    }
}