namespace ServiceManager.UI
{
    partial class frmManage
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
            this.lsvServices = new System.Windows.Forms.ListView();
            this.cmID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmEnabled = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiMenu_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.rpv = new Telerik.WinControls.UI.RadPageView();
            this.radPageViewPage2 = new Telerik.WinControls.UI.RadPageViewPage();
            this.bnAddService = new Telerik.WinControls.UI.RadButton();
            this.radPageViewPage1 = new Telerik.WinControls.UI.RadPageViewPage();
            this.bnSettings_Save = new Telerik.WinControls.UI.RadButton();
            this.nud_TCPPort = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new Telerik.WinControls.UI.RadLabel();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.rrbMode_TCP = new Telerik.WinControls.UI.RadRadioButton();
            this.rrbMode_NetPipes = new Telerik.WinControls.UI.RadRadioButton();
            this.cmsMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpv)).BeginInit();
            this.rpv.SuspendLayout();
            this.radPageViewPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnAddService)).BeginInit();
            this.radPageViewPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnSettings_Save)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_TCP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_NetPipes)).BeginInit();
            this.SuspendLayout();
            // 
            // lsvServices
            // 
            this.lsvServices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvServices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cmID,
            this.cmEnabled,
            this.cmDescription,
            this.cmTitle,
            this.cmPath});
            this.lsvServices.FullRowSelect = true;
            this.lsvServices.GridLines = true;
            this.lsvServices.HideSelection = false;
            this.lsvServices.Location = new System.Drawing.Point(3, 37);
            this.lsvServices.Name = "lsvServices";
            this.lsvServices.Size = new System.Drawing.Size(739, 339);
            this.lsvServices.TabIndex = 3;
            this.lsvServices.UseCompatibleStateImageBehavior = false;
            this.lsvServices.View = System.Windows.Forms.View.Details;
            this.lsvServices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseClick);
            this.lsvServices.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lsvServices_MouseDoubleClick);
            // 
            // cmID
            // 
            this.cmID.Text = "ID";
            this.cmID.Width = 120;
            // 
            // cmEnabled
            // 
            this.cmEnabled.Text = "Enabled";
            // 
            // cmDescription
            // 
            this.cmDescription.Text = "Description";
            this.cmDescription.Width = 150;
            // 
            // cmTitle
            // 
            this.cmTitle.Text = "Title";
            this.cmTitle.Width = 120;
            // 
            // cmPath
            // 
            this.cmPath.Text = "Path";
            this.cmPath.Width = 240;
            // 
            // cmsMenu
            // 
            this.cmsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiMenu_Delete});
            this.cmsMenu.Name = "cmsMenu";
            this.cmsMenu.Size = new System.Drawing.Size(147, 26);
            // 
            // tsmiMenu_Delete
            // 
            this.tsmiMenu_Delete.Name = "tsmiMenu_Delete";
            this.tsmiMenu_Delete.Size = new System.Drawing.Size(146, 22);
            this.tsmiMenu_Delete.Text = "Delete service";
            this.tsmiMenu_Delete.Click += new System.EventHandler(this.tsmiMenu_Delete_Click);
            // 
            // rpv
            // 
            this.rpv.Controls.Add(this.radPageViewPage2);
            this.rpv.Controls.Add(this.radPageViewPage1);
            this.rpv.Location = new System.Drawing.Point(12, 12);
            this.rpv.Name = "rpv";
            this.rpv.SelectedPage = this.radPageViewPage2;
            this.rpv.Size = new System.Drawing.Size(766, 427);
            this.rpv.TabIndex = 4;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.rpv.GetChildAt(0))).StripButtons = Telerik.WinControls.UI.StripViewButtons.None;
            ((Telerik.WinControls.UI.RadPageViewStripElement)(this.rpv.GetChildAt(0))).ShowItemCloseButton = false;
            // 
            // radPageViewPage2
            // 
            this.radPageViewPage2.Controls.Add(this.bnAddService);
            this.radPageViewPage2.Controls.Add(this.lsvServices);
            this.radPageViewPage2.ItemSize = new System.Drawing.SizeF(56F, 28F);
            this.radPageViewPage2.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage2.Name = "radPageViewPage2";
            this.radPageViewPage2.Size = new System.Drawing.Size(745, 379);
            this.radPageViewPage2.Text = "Services";
            // 
            // bnAddService
            // 
            this.bnAddService.Location = new System.Drawing.Point(3, 3);
            this.bnAddService.Name = "bnAddService";
            this.bnAddService.Size = new System.Drawing.Size(110, 28);
            this.bnAddService.TabIndex = 4;
            this.bnAddService.Text = "Add service";
            this.bnAddService.Click += new System.EventHandler(this.bnAddServer_Click);
            // 
            // radPageViewPage1
            // 
            this.radPageViewPage1.Controls.Add(this.bnSettings_Save);
            this.radPageViewPage1.Controls.Add(this.nud_TCPPort);
            this.radPageViewPage1.Controls.Add(this.lblPort);
            this.radPageViewPage1.Controls.Add(this.radLabel1);
            this.radPageViewPage1.Controls.Add(this.rrbMode_TCP);
            this.radPageViewPage1.Controls.Add(this.rrbMode_NetPipes);
            this.radPageViewPage1.ItemSize = new System.Drawing.SizeF(56F, 28F);
            this.radPageViewPage1.Location = new System.Drawing.Point(10, 37);
            this.radPageViewPage1.Name = "radPageViewPage1";
            this.radPageViewPage1.Size = new System.Drawing.Size(745, 379);
            this.radPageViewPage1.Text = "Settings";
            // 
            // bnSettings_Save
            // 
            this.bnSettings_Save.Location = new System.Drawing.Point(30, 325);
            this.bnSettings_Save.Name = "bnSettings_Save";
            this.bnSettings_Save.Size = new System.Drawing.Size(110, 24);
            this.bnSettings_Save.TabIndex = 3;
            this.bnSettings_Save.Text = "Save";
            this.bnSettings_Save.Click += new System.EventHandler(this.bnSettings_Save_Click);
            // 
            // nud_TCPPort
            // 
            this.nud_TCPPort.Enabled = false;
            this.nud_TCPPort.Location = new System.Drawing.Point(118, 97);
            this.nud_TCPPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nud_TCPPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_TCPPort.Name = "nud_TCPPort";
            this.nud_TCPPort.Size = new System.Drawing.Size(75, 20);
            this.nud_TCPPort.TabIndex = 2;
            this.nud_TCPPort.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // lblPort
            // 
            this.lblPort.Enabled = false;
            this.lblPort.Location = new System.Drawing.Point(71, 98);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 18);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port:";
            // 
            // radLabel1
            // 
            this.radLabel1.Location = new System.Drawing.Point(30, 26);
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.Size = new System.Drawing.Size(58, 18);
            this.radLabel1.TabIndex = 1;
            this.radLabel1.Text = "Run Mode";
            // 
            // rrbMode_TCP
            // 
            this.rrbMode_TCP.Location = new System.Drawing.Point(48, 74);
            this.rrbMode_TCP.Name = "rrbMode_TCP";
            this.rrbMode_TCP.Size = new System.Drawing.Size(39, 18);
            this.rrbMode_TCP.TabIndex = 0;
            this.rrbMode_TCP.Text = "TCP";
            this.rrbMode_TCP.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.rrbMode_TCP_ToggleStateChanged);
            // 
            // rrbMode_NetPipes
            // 
            this.rrbMode_NetPipes.Location = new System.Drawing.Point(48, 50);
            this.rrbMode_NetPipes.Name = "rrbMode_NetPipes";
            this.rrbMode_NetPipes.Size = new System.Drawing.Size(68, 18);
            this.rrbMode_NetPipes.TabIndex = 0;
            this.rrbMode_NetPipes.Text = "Net Pipes";
            this.rrbMode_NetPipes.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.rrbMode_NetPipes_ToggleStateChanged);
            // 
            // frmManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 451);
            this.Controls.Add(this.rpv);
            this.Name = "frmManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage";
            this.Load += new System.EventHandler(this.frmManage_Load);
            this.cmsMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rpv)).EndInit();
            this.rpv.ResumeLayout(false);
            this.radPageViewPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bnAddService)).EndInit();
            this.radPageViewPage1.ResumeLayout(false);
            this.radPageViewPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bnSettings_Save)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_TCPPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_TCP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rrbMode_NetPipes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lsvServices;
        private System.Windows.Forms.ColumnHeader cmID;
        private System.Windows.Forms.ColumnHeader cmEnabled;
        private System.Windows.Forms.ColumnHeader cmDescription;
        private System.Windows.Forms.ColumnHeader cmTitle;
        private System.Windows.Forms.ColumnHeader cmPath;
        private System.Windows.Forms.ContextMenuStrip cmsMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiMenu_Delete;
        private Telerik.WinControls.UI.RadPageView rpv;
        private Telerik.WinControls.UI.RadPageViewPage radPageViewPage1;
        private Telerik.WinControls.UI.RadPageViewPage radPageViewPage2;
        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadRadioButton rrbMode_NetPipes;
        private Telerik.WinControls.UI.RadRadioButton rrbMode_TCP;
        private System.Windows.Forms.NumericUpDown nud_TCPPort;
        private Telerik.WinControls.UI.RadLabel lblPort;
        private Telerik.WinControls.UI.RadButton bnAddService;
        private Telerik.WinControls.UI.RadButton bnSettings_Save;
    }
}