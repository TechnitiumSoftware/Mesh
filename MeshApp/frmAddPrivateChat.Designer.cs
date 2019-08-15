namespace MeshApp
{
    partial class frmAddPrivateChat
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtPeerUserId = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.chkLocalNetworkOnly = new System.Windows.Forms.CheckBox();
            this.txtInvitationMessage = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPeerDisplayName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(51, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtPeerUserId
            // 
            this.txtPeerUserId.Location = new System.Drawing.Point(100, 12);
            this.txtPeerUserId.MaxLength = 40;
            this.txtPeerUserId.Name = "txtPeerUserId";
            this.txtPeerUserId.Size = new System.Drawing.Size(300, 20);
            this.txtPeerUserId.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(316, 182);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 9;
            this.btnAdd.Text = "&Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(397, 182);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(97, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(171, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "(Your friend\'s 40 character user ID)";
            // 
            // chkLocalNetworkOnly
            // 
            this.chkLocalNetworkOnly.AutoSize = true;
            this.chkLocalNetworkOnly.Location = new System.Drawing.Point(100, 149);
            this.chkLocalNetworkOnly.Name = "chkLocalNetworkOnly";
            this.chkLocalNetworkOnly.Size = new System.Drawing.Size(259, 17);
            this.chkLocalNetworkOnly.TabIndex = 8;
            this.chkLocalNetworkOnly.Text = "Enable only local network (LAN, WiFi or Tor) chat";
            this.chkLocalNetworkOnly.UseVisualStyleBackColor = true;
            // 
            // txtInvitationMessage
            // 
            this.txtInvitationMessage.Location = new System.Drawing.Point(100, 100);
            this.txtInvitationMessage.MaxLength = 255;
            this.txtInvitationMessage.Name = "txtInvitationMessage";
            this.txtInvitationMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInvitationMessage.Size = new System.Drawing.Size(300, 20);
            this.txtInvitationMessage.TabIndex = 7;
            this.txtInvitationMessage.Text = "Hi! Lets connect with Mesh!";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(44, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Message";
            // 
            // txtPeerDisplayName
            // 
            this.txtPeerDisplayName.Location = new System.Drawing.Point(100, 56);
            this.txtPeerDisplayName.MaxLength = 255;
            this.txtPeerDisplayName.Name = "txtPeerDisplayName";
            this.txtPeerDisplayName.Size = new System.Drawing.Size(300, 20);
            this.txtPeerDisplayName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(97, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "(Your friend\'s name to display)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(97, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(244, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "(Invitation message to help your friend identify you)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 187);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(275, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "NOTE: Your friend must be online to accept private chat.";
            // 
            // frmAddPrivateChat
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(484, 211);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPeerDisplayName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtInvitationMessage);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkLocalNetworkOnly);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtPeerUserId);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddPrivateChat";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Private Chat";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPeerUserId;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkLocalNetworkOnly;
        private System.Windows.Forms.TextBox txtInvitationMessage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPeerDisplayName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
    }
}