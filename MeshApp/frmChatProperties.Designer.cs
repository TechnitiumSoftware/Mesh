namespace MeshApp
{
    partial class frmChatProperties
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
            this.btnClose = new System.Windows.Forms.Button();
            this.lstPeerInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuDhtInfo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showPeersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.labNetworkName = new System.Windows.Forms.Label();
            this.txtNetworkName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.labPassword = new System.Windows.Forms.Label();
            this.chkShowSecret = new System.Windows.Forms.CheckBox();
            this.chkLocalNetworkOnly = new System.Windows.Forms.CheckBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.txtNetworkId = new System.Windows.Forms.TextBox();
            this.labNetworkId = new System.Windows.Forms.Label();
            this.mnuDhtInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(497, 291);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lstPeerInfo
            // 
            this.lstPeerInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstPeerInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader4});
            this.lstPeerInfo.ContextMenuStrip = this.mnuDhtInfo;
            this.lstPeerInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.lstPeerInfo.FullRowSelect = true;
            this.lstPeerInfo.HideSelection = false;
            this.lstPeerInfo.Location = new System.Drawing.Point(12, 142);
            this.lstPeerInfo.MultiSelect = false;
            this.lstPeerInfo.Name = "lstPeerInfo";
            this.lstPeerInfo.Size = new System.Drawing.Size(560, 140);
            this.lstPeerInfo.TabIndex = 4;
            this.lstPeerInfo.UseCompatibleStateImageBehavior = false;
            this.lstPeerInfo.View = System.Windows.Forms.View.Details;
            this.lstPeerInfo.DoubleClick += new System.EventHandler(this.lstPeerInfo_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 320;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Update In";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Peers";
            this.columnHeader4.Width = 80;
            // 
            // mnuDhtInfo
            // 
            this.mnuDhtInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPeersToolStripMenuItem});
            this.mnuDhtInfo.Name = "contextMenuStrip1";
            this.mnuDhtInfo.Size = new System.Drawing.Size(135, 26);
            // 
            // showPeersToolStripMenuItem
            // 
            this.showPeersToolStripMenuItem.Name = "showPeersToolStripMenuItem";
            this.showPeersToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.showPeersToolStripMenuItem.Text = "&Show Peers";
            this.showPeersToolStripMenuItem.Click += new System.EventHandler(this.showPeersToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label2.Location = new System.Drawing.Point(12, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 15);
            this.label2.TabIndex = 51;
            this.label2.Text = "Peer Info";
            // 
            // labNetworkName
            // 
            this.labNetworkName.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labNetworkName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.labNetworkName.Location = new System.Drawing.Point(30, 44);
            this.labNetworkName.Name = "labNetworkName";
            this.labNetworkName.Size = new System.Drawing.Size(114, 14);
            this.labNetworkName.TabIndex = 52;
            this.labNetworkName.Text = "Peer\'s User Id";
            this.labNetworkName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtNetworkName
            // 
            this.txtNetworkName.Location = new System.Drawing.Point(150, 42);
            this.txtNetworkName.Name = "txtNetworkName";
            this.txtNetworkName.ReadOnly = true;
            this.txtNetworkName.Size = new System.Drawing.Size(300, 20);
            this.txtNetworkName.TabIndex = 0;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(150, 68);
            this.txtPassword.MaxLength = 255;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '#';
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.Text = "########";
            // 
            // labPassword
            // 
            this.labPassword.AutoSize = true;
            this.labPassword.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.labPassword.Location = new System.Drawing.Point(79, 70);
            this.labPassword.Name = "labPassword";
            this.labPassword.Size = new System.Drawing.Size(65, 15);
            this.labPassword.TabIndex = 54;
            this.labPassword.Text = "Password";
            // 
            // chkShowSecret
            // 
            this.chkShowSecret.AutoSize = true;
            this.chkShowSecret.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.chkShowSecret.Location = new System.Drawing.Point(356, 70);
            this.chkShowSecret.Name = "chkShowSecret";
            this.chkShowSecret.Size = new System.Drawing.Size(102, 17);
            this.chkShowSecret.TabIndex = 2;
            this.chkShowSecret.Text = "&Show Password";
            this.chkShowSecret.UseVisualStyleBackColor = true;
            this.chkShowSecret.CheckedChanged += new System.EventHandler(this.chkShowSecret_CheckedChanged);
            // 
            // chkLocalNetworkOnly
            // 
            this.chkLocalNetworkOnly.AutoSize = true;
            this.chkLocalNetworkOnly.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.chkLocalNetworkOnly.Location = new System.Drawing.Point(150, 99);
            this.chkLocalNetworkOnly.Name = "chkLocalNetworkOnly";
            this.chkLocalNetworkOnly.Size = new System.Drawing.Size(259, 17);
            this.chkLocalNetworkOnly.TabIndex = 3;
            this.chkLocalNetworkOnly.Text = "&Enable only local network (LAN, WiFi or Tor) chat";
            this.chkLocalNetworkOnly.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(416, 291);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "&Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // txtNetworkId
            // 
            this.txtNetworkId.Location = new System.Drawing.Point(150, 16);
            this.txtNetworkId.Name = "txtNetworkId";
            this.txtNetworkId.ReadOnly = true;
            this.txtNetworkId.Size = new System.Drawing.Size(300, 20);
            this.txtNetworkId.TabIndex = 55;
            // 
            // labNetworkId
            // 
            this.labNetworkId.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labNetworkId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.labNetworkId.Location = new System.Drawing.Point(30, 18);
            this.labNetworkId.Name = "labNetworkId";
            this.labNetworkId.Size = new System.Drawing.Size(114, 14);
            this.labNetworkId.TabIndex = 56;
            this.labNetworkId.Text = "Network ID";
            this.labNetworkId.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // frmChatProperties
            // 
            this.AcceptButton = this.btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(584, 321);
            this.Controls.Add(this.txtNetworkId);
            this.Controls.Add(this.labNetworkId);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.chkLocalNetworkOnly);
            this.Controls.Add(this.chkShowSecret);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.labPassword);
            this.Controls.Add(this.txtNetworkName);
            this.Controls.Add(this.labNetworkName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstPeerInfo);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChatProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chat Properties";
            this.mnuDhtInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListView lstPeerInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ContextMenuStrip mnuDhtInfo;
        private System.Windows.Forms.ToolStripMenuItem showPeersToolStripMenuItem;
        private System.Windows.Forms.Label labNetworkName;
        private System.Windows.Forms.TextBox txtNetworkName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label labPassword;
        private System.Windows.Forms.CheckBox chkShowSecret;
        private System.Windows.Forms.CheckBox chkLocalNetworkOnly;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.TextBox txtNetworkId;
        private System.Windows.Forms.Label labNetworkId;
    }
}