namespace ServiceManager.UI.Forms
{
    partial class FrmUserAccountPicker
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
            this.lblFilter = new System.Windows.Forms.Label();
            this.txtFilter = new System.Windows.Forms.TextBox();
            this.lsvAccounts = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colFull = new System.Windows.Forms.ColumnHeader();
            this.colDescription = new System.Windows.Forms.ColumnHeader();
            this.colState = new System.Windows.Forms.ColumnHeader();
            this.lblCount = new System.Windows.Forms.Label();
            this.bnOK = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFilter
            // 
            this.lblFilter.Location = new System.Drawing.Point(12, 18);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(42, 15);
            this.lblFilter.TabIndex = 0;
            this.lblFilter.Text = "Filter:";
            // 
            // txtFilter
            // 
            this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilter.Location = new System.Drawing.Point(60, 15);
            this.txtFilter.Name = "txtFilter";
            this.txtFilter.Size = new System.Drawing.Size(628, 20);
            this.txtFilter.TabIndex = 1;
            this.txtFilter.TextChanged += new System.EventHandler(this.txtFilter_TextChanged);
            // 
            // lsvAccounts
            // 
            this.lsvAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvAccounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colFull,
            this.colDescription,
            this.colState});
            this.lsvAccounts.FullRowSelect = true;
            this.lsvAccounts.HideSelection = false;
            this.lsvAccounts.Location = new System.Drawing.Point(12, 45);
            this.lsvAccounts.MultiSelect = false;
            this.lsvAccounts.Name = "lsvAccounts";
            this.lsvAccounts.Size = new System.Drawing.Size(676, 359);
            this.lsvAccounts.TabIndex = 2;
            this.lsvAccounts.UseCompatibleStateImageBehavior = false;
            this.lsvAccounts.View = System.Windows.Forms.View.Details;
            this.lsvAccounts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lsvAccounts_MouseDoubleClick);
            // 
            // colName
            // 
            this.colName.Text = "Account";
            this.colName.Width = 150;
            // 
            // colFull
            // 
            this.colFull.Text = "Full name";
            this.colFull.Width = 160;
            // 
            // colDescription
            // 
            this.colDescription.Text = "Description";
            this.colDescription.Width = 260;
            // 
            // colState
            // 
            this.colState.Text = "State";
            this.colState.Width = 80;
            // 
            // lblCount
            // 
            this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCount.Location = new System.Drawing.Point(12, 413);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(200, 15);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "0 of 0 accounts";
            // 
            // bnOK
            // 
            this.bnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnOK.Location = new System.Drawing.Point(502, 408);
            this.bnOK.Name = "bnOK";
            this.bnOK.Size = new System.Drawing.Size(90, 25);
            this.bnOK.TabIndex = 4;
            this.bnOK.Text = "OK";
            this.bnOK.UseVisualStyleBackColor = true;
            this.bnOK.Click += new System.EventHandler(this.bnOK_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(598, 408);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 25);
            this.bnCancel.TabIndex = 5;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            // 
            // FrmUserAccountPicker
            // 
            this.AcceptButton = this.bnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(700, 458);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.lsvAccounts);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.bnOK);
            this.Controls.Add(this.bnCancel);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(420, 260);
            this.Name = "FrmUserAccountPicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Run under account";
            this.Load += new System.EventHandler(this.FrmUserAccountPicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.ListView lsvAccounts;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colFull;
        private System.Windows.Forms.ColumnHeader colDescription;
        private System.Windows.Forms.ColumnHeader colState;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Button bnOK;
        private System.Windows.Forms.Button bnCancel;
    }
}
