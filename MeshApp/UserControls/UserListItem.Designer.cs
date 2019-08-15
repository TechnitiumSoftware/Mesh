namespace MeshApp.UserControls
{
    partial class UserListItem
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
            this.labStatusMessage = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.labIcon = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // labStatusMessage
            // 
            this.labStatusMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labStatusMessage.AutoEllipsis = true;
            this.labStatusMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labStatusMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labStatusMessage.Location = new System.Drawing.Point(54, 31);
            this.labStatusMessage.Name = "labStatusMessage";
            this.labStatusMessage.Size = new System.Drawing.Size(163, 15);
            this.labStatusMessage.TabIndex = 2;
            this.labStatusMessage.Text = "Push the limits!";
            this.labStatusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labName
            // 
            this.labName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labName.AutoEllipsis = true;
            this.labName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labName.Location = new System.Drawing.Point(53, 7);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(164, 22);
            this.labName.TabIndex = 1;
            this.labName.Text = "Shreyas Zare";
            this.labName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labIcon
            // 
            this.labIcon.BackColor = System.Drawing.Color.Gray;
            this.labIcon.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labIcon.ForeColor = System.Drawing.Color.White;
            this.labIcon.Location = new System.Drawing.Point(3, 3);
            this.labIcon.Name = "labIcon";
            this.labIcon.Size = new System.Drawing.Size(48, 48);
            this.labIcon.TabIndex = 0;
            this.labIcon.Text = "SZ";
            this.labIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(3, 3);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(48, 48);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picIcon.TabIndex = 4;
            this.picIcon.TabStop = false;
            this.picIcon.Visible = false;
            // 
            // UserListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.Controls.Add(this.labStatusMessage);
            this.Controls.Add(this.labName);
            this.Controls.Add(this.labIcon);
            this.Controls.Add(this.picIcon);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "UserListItem";
            this.Size = new System.Drawing.Size(220, 55);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labIcon;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Label labStatusMessage;
        private System.Windows.Forms.PictureBox picIcon;
    }
}
