namespace ServiceManager.UI.Forms
{
    partial class FrmServiceLog
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
            this.tsmiLog = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLog_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLog_OpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLog_Separator = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLog_AutoRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.cboFiles = new System.Windows.Forms.ToolStripComboBox();
            this.tsmiInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtContent
            // 
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
            this.txtContent.Size = new System.Drawing.Size(900, 476);
            this.txtContent.TabIndex = 0;
            this.txtContent.Text = "";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiLog,
            this.cboFiles,
            this.tsmiInfo});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(900, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiLog
            // 
            this.tsmiLog.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiLog_Refresh,
            this.tsmiLog_OpenFolder,
            this.tsmiLog_Separator,
            this.tsmiLog_AutoRefresh});
            this.tsmiLog.ForeColor = System.Drawing.Color.White;
            this.tsmiLog.Name = "tsmiLog";
            this.tsmiLog.Size = new System.Drawing.Size(44, 20);
            this.tsmiLog.Text = "Log";
            // 
            // tsmiLog_Refresh
            // 
            this.tsmiLog_Refresh.ForeColor = System.Drawing.Color.Black;
            this.tsmiLog_Refresh.Name = "tsmiLog_Refresh";
            this.tsmiLog_Refresh.Size = new System.Drawing.Size(150, 22);
            this.tsmiLog_Refresh.Text = "Reload";
            this.tsmiLog_Refresh.Click += new System.EventHandler(this.tsmiLog_Refresh_Click);
            // 
            // tsmiLog_OpenFolder
            // 
            this.tsmiLog_OpenFolder.ForeColor = System.Drawing.Color.Black;
            this.tsmiLog_OpenFolder.Name = "tsmiLog_OpenFolder";
            this.tsmiLog_OpenFolder.Size = new System.Drawing.Size(150, 22);
            this.tsmiLog_OpenFolder.Text = "Open folder";
            this.tsmiLog_OpenFolder.Click += new System.EventHandler(this.tsmiLog_OpenFolder_Click);
            // 
            // tsmiLog_Separator
            // 
            this.tsmiLog_Separator.Name = "tsmiLog_Separator";
            this.tsmiLog_Separator.Size = new System.Drawing.Size(147, 6);
            // 
            // tsmiLog_AutoRefresh
            // 
            this.tsmiLog_AutoRefresh.CheckOnClick = true;
            this.tsmiLog_AutoRefresh.ForeColor = System.Drawing.Color.Black;
            this.tsmiLog_AutoRefresh.Name = "tsmiLog_AutoRefresh";
            this.tsmiLog_AutoRefresh.Size = new System.Drawing.Size(150, 22);
            this.tsmiLog_AutoRefresh.Text = "Auto refresh";
            this.tsmiLog_AutoRefresh.CheckedChanged += new System.EventHandler(this.tsmiLog_AutoRefresh_CheckedChanged);
            // 
            // cboFiles
            // 
            this.cboFiles.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.cboFiles.AutoSize = false;
            this.cboFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.cboFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFiles.ForeColor = System.Drawing.Color.White;
            this.cboFiles.Name = "cboFiles";
            this.cboFiles.Size = new System.Drawing.Size(240, 22);
            this.cboFiles.SelectedIndexChanged += new System.EventHandler(this.cboFiles_SelectedIndexChanged);
            // 
            // tsmiInfo
            // 
            this.tsmiInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsmiInfo.Enabled = false;
            this.tsmiInfo.ForeColor = System.Drawing.Color.White;
            this.tsmiInfo.Name = "tsmiInfo";
            this.tsmiInfo.Size = new System.Drawing.Size(85, 20);
            this.tsmiInfo.Text = "no log file";
            // 
            // FrmServiceLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FrmServiceLog";
            this.Text = "Service log";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmServiceLog_FormClosing);
            this.Load += new System.EventHandler(this.FrmServiceLog_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiLog;
        private System.Windows.Forms.ToolStripMenuItem tsmiLog_Refresh;
        private System.Windows.Forms.ToolStripMenuItem tsmiLog_OpenFolder;
        private System.Windows.Forms.ToolStripSeparator tsmiLog_Separator;
        private System.Windows.Forms.ToolStripMenuItem tsmiLog_AutoRefresh;
        private System.Windows.Forms.ToolStripComboBox cboFiles;
        private System.Windows.Forms.ToolStripMenuItem tsmiInfo;
        private System.Windows.Forms.RichTextBox txtContent;
    }
}
