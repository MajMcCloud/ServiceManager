namespace ServiceManager.Extensions.TelegramNotifier.Forms
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtAPIKey = new System.Windows.Forms.TextBox();
            this.txtChatId = new System.Windows.Forms.TextBox();
            this.bnCancel = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnSearch = new System.Windows.Forms.Button();
            this.bnTestMessage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "API Key:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 166);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "Chat Id:";
            // 
            // txtAPIKey
            // 
            this.txtAPIKey.Location = new System.Drawing.Point(224, 59);
            this.txtAPIKey.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtAPIKey.Name = "txtAPIKey";
            this.txtAPIKey.Size = new System.Drawing.Size(464, 29);
            this.txtAPIKey.TabIndex = 2;
            // 
            // txtChatId
            // 
            this.txtChatId.Location = new System.Drawing.Point(224, 161);
            this.txtChatId.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.txtChatId.Name = "txtChatId";
            this.txtChatId.Size = new System.Drawing.Size(464, 29);
            this.txtChatId.TabIndex = 3;
            // 
            // bnCancel
            // 
            this.bnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnCancel.Location = new System.Drawing.Point(554, 403);
            this.bnCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(138, 42);
            this.bnCancel.TabIndex = 4;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // bnSave
            // 
            this.bnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnSave.Location = new System.Drawing.Point(405, 403);
            this.bnSave.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(138, 42);
            this.bnSave.TabIndex = 5;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnSearch
            // 
            this.bnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnSearch.Location = new System.Drawing.Point(224, 212);
            this.bnSearch.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.bnSearch.Name = "bnSearch";
            this.bnSearch.Size = new System.Drawing.Size(238, 42);
            this.bnSearch.TabIndex = 5;
            this.bnSearch.Text = "Start searching";
            this.bnSearch.UseVisualStyleBackColor = true;
            this.bnSearch.Click += new System.EventHandler(this.bnSearch_Click);
            // 
            // bnTestMessage
            // 
            this.bnTestMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnTestMessage.Location = new System.Drawing.Point(224, 290);
            this.bnTestMessage.Margin = new System.Windows.Forms.Padding(6);
            this.bnTestMessage.Name = "bnTestMessage";
            this.bnTestMessage.Size = new System.Drawing.Size(238, 42);
            this.bnTestMessage.TabIndex = 5;
            this.bnTestMessage.Text = "Send test message";
            this.bnTestMessage.UseVisualStyleBackColor = true;
            this.bnTestMessage.Click += new System.EventHandler(this.bnTestMessage_Click);
            // 
            // FrmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 477);
            this.Controls.Add(this.bnSearch);
            this.Controls.Add(this.bnTestMessage);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.txtChatId);
            this.Controls.Add(this.txtAPIKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConfig";
            this.Text = "Config TelegramNotifier";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmConfig_FormClosing);
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAPIKey;
        private System.Windows.Forms.TextBox txtChatId;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnSearch;
        private System.Windows.Forms.Button bnTestMessage;
    }
}