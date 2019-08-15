namespace MeshApp.UserControls
{
    partial class ChatMessageTextItem
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
            this.lblUsername = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopyMessage = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMessageInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.pnlBubble = new System.Windows.Forms.Panel();
            this.picDeliveryStatus = new System.Windows.Forms.PictureBox();
            this.picPointLeft = new System.Windows.Forms.PictureBox();
            this.picPointRight = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mnuForwardTo = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.pnlBubble.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDeliveryStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPointLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPointRight)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUsername
            // 
            this.lblUsername.AutoEllipsis = true;
            this.lblUsername.AutoSize = true;
            this.lblUsername.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblUsername.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(78)))));
            this.lblUsername.Location = new System.Drawing.Point(5, 3);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(66, 15);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblUsername.Click += new System.EventHandler(this.lblUsername_Click);
            this.lblUsername.MouseEnter += new System.EventHandler(this.lblUsername_MouseEnter);
            this.lblUsername.MouseLeave += new System.EventHandler(this.lblUsername_MouseLeave);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopyMessage,
            this.mnuForwardTo,
            this.mnuMessageInfo});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 92);
            // 
            // mnuCopyMessage
            // 
            this.mnuCopyMessage.Name = "mnuCopyMessage";
            this.mnuCopyMessage.Size = new System.Drawing.Size(180, 22);
            this.mnuCopyMessage.Text = "&Copy Message";
            this.mnuCopyMessage.Click += new System.EventHandler(this.mnuCopyMessage_Click);
            // 
            // mnuMessageInfo
            // 
            this.mnuMessageInfo.Name = "mnuMessageInfo";
            this.mnuMessageInfo.Size = new System.Drawing.Size(180, 22);
            this.mnuMessageInfo.Text = "Message &Info";
            this.mnuMessageInfo.Visible = false;
            this.mnuMessageInfo.Click += new System.EventHandler(this.mnuMessageInfo_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.BackColor = System.Drawing.Color.Transparent;
            this.lblMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.Location = new System.Drawing.Point(5, 21);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(390, 16);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.Text = "Test message";
            // 
            // lblDateTime
            // 
            this.lblDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDateTime.AutoEllipsis = true;
            this.lblDateTime.AutoSize = true;
            this.lblDateTime.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblDateTime.Location = new System.Drawing.Point(325, 41);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(51, 14);
            this.lblDateTime.TabIndex = 2;
            this.lblDateTime.Text = "12:00 PM";
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pnlBubble
            // 
            this.pnlBubble.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBubble.BackColor = System.Drawing.Color.White;
            this.pnlBubble.Controls.Add(this.picDeliveryStatus);
            this.pnlBubble.Controls.Add(this.lblDateTime);
            this.pnlBubble.Controls.Add(this.lblMessage);
            this.pnlBubble.Controls.Add(this.lblUsername);
            this.pnlBubble.Location = new System.Drawing.Point(20, 4);
            this.pnlBubble.Name = "pnlBubble";
            this.pnlBubble.Size = new System.Drawing.Size(400, 58);
            this.pnlBubble.TabIndex = 3;
            // 
            // picDeliveryStatus
            // 
            this.picDeliveryStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picDeliveryStatus.BackColor = System.Drawing.Color.Transparent;
            this.picDeliveryStatus.Image = global::MeshApp.Properties.Resources.waiting;
            this.picDeliveryStatus.Location = new System.Drawing.Point(380, 40);
            this.picDeliveryStatus.Name = "picDeliveryStatus";
            this.picDeliveryStatus.Size = new System.Drawing.Size(16, 16);
            this.picDeliveryStatus.TabIndex = 3;
            this.picDeliveryStatus.TabStop = false;
            this.picDeliveryStatus.Visible = false;
            // 
            // picPointLeft
            // 
            this.picPointLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picPointLeft.BackColor = System.Drawing.Color.Transparent;
            this.picPointLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picPointLeft.Image = global::MeshApp.Properties.Resources.point_left;
            this.picPointLeft.Location = new System.Drawing.Point(4, 42);
            this.picPointLeft.Name = "picPointLeft";
            this.picPointLeft.Size = new System.Drawing.Size(16, 16);
            this.picPointLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picPointLeft.TabIndex = 5;
            this.picPointLeft.TabStop = false;
            // 
            // picPointRight
            // 
            this.picPointRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picPointRight.BackColor = System.Drawing.Color.Transparent;
            this.picPointRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picPointRight.Image = global::MeshApp.Properties.Resources.point_right;
            this.picPointRight.Location = new System.Drawing.Point(580, 42);
            this.picPointRight.Name = "picPointRight";
            this.picPointRight.Size = new System.Drawing.Size(16, 16);
            this.picPointRight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picPointRight.TabIndex = 4;
            this.picPointRight.TabStop = false;
            this.picPointRight.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // mnuForwardTo
            // 
            this.mnuForwardTo.Name = "mnuForwardTo";
            this.mnuForwardTo.Size = new System.Drawing.Size(180, 22);
            this.mnuForwardTo.Text = "Forward To";
            this.mnuForwardTo.Click += new System.EventHandler(this.mnuForwardTo_Click);
            // 
            // ChatMessageTextItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.Color.Transparent;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.picPointLeft);
            this.Controls.Add(this.picPointRight);
            this.Controls.Add(this.pnlBubble);
            this.Name = "ChatMessageTextItem";
            this.Size = new System.Drawing.Size(600, 66);
            this.contextMenuStrip1.ResumeLayout(false);
            this.pnlBubble.ResumeLayout(false);
            this.pnlBubble.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDeliveryStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPointLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPointRight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyMessage;
        private System.Windows.Forms.Panel pnlBubble;
        private System.Windows.Forms.PictureBox picDeliveryStatus;
        private System.Windows.Forms.PictureBox picPointRight;
        private System.Windows.Forms.PictureBox picPointLeft;
        private System.Windows.Forms.ToolStripMenuItem mnuMessageInfo;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem mnuForwardTo;
    }
}
