namespace MeshApp
{
    partial class frmViewGroup
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
            this.labName = new System.Windows.Forms.Label();
            this.mnuCopyUtility = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.labIcon = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.mnuGroupImage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuChangePhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRemovePhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopyUtility.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.mnuGroupImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // labName
            // 
            this.labName.AutoEllipsis = true;
            this.labName.ContextMenuStrip = this.mnuCopyUtility;
            this.labName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labName.Location = new System.Drawing.Point(10, 8);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(261, 20);
            this.labName.TabIndex = 42;
            this.labName.Text = "Group";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            // labIcon
            // 
            this.labIcon.BackColor = System.Drawing.Color.Gray;
            this.labIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labIcon.Font = new System.Drawing.Font("Arial", 90F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labIcon.ForeColor = System.Drawing.Color.White;
            this.labIcon.Location = new System.Drawing.Point(14, 36);
            this.labIcon.Name = "labIcon";
            this.labIcon.Size = new System.Drawing.Size(256, 256);
            this.labIcon.TabIndex = 43;
            this.labIcon.Text = "Gr";
            this.labIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labIcon.MouseEnter += new System.EventHandler(this.labIcon_MouseEnter);
            this.labIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labIcon_MouseUp);
            // 
            // picIcon
            // 
            this.picIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picIcon.Image = global::MeshApp.Properties.Resources.change_photo;
            this.picIcon.Location = new System.Drawing.Point(14, 36);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(256, 256);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picIcon.TabIndex = 46;
            this.picIcon.TabStop = false;
            this.picIcon.Visible = false;
            this.picIcon.MouseEnter += new System.EventHandler(this.picIcon_MouseEnter);
            this.picIcon.MouseLeave += new System.EventHandler(this.picIcon_MouseLeave);
            this.picIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labIcon_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(197, 300);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 48;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // mnuGroupImage
            // 
            this.mnuGroupImage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuChangePhoto,
            this.mnuRemovePhoto});
            this.mnuGroupImage.Name = "mnuProfileImage";
            this.mnuGroupImage.Size = new System.Drawing.Size(153, 48);
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
            // frmViewGroup
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(284, 331);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.labIcon);
            this.Controls.Add(this.labName);
            this.Controls.Add(this.picIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmViewGroup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Group Viewer";
            this.mnuCopyUtility.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.mnuGroupImage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Label labIcon;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ContextMenuStrip mnuGroupImage;
        private System.Windows.Forms.ToolStripMenuItem mnuChangePhoto;
        private System.Windows.Forms.ToolStripMenuItem mnuRemovePhoto;
        private System.Windows.Forms.ContextMenuStrip mnuCopyUtility;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
    }
}