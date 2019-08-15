namespace MeshApp
{
    partial class frmSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
            this.label14 = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtProfilePassword = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkAllowOnlyLocalInvitations = new System.Windows.Forms.CheckBox();
            this.chkAllowInvitations = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbProxy = new System.Windows.Forms.ComboBox();
            this.chkProxyAuth = new System.Windows.Forms.CheckBox();
            this.txtProxyPass = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtProxyUser = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtProxyPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtProxyAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkUPnP = new System.Windows.Forms.CheckBox();
            this.btnBrowseDLFolder = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDownloadFolder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnCheckProxy = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(6, 16);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(429, 33);
            this.label14.TabIndex = 38;
            this.label14.Text = "To protect from unauthorized access to your profile, enter a strong encryption pa" +
    "ssword below to stored your profile securely on this computer.";
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Location = new System.Drawing.Point(153, 83);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '#';
            this.txtConfirmPassword.Size = new System.Drawing.Size(220, 20);
            this.txtConfirmPassword.TabIndex = 36;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(57, 86);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(90, 13);
            this.label13.TabIndex = 35;
            this.label13.Text = "Confirm password";
            // 
            // txtProfilePassword
            // 
            this.txtProfilePassword.Location = new System.Drawing.Point(153, 57);
            this.txtProfilePassword.Name = "txtProfilePassword";
            this.txtProfilePassword.PasswordChar = '#';
            this.txtProfilePassword.Size = new System.Drawing.Size(220, 20);
            this.txtProfilePassword.TabIndex = 34;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(63, 60);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(84, 13);
            this.label12.TabIndex = 33;
            this.label12.Text = "Profile password";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.txtProfilePassword);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.txtConfirmPassword);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(440, 159);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Profile Password";
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(6, 111);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(429, 45);
            this.label15.TabIndex = 41;
            this.label15.Text = resources.GetString("label15.Text");
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkAllowOnlyLocalInvitations);
            this.groupBox2.Controls.Add(this.chkAllowInvitations);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.cmbProxy);
            this.groupBox2.Controls.Add(this.chkProxyAuth);
            this.groupBox2.Controls.Add(this.txtProxyPass);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.txtProxyUser);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.txtProxyPort);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtProxyAddress);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.chkUPnP);
            this.groupBox2.Controls.Add(this.btnBrowseDLFolder);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtDownloadFolder);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.txtPort);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 177);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(440, 243);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Profile Settings";
            // 
            // chkAllowOnlyLocalInvitations
            // 
            this.chkAllowOnlyLocalInvitations.AutoSize = true;
            this.chkAllowOnlyLocalInvitations.Location = new System.Drawing.Point(120, 104);
            this.chkAllowOnlyLocalInvitations.Name = "chkAllowOnlyLocalInvitations";
            this.chkAllowOnlyLocalInvitations.Size = new System.Drawing.Size(304, 17);
            this.chkAllowOnlyLocalInvitations.TabIndex = 7;
            this.chkAllowOnlyLocalInvitations.Text = "Allow Only Local (LAN, WiFi or Tor) Private Chat Invitations";
            this.chkAllowOnlyLocalInvitations.UseVisualStyleBackColor = true;
            // 
            // chkAllowInvitations
            // 
            this.chkAllowInvitations.AutoSize = true;
            this.chkAllowInvitations.Location = new System.Drawing.Point(101, 81);
            this.chkAllowInvitations.Name = "chkAllowInvitations";
            this.chkAllowInvitations.Size = new System.Drawing.Size(205, 17);
            this.chkAllowInvitations.TabIndex = 6;
            this.chkAllowInvitations.Text = "Allow Inbound Private Chat Invitations";
            this.chkAllowInvitations.UseVisualStyleBackColor = true;
            this.chkAllowInvitations.CheckedChanged += new System.EventHandler(this.chkAllowInvitations_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(35, 163);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(60, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "Proxy Type";
            // 
            // cmbProxy
            // 
            this.cmbProxy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProxy.FormattingEnabled = true;
            this.cmbProxy.Items.AddRange(new object[] {
            "Proxy Disabled",
            "Http Proxy",
            "Socks 5 Proxy"});
            this.cmbProxy.Location = new System.Drawing.Point(101, 160);
            this.cmbProxy.Name = "cmbProxy";
            this.cmbProxy.Size = new System.Drawing.Size(167, 21);
            this.cmbProxy.TabIndex = 9;
            this.cmbProxy.SelectedIndexChanged += new System.EventHandler(this.cmbProxy_SelectedIndexChanged);
            // 
            // chkProxyAuth
            // 
            this.chkProxyAuth.AutoSize = true;
            this.chkProxyAuth.Location = new System.Drawing.Point(279, 162);
            this.chkProxyAuth.Name = "chkProxyAuth";
            this.chkProxyAuth.Size = new System.Drawing.Size(159, 17);
            this.chkProxyAuth.TabIndex = 12;
            this.chkProxyAuth.Text = "Enable Proxy Authentication";
            this.chkProxyAuth.UseVisualStyleBackColor = true;
            this.chkProxyAuth.CheckedChanged += new System.EventHandler(this.chkProxyAuth_CheckedChanged);
            // 
            // txtProxyPass
            // 
            this.txtProxyPass.Location = new System.Drawing.Point(335, 213);
            this.txtProxyPass.MaxLength = 255;
            this.txtProxyPass.Name = "txtProxyPass";
            this.txtProxyPass.PasswordChar = '#';
            this.txtProxyPass.Size = new System.Drawing.Size(96, 20);
            this.txtProxyPass.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(276, 216);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 50;
            this.label7.Text = "Password";
            // 
            // txtProxyUser
            // 
            this.txtProxyUser.Location = new System.Drawing.Point(335, 187);
            this.txtProxyUser.MaxLength = 255;
            this.txtProxyUser.Name = "txtProxyUser";
            this.txtProxyUser.Size = new System.Drawing.Size(96, 20);
            this.txtProxyUser.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(274, 190);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 48;
            this.label8.Text = "Username";
            // 
            // txtProxyPort
            // 
            this.txtProxyPort.Location = new System.Drawing.Point(178, 213);
            this.txtProxyPort.MaxLength = 5;
            this.txtProxyPort.Name = "txtProxyPort";
            this.txtProxyPort.Size = new System.Drawing.Size(45, 20);
            this.txtProxyPort.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(117, 216);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 46;
            this.label6.Text = "Proxy Port";
            // 
            // txtProxyAddress
            // 
            this.txtProxyAddress.Location = new System.Drawing.Point(178, 187);
            this.txtProxyAddress.MaxLength = 255;
            this.txtProxyAddress.Name = "txtProxyAddress";
            this.txtProxyAddress.Size = new System.Drawing.Size(90, 20);
            this.txtProxyAddress.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(98, 190);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 44;
            this.label5.Text = "Proxy Address";
            // 
            // chkUPnP
            // 
            this.chkUPnP.AutoSize = true;
            this.chkUPnP.Location = new System.Drawing.Point(101, 127);
            this.chkUPnP.Name = "chkUPnP";
            this.chkUPnP.Size = new System.Drawing.Size(90, 17);
            this.chkUPnP.TabIndex = 8;
            this.chkUPnP.Text = "Enable UPnP";
            this.chkUPnP.UseVisualStyleBackColor = true;
            // 
            // btnBrowseDLFolder
            // 
            this.btnBrowseDLFolder.Location = new System.Drawing.Point(378, 18);
            this.btnBrowseDLFolder.Name = "btnBrowseDLFolder";
            this.btnBrowseDLFolder.Size = new System.Drawing.Size(53, 22);
            this.btnBrowseDLFolder.TabIndex = 2;
            this.btnBrowseDLFolder.Text = "Bro&wse";
            this.btnBrowseDLFolder.UseVisualStyleBackColor = true;
            this.btnBrowseDLFolder.Click += new System.EventHandler(this.btnBrowseDLFolder_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(162, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(195, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "(set 0 for random; requires profile restart)";
            // 
            // txtDownloadFolder
            // 
            this.txtDownloadFolder.Location = new System.Drawing.Point(101, 19);
            this.txtDownloadFolder.MaxLength = 255;
            this.txtDownloadFolder.Name = "txtDownloadFolder";
            this.txtDownloadFolder.ReadOnly = true;
            this.txtDownloadFolder.Size = new System.Drawing.Size(271, 20);
            this.txtDownloadFolder.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Download Folder";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(101, 45);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(55, 20);
            this.txtPort.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Incoming Port";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(296, 431);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(377, 431);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnCheckProxy
            // 
            this.btnCheckProxy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCheckProxy.Location = new System.Drawing.Point(12, 431);
            this.btnCheckProxy.Name = "btnCheckProxy";
            this.btnCheckProxy.Size = new System.Drawing.Size(75, 23);
            this.btnCheckProxy.TabIndex = 3;
            this.btnCheckProxy.Text = "Check Proxy";
            this.btnCheckProxy.UseVisualStyleBackColor = true;
            this.btnCheckProxy.Click += new System.EventHandler(this.btnCheckProxy_Click);
            // 
            // frmSettings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(464, 461);
            this.Controls.Add(this.btnCheckProxy);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Profile Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtProfilePassword;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDownloadFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnBrowseDLFolder;
        private System.Windows.Forms.CheckBox chkUPnP;
        private System.Windows.Forms.TextBox txtProxyPass;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtProxyUser;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtProxyPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtProxyAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkProxyAuth;
        private System.Windows.Forms.Button btnCheckProxy;
        private System.Windows.Forms.ComboBox cmbProxy;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox chkAllowInvitations;
        private System.Windows.Forms.CheckBox chkAllowOnlyLocalInvitations;
    }
}