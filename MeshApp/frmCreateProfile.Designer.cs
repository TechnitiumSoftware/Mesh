namespace MeshApp
{
    partial class frmCreateProfile
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateProfile));
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtProfileDisplayName = new System.Windows.Forms.TextBox();
            this.chkEnableProxy = new System.Windows.Forms.CheckBox();
            this.rbImportRSA = new System.Windows.Forms.RadioButton();
            this.rbAutoGenRSA = new System.Windows.Forms.RadioButton();
            this.label14 = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtProfilePassword = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.btnBack = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label2.Location = new System.Drawing.Point(11, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 22);
            this.label2.TabIndex = 9;
            this.label2.Text = "Create Profile";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 115);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Name";
            // 
            // txtProfileDisplayName
            // 
            this.txtProfileDisplayName.Location = new System.Drawing.Point(100, 112);
            this.txtProfileDisplayName.MaxLength = 255;
            this.txtProfileDisplayName.Name = "txtProfileDisplayName";
            this.txtProfileDisplayName.Size = new System.Drawing.Size(200, 20);
            this.txtProfileDisplayName.TabIndex = 11;
            // 
            // chkEnableProxy
            // 
            this.chkEnableProxy.AutoSize = true;
            this.chkEnableProxy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.chkEnableProxy.Location = new System.Drawing.Point(25, 230);
            this.chkEnableProxy.Name = "chkEnableProxy";
            this.chkEnableProxy.Size = new System.Drawing.Size(88, 17);
            this.chkEnableProxy.TabIndex = 43;
            this.chkEnableProxy.Text = "&Enable Proxy";
            this.chkEnableProxy.UseVisualStyleBackColor = true;
            this.chkEnableProxy.CheckedChanged += new System.EventHandler(this.chkEnableProxy_CheckedChanged);
            // 
            // rbImportRSA
            // 
            this.rbImportRSA.AutoSize = true;
            this.rbImportRSA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.rbImportRSA.Location = new System.Drawing.Point(25, 198);
            this.rbImportRSA.Name = "rbImportRSA";
            this.rbImportRSA.Size = new System.Drawing.Size(156, 17);
            this.rbImportRSA.TabIndex = 41;
            this.rbImportRSA.Text = "&Import custom RSA key pair";
            this.rbImportRSA.UseVisualStyleBackColor = true;
            this.rbImportRSA.CheckedChanged += new System.EventHandler(this.rbImportRSA_CheckedChanged);
            // 
            // rbAutoGenRSA
            // 
            this.rbAutoGenRSA.AutoSize = true;
            this.rbAutoGenRSA.Checked = true;
            this.rbAutoGenRSA.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.rbAutoGenRSA.Location = new System.Drawing.Point(25, 175);
            this.rbAutoGenRSA.Name = "rbAutoGenRSA";
            this.rbAutoGenRSA.Size = new System.Drawing.Size(238, 17);
            this.rbAutoGenRSA.TabIndex = 40;
            this.rbAutoGenRSA.TabStop = true;
            this.rbAutoGenRSA.Text = "&Automatically generate RSA key pair (default)";
            this.rbAutoGenRSA.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label14.Location = new System.Drawing.Point(12, 263);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(360, 33);
            this.label14.TabIndex = 45;
            this.label14.Text = "To protect from unauthorized access to your profile, enter a strong encryption pa" +
    "ssword below to stored your profile securely on this computer.";
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Location = new System.Drawing.Point(110, 327);
            this.txtConfirmPassword.MaxLength = 255;
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '#';
            this.txtConfirmPassword.Size = new System.Drawing.Size(180, 20);
            this.txtConfirmPassword.TabIndex = 46;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label13.Location = new System.Drawing.Point(14, 330);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(90, 13);
            this.label13.TabIndex = 42;
            this.label13.Text = "Confirm password";
            // 
            // txtProfilePassword
            // 
            this.txtProfilePassword.Location = new System.Drawing.Point(110, 301);
            this.txtProfilePassword.MaxLength = 255;
            this.txtProfilePassword.Name = "txtProfilePassword";
            this.txtProfilePassword.PasswordChar = '#';
            this.txtProfilePassword.Size = new System.Drawing.Size(180, 20);
            this.txtProfilePassword.TabIndex = 44;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label12.Location = new System.Drawing.Point(20, 304);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(84, 13);
            this.label12.TabIndex = 39;
            this.label12.Text = "Profile password";
            // 
            // label15
            // 
            this.label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label15.Location = new System.Drawing.Point(12, 360);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(360, 63);
            this.label15.TabIndex = 47;
            this.label15.Text = resources.GetString("label15.Text");
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(216, 426);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 48;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // label3
            // 
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.label3.Location = new System.Drawing.Point(12, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(360, 67);
            this.label3.TabIndex = 50;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(63, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 51;
            this.label4.Text = "Type";
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Peer-to-Peer (Not Anonymous)",
            "Anonymous (Tor Hidden Service)"});
            this.cmbType.Location = new System.Drawing.Point(100, 138);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(200, 21);
            this.cmbType.TabIndex = 12;
            // 
            // btnBack
            // 
            this.btnBack.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnBack.Location = new System.Drawing.Point(297, 426);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 23);
            this.btnBack.TabIndex = 52;
            this.btnBack.Text = "&Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // frmCreateProfile
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnBack;
            this.ClientSize = new System.Drawing.Size(384, 461);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.chkEnableProxy);
            this.Controls.Add(this.rbImportRSA);
            this.Controls.Add(this.rbAutoGenRSA);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtProfilePassword);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtProfileDisplayName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreateProfile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mesh";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCreateProfile_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProfileDisplayName;
        private System.Windows.Forms.CheckBox chkEnableProxy;
        private System.Windows.Forms.RadioButton rbImportRSA;
        private System.Windows.Forms.RadioButton rbAutoGenRSA;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtProfilePassword;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Button btnBack;
    }
}