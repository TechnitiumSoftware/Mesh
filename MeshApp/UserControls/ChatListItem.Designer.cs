namespace MeshApp.UserControls
{
    partial class ChatListItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labIcon = new System.Windows.Forms.Label();
            this.labTitle = new System.Windows.Forms.Label();
            this.labLastMessage = new System.Windows.Forms.Label();
            this.labUnreadMessageCount = new System.Windows.Forms.Label();
            this.picIcon = new System.Windows.Forms.PictureBox();
            this.labLastMessageDate = new System.Windows.Forms.Label();
            this.timerTypingNotification = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // labIcon
            // 
            this.labIcon.BackColor = System.Drawing.Color.White;
            this.labIcon.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.labIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(78)))));
            this.labIcon.Location = new System.Drawing.Point(3, 3);
            this.labIcon.Name = "labIcon";
            this.labIcon.Size = new System.Drawing.Size(48, 48);
            this.labIcon.TabIndex = 0;
            this.labIcon.Text = "TO";
            this.labIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labTitle
            // 
            this.labTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labTitle.AutoEllipsis = true;
            this.labTitle.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labTitle.ForeColor = System.Drawing.Color.White;
            this.labTitle.Location = new System.Drawing.Point(53, 7);
            this.labTitle.Name = "labTitle";
            this.labTitle.Size = new System.Drawing.Size(190, 17);
            this.labTitle.TabIndex = 1;
            this.labTitle.Text = "Title Of Mesh";
            this.labTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labLastMessage
            // 
            this.labLastMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labLastMessage.AutoEllipsis = true;
            this.labLastMessage.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labLastMessage.ForeColor = System.Drawing.Color.White;
            this.labLastMessage.Location = new System.Drawing.Point(54, 31);
            this.labLastMessage.Name = "labLastMessage";
            this.labLastMessage.Size = new System.Drawing.Size(205, 14);
            this.labLastMessage.TabIndex = 2;
            this.labLastMessage.Text = "typing...";
            // 
            // labUnreadMessageCount
            // 
            this.labUnreadMessageCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labUnreadMessageCount.AutoEllipsis = true;
            this.labUnreadMessageCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(71)))), ((int)(((byte)(36)))));
            this.labUnreadMessageCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labUnreadMessageCount.ForeColor = System.Drawing.Color.White;
            this.labUnreadMessageCount.Location = new System.Drawing.Point(270, 30);
            this.labUnreadMessageCount.Name = "labUnreadMessageCount";
            this.labUnreadMessageCount.Size = new System.Drawing.Size(28, 17);
            this.labUnreadMessageCount.TabIndex = 3;
            this.labUnreadMessageCount.Text = "999";
            this.labUnreadMessageCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picIcon
            // 
            this.picIcon.Location = new System.Drawing.Point(3, 3);
            this.picIcon.Name = "picIcon";
            this.picIcon.Size = new System.Drawing.Size(48, 48);
            this.picIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picIcon.TabIndex = 5;
            this.picIcon.TabStop = false;
            this.picIcon.Visible = false;
            // 
            // labLastMessageDate
            // 
            this.labLastMessageDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labLastMessageDate.AutoEllipsis = true;
            this.labLastMessageDate.AutoSize = true;
            this.labLastMessageDate.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labLastMessageDate.ForeColor = System.Drawing.Color.White;
            this.labLastMessageDate.Location = new System.Drawing.Point(236, 10);
            this.labLastMessageDate.Name = "labLastMessageDate";
            this.labLastMessageDate.Size = new System.Drawing.Size(61, 14);
            this.labLastMessageDate.TabIndex = 6;
            this.labLastMessageDate.Text = "10/31/2016";
            this.labLastMessageDate.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // timerTypingNotification
            // 
            this.timerTypingNotification.Interval = 10000;
            this.timerTypingNotification.Tick += new System.EventHandler(this.timerTypingNotification_Tick);
            // 
            // ChatListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(78)))));
            this.Controls.Add(this.labLastMessageDate);
            this.Controls.Add(this.labUnreadMessageCount);
            this.Controls.Add(this.labLastMessage);
            this.Controls.Add(this.labTitle);
            this.Controls.Add(this.labIcon);
            this.Controls.Add(this.picIcon);
            this.Name = "ChatListItem";
            this.SeparatorColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(237)))), ((int)(((byte)(238)))));
            this.Size = new System.Drawing.Size(300, 55);
            ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labIcon;
        private System.Windows.Forms.Label labTitle;
        private System.Windows.Forms.Label labLastMessage;
        private System.Windows.Forms.Label labUnreadMessageCount;
        private System.Windows.Forms.PictureBox picIcon;
        private System.Windows.Forms.Label labLastMessageDate;
        private System.Windows.Forms.Timer timerTypingNotification;
    }
}
