namespace MeshApp
{
    partial class frmProxyConfig
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
            this.txtProxyPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtProxyAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.chkProxyAuth = new System.Windows.Forms.CheckBox();
            this.txtProxyPass = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtProxyUser = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCheckProxy = new System.Windows.Forms.Button();
            this.cmbProxy = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtProxyPort
            // 
            this.txtProxyPort.Location = new System.Drawing.Point(157, 101);
            this.txtProxyPort.MaxLength = 5;
            this.txtProxyPort.Name = "txtProxyPort";
            this.txtProxyPort.Size = new System.Drawing.Size(45, 20);
            this.txtProxyPort.TabIndex = 3;
            this.txtProxyPort.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(96, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 50;
            this.label6.Text = "Proxy Port";
            // 
            // txtProxyAddress
            // 
            this.txtProxyAddress.Location = new System.Drawing.Point(157, 75);
            this.txtProxyAddress.MaxLength = 255;
            this.txtProxyAddress.Name = "txtProxyAddress";
            this.txtProxyAddress.Size = new System.Drawing.Size(140, 20);
            this.txtProxyAddress.TabIndex = 2;
            this.txtProxyAddress.Text = "127.0.0.1";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(77, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 49;
            this.label5.Text = "Proxy Address";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(360, 32);
            this.label1.TabIndex = 51;
            this.label1.Text = "Please enter the proxy server details below that Mesh will use for all network co" +
    "mmunications.";
            // 
            // chkProxyAuth
            // 
            this.chkProxyAuth.AutoSize = true;
            this.chkProxyAuth.Location = new System.Drawing.Point(157, 127);
            this.chkProxyAuth.Name = "chkProxyAuth";
            this.chkProxyAuth.Size = new System.Drawing.Size(130, 17);
            this.chkProxyAuth.TabIndex = 4;
            this.chkProxyAuth.Text = "Enable Authentication";
            this.chkProxyAuth.UseVisualStyleBackColor = true;
            this.chkProxyAuth.CheckedChanged += new System.EventHandler(this.chkProxyAuth_CheckedChanged);
            // 
            // txtProxyPass
            // 
            this.txtProxyPass.Enabled = false;
            this.txtProxyPass.Location = new System.Drawing.Point(157, 176);
            this.txtProxyPass.MaxLength = 255;
            this.txtProxyPass.Name = "txtProxyPass";
            this.txtProxyPass.PasswordChar = '#';
            this.txtProxyPass.Size = new System.Drawing.Size(140, 20);
            this.txtProxyPass.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(98, 179);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 56;
            this.label7.Text = "Password";
            // 
            // txtProxyUser
            // 
            this.txtProxyUser.Enabled = false;
            this.txtProxyUser.Location = new System.Drawing.Point(157, 150);
            this.txtProxyUser.MaxLength = 255;
            this.txtProxyUser.Name = "txtProxyUser";
            this.txtProxyUser.Size = new System.Drawing.Size(140, 20);
            this.txtProxyUser.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(96, 153);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 55;
            this.label8.Text = "Username";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(297, 211);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(216, 211);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCheckProxy
            // 
            this.btnCheckProxy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCheckProxy.Location = new System.Drawing.Point(12, 211);
            this.btnCheckProxy.Name = "btnCheckProxy";
            this.btnCheckProxy.Size = new System.Drawing.Size(75, 23);
            this.btnCheckProxy.TabIndex = 7;
            this.btnCheckProxy.Text = "Check &Proxy";
            this.btnCheckProxy.UseVisualStyleBackColor = true;
            this.btnCheckProxy.Click += new System.EventHandler(this.btnCheckProxy_Click);
            // 
            // cmbProxy
            // 
            this.cmbProxy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProxy.FormattingEnabled = true;
            this.cmbProxy.Items.AddRange(new object[] {
            "Proxy Disabled",
            "Http Proxy",
            "Socks 5 Proxy"});
            this.cmbProxy.Location = new System.Drawing.Point(157, 48);
            this.cmbProxy.Name = "cmbProxy";
            this.cmbProxy.Size = new System.Drawing.Size(140, 21);
            this.cmbProxy.TabIndex = 1;
            this.cmbProxy.SelectedIndexChanged += new System.EventHandler(this.cmbProxy_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(91, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 61;
            this.label2.Text = "Proxy Type";
            // 
            // frmProxyConfig
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(384, 242);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbProxy);
            this.Controls.Add(this.btnCheckProxy);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtProxyPass);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtProxyUser);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.chkProxyAuth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtProxyPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtProxyAddress);
            this.Controls.Add(this.label5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProxyConfig";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Proxy";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProxyPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtProxyAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkProxyAuth;
        private System.Windows.Forms.TextBox txtProxyPass;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtProxyUser;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCheckProxy;
        private System.Windows.Forms.ComboBox cmbProxy;
        private System.Windows.Forms.Label label2;
    }
}