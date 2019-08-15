namespace MeshApp
{
    partial class frmViewUserDetails
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
            this.labIcon = new System.Windows.Forms.Label();
            this.labUserId = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.lstConnectedWith = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstNotConnectedWith = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label15 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labNetworkStatus = new System.Windows.Forms.Label();
            this.picNetwork = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labCipherSuite = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.labStatus = new System.Windows.Forms.Label();
            this.mnuCopyUtility = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picNetwork)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.mnuCopyUtility.SuspendLayout();
            this.SuspendLayout();
            // 
            // labIcon
            // 
            this.labIcon.BackColor = System.Drawing.Color.Gray;
            this.labIcon.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labIcon.ForeColor = System.Drawing.Color.White;
            this.labIcon.Location = new System.Drawing.Point(12, 12);
            this.labIcon.Name = "labIcon";
            this.labIcon.Size = new System.Drawing.Size(48, 48);
            this.labIcon.TabIndex = 1;
            this.labIcon.Text = "SZ";
            this.labIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labUserId
            // 
            this.labUserId.AutoEllipsis = true;
            this.labUserId.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labUserId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labUserId.Location = new System.Drawing.Point(63, 47);
            this.labUserId.Name = "labUserId";
            this.labUserId.Size = new System.Drawing.Size(279, 14);
            this.labUserId.TabIndex = 4;
            this.labUserId.Text = "userid-hash";
            this.labUserId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labUserId.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labUserId_MouseUp);
            // 
            // labName
            // 
            this.labName.AutoEllipsis = true;
            this.labName.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.labName.Location = new System.Drawing.Point(62, 13);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(280, 17);
            this.labName.TabIndex = 3;
            this.labName.Text = "Shreyas Zare";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labUserId_MouseUp);
            // 
            // lstConnectedWith
            // 
            this.lstConnectedWith.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstConnectedWith.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstConnectedWith.FullRowSelect = true;
            this.lstConnectedWith.Location = new System.Drawing.Point(12, 149);
            this.lstConnectedWith.Name = "lstConnectedWith";
            this.lstConnectedWith.Size = new System.Drawing.Size(330, 90);
            this.lstConnectedWith.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstConnectedWith.TabIndex = 5;
            this.lstConnectedWith.UseCompatibleStateImageBehavior = false;
            this.lstConnectedWith.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Peer";
            this.columnHeader1.Width = 170;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "IP Address";
            this.columnHeader2.Width = 130;
            // 
            // lstNotConnectedWith
            // 
            this.lstNotConnectedWith.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstNotConnectedWith.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.lstNotConnectedWith.FullRowSelect = true;
            this.lstNotConnectedWith.Location = new System.Drawing.Point(12, 265);
            this.lstNotConnectedWith.Name = "lstNotConnectedWith";
            this.lstNotConnectedWith.Size = new System.Drawing.Size(330, 90);
            this.lstNotConnectedWith.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstNotConnectedWith.TabIndex = 6;
            this.lstNotConnectedWith.UseCompatibleStateImageBehavior = false;
            this.lstNotConnectedWith.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Peer";
            this.columnHeader3.Width = 170;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "IP Address";
            this.columnHeader4.Width = 130;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label15.Location = new System.Drawing.Point(9, 131);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(97, 15);
            this.label15.TabIndex = 35;
            this.label15.Text = "Connected With";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label1.Location = new System.Drawing.Point(9, 247);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 15);
            this.label1.TabIndex = 36;
            this.label1.Text = "Not Connected With";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(267, 362);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 37;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label2.Location = new System.Drawing.Point(9, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 15);
            this.label2.TabIndex = 50;
            this.label2.Text = "Network Status";
            // 
            // labNetworkStatus
            // 
            this.labNetworkStatus.AutoEllipsis = true;
            this.labNetworkStatus.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labNetworkStatus.ForeColor = System.Drawing.Color.DimGray;
            this.labNetworkStatus.Location = new System.Drawing.Point(135, 73);
            this.labNetworkStatus.Name = "labNetworkStatus";
            this.labNetworkStatus.Size = new System.Drawing.Size(207, 17);
            this.labNetworkStatus.TabIndex = 51;
            this.labNetworkStatus.Text = "No Network";
            // 
            // picNetwork
            // 
            this.picNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picNetwork.Image = global::MeshApp.Properties.Resources.NoNetwork;
            this.picNetwork.Location = new System.Drawing.Point(110, 66);
            this.picNetwork.Name = "picNetwork";
            this.picNetwork.Size = new System.Drawing.Size(24, 24);
            this.picNetwork.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picNetwork.TabIndex = 52;
            this.picNetwork.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label3.Location = new System.Drawing.Point(9, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 15);
            this.label3.TabIndex = 53;
            this.label3.Text = "Cipher Suite";
            // 
            // labCipherSuite
            // 
            this.labCipherSuite.AutoEllipsis = true;
            this.labCipherSuite.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labCipherSuite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labCipherSuite.Location = new System.Drawing.Point(9, 111);
            this.labCipherSuite.Name = "labCipherSuite";
            this.labCipherSuite.Size = new System.Drawing.Size(333, 15);
            this.labCipherSuite.TabIndex = 54;
            this.labCipherSuite.Text = "DHE2048_RSA2048_WITH_AES256_CBC_HMAC_SHA256";
            this.labCipherSuite.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labUserId_MouseUp);
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(12, 12);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(48, 48);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picIcon.TabIndex = 55;
            this.picIcon.TabStop = false;
            this.picIcon.Visible = false;
            // 
            // labStatus
            // 
            this.labStatus.AutoEllipsis = true;
            this.labStatus.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labStatus.Location = new System.Drawing.Point(63, 31);
            this.labStatus.Name = "labStatus";
            this.labStatus.Size = new System.Drawing.Size(279, 14);
            this.labStatus.TabIndex = 56;
            this.labStatus.Text = "(Busy) Status Message";
            this.labStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labStatus.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labUserId_MouseUp);
            // 
            // mnuCopyUtility
            // 
            this.mnuCopyUtility.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopy});
            this.mnuCopyUtility.Name = "mnuCopyUtility";
            this.mnuCopyUtility.Size = new System.Drawing.Size(103, 26);
            // 
            // mnuCopy
            // 
            this.mnuCopy.Name = "mnuCopy";
            this.mnuCopy.Size = new System.Drawing.Size(102, 22);
            this.mnuCopy.Text = "&Copy";
            this.mnuCopy.Click += new System.EventHandler(this.mnuCopy_Click);
            // 
            // frmViewUserDetails
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(354, 391);
            this.Controls.Add(this.labStatus);
            this.Controls.Add(this.labCipherSuite);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.picNetwork);
            this.Controls.Add(this.labNetworkStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.lstNotConnectedWith);
            this.Controls.Add(this.lstConnectedWith);
            this.Controls.Add(this.labUserId);
            this.Controls.Add(this.labName);
            this.Controls.Add(this.labIcon);
            this.Controls.Add(this.picIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmViewUserDetails";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Details";
            ((System.ComponentModel.ISupportInitialize)(this.picNetwork)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.mnuCopyUtility.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labIcon;
        private System.Windows.Forms.Label labUserId;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.ListView lstConnectedWith;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ListView lstNotConnectedWith;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labNetworkStatus;
        private System.Windows.Forms.PictureBox picNetwork;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labCipherSuite;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label labStatus;
        private System.Windows.Forms.ContextMenuStrip mnuCopyUtility;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
    }
}