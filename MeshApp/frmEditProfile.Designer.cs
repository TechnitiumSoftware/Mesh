namespace MeshApp
{
    partial class frmEditProfile
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
            this.txtDisplayName = new System.Windows.Forms.TextBox();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.txtStatusMessage = new System.Windows.Forms.TextBox();
            this.labIcon = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCopyUserId = new System.Windows.Forms.Button();
            this.btnRandomUserId = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.mnuProfileImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuChangePhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRemovePhoto = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.mnuProfileImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDisplayName
            // 
            this.txtDisplayName.BackColor = System.Drawing.Color.White;
            this.txtDisplayName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDisplayName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtDisplayName.Location = new System.Drawing.Point(278, 30);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new System.Drawing.Size(294, 26);
            this.txtDisplayName.TabIndex = 0;
            this.txtDisplayName.Text = "Shreyas Zare";
            // 
            // txtUserId
            // 
            this.txtUserId.BackColor = System.Drawing.SystemColors.Control;
            this.txtUserId.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserId.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtUserId.Location = new System.Drawing.Point(278, 82);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.ReadOnly = true;
            this.txtUserId.Size = new System.Drawing.Size(294, 20);
            this.txtUserId.TabIndex = 1;
            this.txtUserId.Text = "userId-hash";
            // 
            // txtStatusMessage
            // 
            this.txtStatusMessage.BackColor = System.Drawing.Color.White;
            this.txtStatusMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatusMessage.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtStatusMessage.Location = new System.Drawing.Point(344, 158);
            this.txtStatusMessage.Name = "txtStatusMessage";
            this.txtStatusMessage.Size = new System.Drawing.Size(228, 20);
            this.txtStatusMessage.TabIndex = 5;
            this.txtStatusMessage.Text = "status message";
            // 
            // labIcon
            // 
            this.labIcon.BackColor = System.Drawing.Color.Gray;
            this.labIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labIcon.Font = new System.Drawing.Font("Arial", 90F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labIcon.ForeColor = System.Drawing.Color.White;
            this.labIcon.Location = new System.Drawing.Point(12, 12);
            this.labIcon.Name = "labIcon";
            this.labIcon.Size = new System.Drawing.Size(256, 256);
            this.labIcon.TabIndex = 41;
            this.labIcon.Text = "SZ";
            this.labIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labIcon.MouseEnter += new System.EventHandler(this.labIcon_MouseEnter);
            this.labIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labIcon_MouseUp);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(416, 246);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(497, 246);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Canc&el";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCopyUserId
            // 
            this.btnCopyUserId.Location = new System.Drawing.Point(512, 108);
            this.btnCopyUserId.Name = "btnCopyUserId";
            this.btnCopyUserId.Size = new System.Drawing.Size(60, 23);
            this.btnCopyUserId.TabIndex = 3;
            this.btnCopyUserId.Text = "&Copy";
            this.btnCopyUserId.UseVisualStyleBackColor = true;
            this.btnCopyUserId.Click += new System.EventHandler(this.btnCopyUserId_Click);
            // 
            // btnRandomUserId
            // 
            this.btnRandomUserId.Location = new System.Drawing.Point(446, 108);
            this.btnRandomUserId.Name = "btnRandomUserId";
            this.btnRandomUserId.Size = new System.Drawing.Size(60, 23);
            this.btnRandomUserId.TabIndex = 2;
            this.btnRandomUserId.Text = "&Random";
            this.btnRandomUserId.UseVisualStyleBackColor = true;
            this.btnRandomUserId.Click += new System.EventHandler(this.btnRandomUserId_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(275, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 15);
            this.label1.TabIndex = 47;
            this.label1.Text = "User Id";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(275, 140);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 48;
            this.label2.Text = "Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(275, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 49;
            this.label3.Text = "Display Name";
            // 
            // cmbStatus
            // 
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.FormattingEnabled = true;
            this.cmbStatus.Items.AddRange(new object[] {
            "Active",
            "Inactive",
            "Busy"});
            this.cmbStatus.Location = new System.Drawing.Point(278, 158);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new System.Drawing.Size(60, 21);
            this.cmbStatus.TabIndex = 4;
            // 
            // picIcon
            // 
            this.picIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picIcon.Image = global::MeshApp.Properties.Resources.change_photo;
            this.picIcon.Location = new System.Drawing.Point(12, 12);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(256, 256);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picIcon.TabIndex = 50;
            this.picIcon.TabStop = false;
            this.picIcon.Visible = false;
            this.picIcon.MouseEnter += new System.EventHandler(this.picIcon_MouseEnter);
            this.picIcon.MouseLeave += new System.EventHandler(this.picIcon_MouseLeave);
            this.picIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labIcon_MouseUp);
            // 
            // mnuProfileImage
            // 
            this.mnuProfileImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuChangePhoto,
            this.mnuRemovePhoto});
            this.mnuProfileImage.Name = "mnuProfileImage";
            this.mnuProfileImage.Size = new System.Drawing.Size(153, 48);
            // 
            // mnuChangePhoto
            // 
            this.mnuChangePhoto.Name = "mnuChangePhoto";
            this.mnuChangePhoto.Size = new System.Drawing.Size(152, 22);
            this.mnuChangePhoto.Text = "Change Photo";
            this.mnuChangePhoto.Click += new System.EventHandler(this.mnuChangePhoto_Click);
            // 
            // mnuRemovePhoto
            // 
            this.mnuRemovePhoto.Name = "mnuRemovePhoto";
            this.mnuRemovePhoto.Size = new System.Drawing.Size(152, 22);
            this.mnuRemovePhoto.Text = "Remove Photo";
            this.mnuRemovePhoto.Click += new System.EventHandler(this.mnuRemovePhoto_Click);
            // 
            // frmEditProfile
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(584, 281);
            this.Controls.Add(this.labIcon);
            this.Controls.Add(this.picIcon);
            this.Controls.Add(this.cmbStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRandomUserId);
            this.Controls.Add(this.btnCopyUserId);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtStatusMessage);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.txtDisplayName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEditProfile";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "My Profile";
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.mnuProfileImage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDisplayName;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.TextBox txtStatusMessage;
        private System.Windows.Forms.Label labIcon;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCopyUserId;
        private System.Windows.Forms.Button btnRandomUserId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.ContextMenuStrip mnuProfileImage;
        private System.Windows.Forms.ToolStripMenuItem mnuChangePhoto;
        private System.Windows.Forms.ToolStripMenuItem mnuRemovePhoto;
    }
}