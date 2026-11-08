namespace ServiceManager.Extensions.ScriptNotifier.forms
{
    partial class frmConfig
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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtArgs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtWorkdir = new System.Windows.Forms.TextBox();
            this.chkStdin = new System.Windows.Forms.CheckBox();
            this.chkArgument = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bnTest = new System.Windows.Forms.Button();
            this.bnSave = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 28);
            this.label1.Name = "label1";
            this.label1.TabIndex = 0;
            this.label1.Text = "Program:";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(190, 25);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(330, 20);
            this.txtPath.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 56);
            this.label2.Name = "label2";
            this.label2.TabIndex = 2;
            this.label2.Text = "Arguments:";
            // 
            // txtArgs
            // 
            this.txtArgs.Location = new System.Drawing.Point(190, 53);
            this.txtArgs.Name = "txtArgs";
            this.txtArgs.Size = new System.Drawing.Size(330, 20);
            this.txtArgs.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 84);
            this.label3.Name = "label3";
            this.label3.TabIndex = 4;
            this.label3.Text = "Working directory:";
            // 
            // txtWorkdir
            // 
            this.txtWorkdir.Location = new System.Drawing.Point(190, 81);
            this.txtWorkdir.Name = "txtWorkdir";
            this.txtWorkdir.Size = new System.Drawing.Size(330, 20);
            this.txtWorkdir.TabIndex = 5;
            // 
            // chkStdin
            // 
            this.chkStdin.AutoSize = true;
            this.chkStdin.Location = new System.Drawing.Point(30, 112);
            this.chkStdin.Name = "chkStdin";
            this.chkStdin.Size = new System.Drawing.Size(200, 17);
            this.chkStdin.TabIndex = 6;
            this.chkStdin.Text = "Hand the event as json to the input";
            this.chkStdin.UseVisualStyleBackColor = true;
            // 
            // chkArgument
            // 
            this.chkArgument.AutoSize = true;
            this.chkArgument.Location = new System.Drawing.Point(30, 138);
            this.chkArgument.Name = "chkArgument";
            this.chkArgument.Size = new System.Drawing.Size(200, 17);
            this.chkArgument.TabIndex = 7;
            this.chkArgument.Text = "Hand the event as one extra argument";
            this.chkArgument.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 170);
            this.label4.Name = "label4";
            this.label4.TabIndex = 8;
            this.label4.Text = "Timeout seconds:";
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new System.Drawing.Point(190, 167);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(60, 20);
            this.txtTimeout.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 196);
            this.label5.Name = "label5";
            this.label5.TabIndex = 10;
            this.label5.Text = "The program is started directly, without a shell. The json carries instance, event, level, service, time, text and details.";
            // 
            // bnTest
            // 
            this.bnTest.Location = new System.Drawing.Point(30, 224);
            this.bnTest.Name = "bnTest";
            this.bnTest.Size = new System.Drawing.Size(150, 26);
            this.bnTest.TabIndex = 11;
            this.bnTest.Text = "Run test program";
            this.bnTest.UseVisualStyleBackColor = true;
            this.bnTest.Click += new System.EventHandler(this.bnTest_Click);
            // 
            // bnSave
            // 
            this.bnSave.Location = new System.Drawing.Point(350, 224);
            this.bnSave.Name = "bnSave";
            this.bnSave.Size = new System.Drawing.Size(90, 26);
            this.bnSave.TabIndex = 12;
            this.bnSave.Text = "Save";
            this.bnSave.UseVisualStyleBackColor = true;
            this.bnSave.Click += new System.EventHandler(this.bnSave_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.Location = new System.Drawing.Point(450, 224);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(90, 26);
            this.bnCancel.TabIndex = 13;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // frmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 268);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtArgs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtWorkdir);
            this.Controls.Add(this.chkStdin);
            this.Controls.Add(this.chkArgument);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.bnTest);
            this.Controls.Add(this.bnSave);
            this.Controls.Add(this.bnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Config Script hook";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtArgs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtWorkdir;
        private System.Windows.Forms.CheckBox chkStdin;
        private System.Windows.Forms.CheckBox chkArgument;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button bnTest;
        private System.Windows.Forms.Button bnSave;
        private System.Windows.Forms.Button bnCancel;
    }
}
