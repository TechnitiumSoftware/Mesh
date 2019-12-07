using MeshApp.UserControls;
namespace MeshApp
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.mnuChat = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuMuteNotifications = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLockGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGoOffline = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDeleteChat = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewPeerProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGroupPhoto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddPrivateChat2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddGroupChat2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnPlusButton = new MeshApp.UserControls.CustomButton();
            this.lblProfileDisplayName = new System.Windows.Forms.Label();
            this.lstChats = new MeshApp.UserControls.CustomListView();
            this.panelGetStarted = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCreateChat = new MeshApp.UserControls.CustomButton();
            this.mnuAddGroupChat1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuProfileSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlus = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAddPrivateChat1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMyProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNetworkInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCheckUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAboutMesh = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuProfileManager = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCloseWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlusButton)).BeginInit();
            this.panelGetStarted.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateChat)).BeginInit();
            this.mnuPlus.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuChat
            // 
            this.mnuChat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMuteNotifications,
            this.mnuLockGroup,
            this.mnuGoOffline,
            this.mnuDeleteChat,
            this.toolStripSeparator6,
            this.mnuViewPeerProfile,
            this.mnuGroupPhoto,
            this.mnuProperties,
            this.toolStripSeparator1,
            this.mnuAddPrivateChat2,
            this.mnuAddGroupChat2});
            this.mnuChat.Name = "chatContextMenu";
            this.mnuChat.Size = new System.Drawing.Size(164, 214);
            // 
            // mnuMuteNotifications
            // 
            this.mnuMuteNotifications.Name = "mnuMuteNotifications";
            this.mnuMuteNotifications.Size = new System.Drawing.Size(163, 22);
            this.mnuMuteNotifications.Text = "&Mute";
            this.mnuMuteNotifications.Click += new System.EventHandler(this.mnuMuteNotifications_Click);
            // 
            // mnuLockGroup
            // 
            this.mnuLockGroup.Name = "mnuLockGroup";
            this.mnuLockGroup.Size = new System.Drawing.Size(163, 22);
            this.mnuLockGroup.Text = "Lock Group";
            this.mnuLockGroup.Click += new System.EventHandler(this.mnuLockGroup_Click);
            // 
            // mnuGoOffline
            // 
            this.mnuGoOffline.Name = "mnuGoOffline";
            this.mnuGoOffline.Size = new System.Drawing.Size(163, 22);
            this.mnuGoOffline.Text = "Go &Offline";
            this.mnuGoOffline.Click += new System.EventHandler(this.mnuGoOffline_Click);
            // 
            // mnuDeleteChat
            // 
            this.mnuDeleteChat.Name = "mnuDeleteChat";
            this.mnuDeleteChat.Size = new System.Drawing.Size(163, 22);
            this.mnuDeleteChat.Text = "&Delete Chat";
            this.mnuDeleteChat.Click += new System.EventHandler(this.mnuDeleteChat_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(160, 6);
            // 
            // mnuViewPeerProfile
            // 
            this.mnuViewPeerProfile.Name = "mnuViewPeerProfile";
            this.mnuViewPeerProfile.Size = new System.Drawing.Size(163, 22);
            this.mnuViewPeerProfile.Text = "&View Profile";
            this.mnuViewPeerProfile.Click += new System.EventHandler(this.mnuViewPeerProfile_Click);
            // 
            // mnuGroupPhoto
            // 
            this.mnuGroupPhoto.Name = "mnuGroupPhoto";
            this.mnuGroupPhoto.Size = new System.Drawing.Size(163, 22);
            this.mnuGroupPhoto.Text = "Group &Photo";
            this.mnuGroupPhoto.Visible = false;
            this.mnuGroupPhoto.Click += new System.EventHandler(this.mnuGroupPhoto_Click);
            // 
            // mnuProperties
            // 
            this.mnuProperties.Name = "mnuProperties";
            this.mnuProperties.Size = new System.Drawing.Size(163, 22);
            this.mnuProperties.Text = "P&roperties";
            this.mnuProperties.Click += new System.EventHandler(this.mnuProperties_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(160, 6);
            // 
            // mnuAddPrivateChat2
            // 
            this.mnuAddPrivateChat2.Name = "mnuAddPrivateChat2";
            this.mnuAddPrivateChat2.Size = new System.Drawing.Size(163, 22);
            this.mnuAddPrivateChat2.Text = "Add &Private Chat";
            this.mnuAddPrivateChat2.Click += new System.EventHandler(this.mnuAddPrivateChat_Click);
            // 
            // mnuAddGroupChat2
            // 
            this.mnuAddGroupChat2.Name = "mnuAddGroupChat2";
            this.mnuAddGroupChat2.Size = new System.Drawing.Size(163, 22);
            this.mnuAddGroupChat2.Text = "Add &Group Chat";
            this.mnuAddGroupChat2.Click += new System.EventHandler(this.mnuAddGroupChat_Click);
            // 
            // mainContainer
            // 
            this.mainContainer.CausesValidation = false;
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainContainer.Location = new System.Drawing.Point(0, 0);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.mainContainer.Panel1.Controls.Add(this.panel1);
            this.mainContainer.Panel1.Controls.Add(this.lstChats);
            this.mainContainer.Panel1MinSize = 200;
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.mainContainer.Panel2.Controls.Add(this.panelGetStarted);
            this.mainContainer.Panel2.Resize += new System.EventHandler(this.mainContainer_Panel2_Resize);
            this.mainContainer.Panel2MinSize = 200;
            this.mainContainer.Size = new System.Drawing.Size(944, 501);
            this.mainContainer.SplitterDistance = 277;
            this.mainContainer.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(78)))));
            this.panel1.Controls.Add(this.btnPlusButton);
            this.panel1.Controls.Add(this.lblProfileDisplayName);
            this.panel1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 36);
            this.panel1.TabIndex = 14;
            this.panel1.Click += new System.EventHandler(this.mnuMyProfile_Click);
            this.panel1.MouseEnter += new System.EventHandler(this.lblProfileDisplayName_MouseEnter);
            this.panel1.MouseLeave += new System.EventHandler(this.lblProfileDisplayName_MouseLeave);
            // 
            // btnPlusButton
            // 
            this.btnPlusButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlusButton.Image = ((System.Drawing.Image)(resources.GetObject("btnPlusButton.Image")));
            this.btnPlusButton.ImageHover = ((System.Drawing.Image)(resources.GetObject("btnPlusButton.ImageHover")));
            this.btnPlusButton.ImageMouseDown = ((System.Drawing.Image)(resources.GetObject("btnPlusButton.ImageMouseDown")));
            this.btnPlusButton.Location = new System.Drawing.Point(250, 7);
            this.btnPlusButton.Name = "btnPlusButton";
            this.btnPlusButton.Size = new System.Drawing.Size(24, 24);
            this.btnPlusButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnPlusButton.TabIndex = 14;
            this.btnPlusButton.TabStop = false;
            this.btnPlusButton.Click += new System.EventHandler(this.btnPlusButton_Click);
            // 
            // lblProfileDisplayName
            // 
            this.lblProfileDisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProfileDisplayName.AutoEllipsis = true;
            this.lblProfileDisplayName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(78)))));
            this.lblProfileDisplayName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblProfileDisplayName.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProfileDisplayName.ForeColor = System.Drawing.Color.White;
            this.lblProfileDisplayName.Location = new System.Drawing.Point(24, 1);
            this.lblProfileDisplayName.Name = "lblProfileDisplayName";
            this.lblProfileDisplayName.Size = new System.Drawing.Size(226, 34);
            this.lblProfileDisplayName.TabIndex = 11;
            this.lblProfileDisplayName.Text = "Username";
            this.lblProfileDisplayName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblProfileDisplayName.Click += new System.EventHandler(this.mnuMyProfile_Click);
            this.lblProfileDisplayName.MouseEnter += new System.EventHandler(this.lblProfileDisplayName_MouseEnter);
            this.lblProfileDisplayName.MouseLeave += new System.EventHandler(this.lblProfileDisplayName_MouseLeave);
            // 
            // lstChats
            // 
            this.lstChats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstChats.AutoScroll = true;
            this.lstChats.AutoScrollToBottom = false;
            this.lstChats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.lstChats.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.lstChats.Location = new System.Drawing.Point(0, 36);
            this.lstChats.Name = "lstChats";
            this.lstChats.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.lstChats.SeparatorColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(53)))), ((int)(((byte)(65)))));
            this.lstChats.Size = new System.Drawing.Size(277, 462);
            this.lstChats.SortItems = true;
            this.lstChats.TabIndex = 13;
            this.lstChats.ItemClick += new System.EventHandler(this.lstChats_ItemClick);
            this.lstChats.ItemMouseUp += new System.Windows.Forms.MouseEventHandler(this.lstChats_ItemMouseUp);
            this.lstChats.ItemKeyUp += new System.Windows.Forms.KeyEventHandler(this.lstChats_ItemKeyUp);
            this.lstChats.DoubleClick += new System.EventHandler(this.lstChats_DoubleClick);
            this.lstChats.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstChats_KeyUp);
            this.lstChats.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lstChats_MouseUp);
            // 
            // panelGetStarted
            // 
            this.panelGetStarted.Controls.Add(this.label2);
            this.panelGetStarted.Controls.Add(this.label4);
            this.panelGetStarted.Controls.Add(this.btnCreateChat);
            this.panelGetStarted.Location = new System.Drawing.Point(102, 126);
            this.panelGetStarted.Name = "panelGetStarted";
            this.panelGetStarted.Size = new System.Drawing.Size(488, 197);
            this.panelGetStarted.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(360, 75);
            this.label2.TabIndex = 19;
            this.label2.Text = "get started";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(57)))), ((int)(((byte)(69)))));
            this.label4.Location = new System.Drawing.Point(213, 75);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(275, 36);
            this.label4.TabIndex = 20;
            this.label4.Text = "create a chat now!";
            // 
            // btnCreateChat
            // 
            this.btnCreateChat.BackColor = System.Drawing.Color.Transparent;
            this.btnCreateChat.Image = ((System.Drawing.Image)(resources.GetObject("btnCreateChat.Image")));
            this.btnCreateChat.ImageHover = ((System.Drawing.Image)(resources.GetObject("btnCreateChat.ImageHover")));
            this.btnCreateChat.ImageMouseDown = ((System.Drawing.Image)(resources.GetObject("btnCreateChat.ImageMouseDown")));
            this.btnCreateChat.Location = new System.Drawing.Point(178, 139);
            this.btnCreateChat.Name = "btnCreateChat";
            this.btnCreateChat.Size = new System.Drawing.Size(134, 44);
            this.btnCreateChat.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.btnCreateChat.TabIndex = 2;
            this.btnCreateChat.TabStop = false;
            this.btnCreateChat.Click += new System.EventHandler(this.mnuAddGroupChat_Click);
            // 
            // mnuAddGroupChat1
            // 
            this.mnuAddGroupChat1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mnuAddGroupChat1.Name = "mnuAddGroupChat1";
            this.mnuAddGroupChat1.Size = new System.Drawing.Size(180, 22);
            this.mnuAddGroupChat1.Text = "Add &Group Chat";
            this.mnuAddGroupChat1.Click += new System.EventHandler(this.mnuAddGroupChat_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
            // 
            // mnuProfileSettings
            // 
            this.mnuProfileSettings.Name = "mnuProfileSettings";
            this.mnuProfileSettings.Size = new System.Drawing.Size(180, 22);
            this.mnuProfileSettings.Text = "Profile &Settings";
            this.mnuProfileSettings.Click += new System.EventHandler(this.mnuProfileSettings_Click);
            // 
            // mnuPlus
            // 
            this.mnuPlus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAddPrivateChat1,
            this.mnuAddGroupChat1,
            this.toolStripSeparator4,
            this.mnuMyProfile,
            this.mnuProfileSettings,
            this.mnuNetworkInfo,
            this.toolStripSeparator7,
            this.mnuCheckUpdate,
            this.mnuAboutMesh,
            this.toolStripSeparator5,
            this.mnuProfileManager,
            this.mnuExit,
            this.toolStripSeparator3,
            this.mnuCloseWindow,
            this.mnuLogout});
            this.mnuPlus.Name = "addChatContextMenu";
            this.mnuPlus.Size = new System.Drawing.Size(181, 292);
            // 
            // mnuAddPrivateChat1
            // 
            this.mnuAddPrivateChat1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.mnuAddPrivateChat1.Name = "mnuAddPrivateChat1";
            this.mnuAddPrivateChat1.Size = new System.Drawing.Size(180, 22);
            this.mnuAddPrivateChat1.Text = "Add &Private Chat";
            this.mnuAddPrivateChat1.Click += new System.EventHandler(this.mnuAddPrivateChat_Click);
            // 
            // mnuMyProfile
            // 
            this.mnuMyProfile.Name = "mnuMyProfile";
            this.mnuMyProfile.Size = new System.Drawing.Size(180, 22);
            this.mnuMyProfile.Text = "&My Profile";
            this.mnuMyProfile.Click += new System.EventHandler(this.mnuMyProfile_Click);
            // 
            // mnuNetworkInfo
            // 
            this.mnuNetworkInfo.Name = "mnuNetworkInfo";
            this.mnuNetworkInfo.Size = new System.Drawing.Size(180, 22);
            this.mnuNetworkInfo.Text = "Network &Info";
            this.mnuNetworkInfo.Click += new System.EventHandler(this.mnuNetworkInfo_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(177, 6);
            // 
            // mnuCheckUpdate
            // 
            this.mnuCheckUpdate.Name = "mnuCheckUpdate";
            this.mnuCheckUpdate.Size = new System.Drawing.Size(180, 22);
            this.mnuCheckUpdate.Text = "Check For &Update";
            this.mnuCheckUpdate.Click += new System.EventHandler(this.mnuCheckUpdate_Click);
            // 
            // mnuAboutMesh
            // 
            this.mnuAboutMesh.Image = global::MeshApp.Properties.Resources.logo2;
            this.mnuAboutMesh.Name = "mnuAboutMesh";
            this.mnuAboutMesh.Size = new System.Drawing.Size(180, 22);
            this.mnuAboutMesh.Text = "&About Mesh";
            this.mnuAboutMesh.Click += new System.EventHandler(this.mnuAboutMesh_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
            // 
            // mnuProfileManager
            // 
            this.mnuProfileManager.Name = "mnuProfileManager";
            this.mnuProfileManager.Size = new System.Drawing.Size(180, 22);
            this.mnuProfileManager.Text = "P&rofile Manager";
            this.mnuProfileManager.Click += new System.EventHandler(this.mnuProfileManager_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(180, 22);
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // mnuCloseWindow
            // 
            this.mnuCloseWindow.Name = "mnuCloseWindow";
            this.mnuCloseWindow.Size = new System.Drawing.Size(180, 22);
            this.mnuCloseWindow.Text = "&Close Window";
            this.mnuCloseWindow.Click += new System.EventHandler(this.mnuCloseWindow_Click);
            // 
            // mnuLogout
            // 
            this.mnuLogout.Name = "mnuLogout";
            this.mnuLogout.Size = new System.Drawing.Size(180, 22);
            this.mnuLogout.Text = "&Logout";
            this.mnuLogout.Click += new System.EventHandler(this.mnuLogout_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.ClientSize = new System.Drawing.Size(944, 501);
            this.Controls.Add(this.mainContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mesh";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.mnuChat.ResumeLayout(false);
            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
            this.mainContainer.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnPlusButton)).EndInit();
            this.panelGetStarted.ResumeLayout(false);
            this.panelGetStarted.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateChat)).EndInit();
            this.mnuPlus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip mnuChat;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuDeleteChat;
        private System.Windows.Forms.ToolStripMenuItem mnuProperties;
        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.Label lblProfileDisplayName;
        private MeshApp.UserControls.CustomListView lstChats;
        private MeshApp.UserControls.CustomButton btnCreateChat;
        private System.Windows.Forms.Panel panelGetStarted;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private CustomButton btnPlusButton;
        private System.Windows.Forms.ToolStripMenuItem mnuAddGroupChat2;
        private System.Windows.Forms.ToolStripMenuItem mnuAddGroupChat1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem mnuProfileSettings;
        private System.Windows.Forms.ContextMenuStrip mnuPlus;
        private System.Windows.Forms.ToolStripMenuItem mnuAddPrivateChat2;
        private System.Windows.Forms.ToolStripMenuItem mnuAddPrivateChat1;
        private System.Windows.Forms.ToolStripMenuItem mnuNetworkInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuGoOffline;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuMuteNotifications;
        private System.Windows.Forms.ToolStripMenuItem mnuGroupPhoto;
        private System.Windows.Forms.ToolStripMenuItem mnuMyProfile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem mnuViewPeerProfile;
        private System.Windows.Forms.ToolStripMenuItem mnuCloseWindow;
        private System.Windows.Forms.ToolStripMenuItem mnuLogout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuProfileManager;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuAboutMesh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuLockGroup;
        private System.Windows.Forms.ToolStripMenuItem mnuCheckUpdate;
    }
}

