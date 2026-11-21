namespace ServiceManager.UI.Forms
{
    partial class FrmEndpointUser
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
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblName.Location = new System.Drawing.Point(12, 36);
            this.lblName.Name = "lblName";
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Windows account";
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtName.Location = new System.Drawing.Point(140, 33);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(232, 26);
            this.txtName.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblPassword.Location = new System.Drawing.Point(12, 68);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPassword.Location = new System.Drawing.Point(140, 65);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(232, 26);
            this.txtPassword.TabIndex = 3;
            // 
            // lblHint
            // 
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHint.Location = new System.Drawing.Point(140, 97);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(232, 46);
            this.lblHint.TabIndex = 4;
            this.lblHint.Text = "The account has to exist on this machine. An empty password keeps the one that is a" +
    "lready stored.";
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(140, 152);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(110, 26);
            this.bnSave.TabIndex = 5;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(262, 152);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(110, 26);
            this.bnCancel.TabIndex = 6;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // FrmEndpointUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 190);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmEndpointUser";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ServiceManager account";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
