namespace ServiceManager.Extensions.WebhookNotifier.Forms
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
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtContentType = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtRetries = new System.Windows.Forms.TextBox();
            this.chkIgnoreCertificateErrors = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtHeaders = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.bnDefaultBody = new System.Windows.Forms.Button();
            this.bnTest = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "URL:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(140, 20);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(440, 20);
            this.txtUrl.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 53);
            this.label2.Name = "label2";
            this.label2.TabIndex = 2;
            this.label2.Text = "Method:";
            // 
            // cmbMethod
            // 
            this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMethod.Items.AddRange(new object[] {
            "POST",
            "PUT",
            "PATCH"});
            this.cmbMethod.Location = new System.Drawing.Point(140, 50);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(90, 21);
            this.cmbMethod.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 83);
            this.label3.Name = "label3";
            this.label3.TabIndex = 4;
            this.label3.Text = "Content type:";
            // 
            // txtContentType
            // 
            this.txtContentType.Location = new System.Drawing.Point(140, 80);
            this.txtContentType.Name = "txtContentType";
            this.txtContentType.Size = new System.Drawing.Size(250, 20);
            this.txtContentType.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 113);
            this.label4.Name = "label4";
            this.label4.TabIndex = 6;
            this.label4.Text = "Timeout seconds:";
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new System.Drawing.Point(140, 110);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(60, 20);
            this.txtTimeout.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(230, 113);
            this.label5.Name = "label5";
            this.label5.TabIndex = 8;
            this.label5.Text = "Retries:";
            // 
            // txtRetries
            // 
            this.txtRetries.Location = new System.Drawing.Point(300, 110);
            this.txtRetries.Name = "txtRetries";
            this.txtRetries.Size = new System.Drawing.Size(60, 20);
            this.txtRetries.TabIndex = 9;
            // 
            // chkIgnoreCertificateErrors
            // 
            this.chkIgnoreCertificateErrors.AutoSize = true;
            this.chkIgnoreCertificateErrors.Location = new System.Drawing.Point(140, 140);
            this.chkIgnoreCertificateErrors.Name = "chkIgnoreCertificateErrors";
            this.chkIgnoreCertificateErrors.Size = new System.Drawing.Size(300, 17);
            this.chkIgnoreCertificateErrors.TabIndex = 10;
            this.chkIgnoreCertificateErrors.Text = "Accept any certificate (only inside a trusted network)";
            this.chkIgnoreCertificateErrors.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 172);
            this.label6.Name = "label6";
            this.label6.TabIndex = 11;
            this.label6.Text = "Additional headers, one per line as \"Name: value\":";
            // 
            // txtHeaders
            // 
            this.txtHeaders.Location = new System.Drawing.Point(20, 190);
            this.txtHeaders.Multiline = true;
            this.txtHeaders.Name = "txtHeaders";
            this.txtHeaders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHeaders.Size = new System.Drawing.Size(560, 60);
            this.txtHeaders.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 262);
            this.label7.Name = "label7";
            this.label7.TabIndex = 13;
            this.label7.Text = "Body:";
            // 
            // txtBody
            // 
            this.txtBody.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBody.Location = new System.Drawing.Point(20, 280);
            this.txtBody.Multiline = true;
            this.txtBody.Name = "txtBody";
            this.txtBody.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBody.Size = new System.Drawing.Size(560, 160);
            this.txtBody.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 447);
            this.label8.Name = "label8";
            this.label8.TabIndex = 15;
            this.label8.Text = "{text} {details} {kind} {level} {service} {instance} {timestamp} are replaced, every" +
    "thing else is sent as it is.";
            // 
            // bnDefaultBody
            // 
            this.bnDefaultBody.Location = new System.Drawing.Point(20, 475);
            this.bnDefaultBody.Name = "bnDefaultBody";
            this.bnDefaultBody.Size = new System.Drawing.Size(120, 26);
            this.bnDefaultBody.TabIndex = 16;
            this.bnDefaultBody.Text = "Standard body";
            this.bnDefaultBody.UseVisualStyleBackColor = true;
            this.bnDefaultBody.Click += new System.EventHandler(this.bnDefaultBody_Click);
            // 
            // bnTest
            // 
            this.bnTest.Location = new System.Drawing.Point(150, 475);
            this.bnTest.Name = "bnTest";
            this.bnTest.Size = new System.Drawing.Size(150, 26);
            this.bnTest.TabIndex = 17;
            this.bnTest.Text = "Send test message";
            this.bnTest.UseVisualStyleBackColor = true;
            this.bnTest.Click += new System.EventHandler(this.bnTest_Click);
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(410, 475);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(80, 26);
            this.bnSave.TabIndex = 18;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(500, 475);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(80, 26);
            this.bnCancel.TabIndex = 19;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // FrmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 520);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbMethod);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtContentType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtRetries);
            this.Controls.Add(this.chkIgnoreCertificateErrors);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtHeaders);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.bnDefaultBody);
            this.Controls.Add(this.bnTest);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config Webhook";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbMethod;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtContentType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtRetries;
        private System.Windows.Forms.CheckBox chkIgnoreCertificateErrors;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtHeaders;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtBody;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button bnDefaultBody;
        private System.Windows.Forms.Button bnTest;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
