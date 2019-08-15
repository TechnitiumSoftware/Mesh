namespace MeshApp.UserControls
{
    partial class ChatMessageView
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.labTypingNotification = new System.Windows.Forms.Label();
            this.customListView1 = new MeshApp.UserControls.CustomListView();
            this.btnShareFile = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.timerTypingNotification = new System.Windows.Forms.Timer(this.components);
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(224)))));
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Size = new System.Drawing.Size(697, 371);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.splitContainer1.Panel1.Controls.Add(this.labTypingNotification);
            this.splitContainer1.Panel1.Controls.Add(this.customListView1);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.splitContainer1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainer1.Panel2.Controls.Add(this.btnShareFile);
            this.splitContainer1.Panel2.Controls.Add(this.btnSend);
            this.splitContainer1.Panel2MinSize = 52;
            this.splitContainer1.Size = new System.Drawing.Size(697, 371);
            this.splitContainer1.SplitterDistance = 317;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 3;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // labTypingNotification
            // 
            this.labTypingNotification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labTypingNotification.AutoEllipsis = true;
            this.labTypingNotification.BackColor = System.Drawing.Color.Transparent;
            this.labTypingNotification.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labTypingNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labTypingNotification.Location = new System.Drawing.Point(0, 302);
            this.labTypingNotification.Name = "labTypingNotification";
            this.labTypingNotification.Size = new System.Drawing.Size(697, 15);
            this.labTypingNotification.TabIndex = 1;
            // 
            // customListView1
            // 
            this.customListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customListView1.AutoScroll = true;
            this.customListView1.AutoScrollToBottom = false;
            this.customListView1.BackColor = System.Drawing.Color.Transparent;
            this.customListView1.BorderColor = System.Drawing.Color.Empty;
            this.customListView1.Location = new System.Drawing.Point(0, 0);
            this.customListView1.Name = "customListView1";
            this.customListView1.SeparatorColor = System.Drawing.Color.Empty;
            this.customListView1.Size = new System.Drawing.Size(697, 302);
            this.customListView1.SortItems = false;
            this.customListView1.TabIndex = 0;
            this.customListView1.ScrolledNearStart += new System.EventHandler(this.customListView1_ScrolledNearStart);
            // 
            // btnShareFile
            // 
            this.btnShareFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShareFile.BackColor = System.Drawing.Color.Transparent;
            this.btnShareFile.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShareFile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnShareFile.Image = global::MeshApp.Properties.Resources.Attachment;
            this.btnShareFile.Location = new System.Drawing.Point(635, 26);
            this.btnShareFile.Name = "btnShareFile";
            this.btnShareFile.Size = new System.Drawing.Size(60, 24);
            this.btnShareFile.TabIndex = 2;
            this.btnShareFile.Text = "Share";
            this.btnShareFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnShareFile.UseVisualStyleBackColor = false;
            this.btnShareFile.Click += new System.EventHandler(this.btnShareFile_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.BackColor = System.Drawing.Color.Transparent;
            this.btnSend.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSend.Image = global::MeshApp.Properties.Resources.send_message;
            this.btnSend.Location = new System.Drawing.Point(635, 2);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(60, 24);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.HideSelection = false;
            this.txtMessage.Location = new System.Drawing.Point(1, 1);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(607, 64);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMessage_KeyDown);
            this.txtMessage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMessage_KeyPress);
            // 
            // timerTypingNotification
            // 
            this.timerTypingNotification.Interval = 10000;
            this.timerTypingNotification.Tick += new System.EventHandler(this.timerTypingNotification_Tick);
            // 
            // ChatMessageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "ChatMessageView";
            this.Size = new System.Drawing.Size(700, 400);
            this.Title = "Chat Title";
            this.panel2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnShareFile;
        private CustomListView customListView1;
        private System.Windows.Forms.Label labTypingNotification;
        private System.Windows.Forms.Timer timerTypingNotification;
    }
}
