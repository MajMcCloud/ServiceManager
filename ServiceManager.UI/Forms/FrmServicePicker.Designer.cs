namespace ServiceManager.UI.Forms
{
    partial class FrmServicePicker
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
            this.lsvServices = new System.Windows.Forms.ListView();
            this.colDisplay = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colStart = new System.Windows.Forms.ColumnHeader();
            this.lblCount = new System.Windows.Forms.Label();
            this.bnSelectAll = new System.Windows.Forms.Button();
            this.bnClear = new System.Windows.Forms.Button();
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
            // lsvServices
            // 
            this.lsvServices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvServices.CheckBoxes = true;
            this.lsvServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDisplay,
            this.colName,
            this.colStatus,
            this.colStart});
            this.lsvServices.FullRowSelect = true;
            this.lsvServices.HideSelection = false;
            this.lsvServices.Location = new System.Drawing.Point(12, 45);
            this.lsvServices.MultiSelect = false;
            this.lsvServices.Name = "lsvServices";
            this.lsvServices.Size = new System.Drawing.Size(676, 337);
            this.lsvServices.TabIndex = 2;
            this.lsvServices.UseCompatibleStateImageBehavior = false;
            this.lsvServices.View = System.Windows.Forms.View.Details;
            this.lsvServices.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lsvServices_ItemChecked);
            // 
            // colDisplay
            // 
            this.colDisplay.Text = "Display name";
            this.colDisplay.Width = 250;
            // 
            // colName
            // 
            this.colName.Text = "Service name";
            this.colName.Width = 170;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 90;
            // 
            // colStart
            // 
            this.colStart.Text = "Start type";
            this.colStart.Width = 90;
            // 
            // lblCount
            // 
            this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblCount.Location = new System.Drawing.Point(12, 391);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(200, 15);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "0 of 0 selected";
            // 
            // bnSelectAll
            // 
            this.bnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnSelectAll.Location = new System.Drawing.Point(502, 386);
            this.bnSelectAll.Name = "bnSelectAll";
            this.bnSelectAll.Size = new System.Drawing.Size(90, 25);
            this.bnSelectAll.TabIndex = 4;
            this.bnSelectAll.Text = "Select all";
            this.bnSelectAll.UseVisualStyleBackColor = true;
            this.bnSelectAll.Click += new System.EventHandler(this.bnSelectAll_Click);
            // 
            // bnClear
            // 
            this.bnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnClear.Location = new System.Drawing.Point(598, 386);
            this.bnClear.Name = "bnClear";
            this.bnClear.Size = new System.Drawing.Size(90, 25);
            this.bnClear.TabIndex = 5;
            this.bnClear.Text = "Clear";
            this.bnClear.UseVisualStyleBackColor = true;
            this.bnClear.Click += new System.EventHandler(this.bnClear_Click);
            // 
            // bnOK
            // 
            this.bnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bnOK.Location = new System.Drawing.Point(502, 421);
            this.bnOK.Name = "bnOK";
            this.bnOK.Size = new System.Drawing.Size(90, 25);
            this.bnOK.TabIndex = 6;
            this.bnOK.Text = "OK";
            this.bnOK.UseVisualStyleBackColor = true;
            this.bnOK.Click += new System.EventHandler(this.bnOK_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(598, 421);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 25);
            this.bnCancel.TabIndex = 7;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            // 
            // FrmServicePicker
            // 
            this.AcceptButton = this.bnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(700, 458);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.txtFilter);
            this.Controls.Add(this.lsvServices);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.bnSelectAll);
            this.Controls.Add(this.bnClear);
            this.Controls.Add(this.bnOK);
            this.Controls.Add(this.bnCancel);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(420, 260);
            this.Name = "FrmServicePicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Windows services";
            this.Load += new System.EventHandler(this.FrmServicePicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.ListView lsvServices;
        private System.Windows.Forms.ColumnHeader colDisplay;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Button bnSelectAll;
        private System.Windows.Forms.Button bnClear;
        private System.Windows.Forms.Button bnOK;
        private System.Windows.Forms.Button bnCancel;
    }
}
