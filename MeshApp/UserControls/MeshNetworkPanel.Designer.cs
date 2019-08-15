namespace MeshApp.UserControls
{
    partial class MeshNetworkPanel
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
            this.meshPanelSplitContainer = new System.Windows.Forms.SplitContainer();
            this.lstUsers = new MeshApp.UserControls.CustomListViewPanel();
            this.mnuUserList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuViewUserProfile = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.meshPanelSplitContainer)).BeginInit();
            this.meshPanelSplitContainer.Panel2.SuspendLayout();
            this.meshPanelSplitContainer.SuspendLayout();
            this.mnuUserList.SuspendLayout();
            this.SuspendLayout();
            // 
            // meshPanelSplitContainer
            // 
            this.meshPanelSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meshPanelSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.meshPanelSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.meshPanelSplitContainer.Name = "meshPanelSplitContainer";
            // 
            // meshPanelSplitContainer.Panel1
            // 
            this.meshPanelSplitContainer.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.meshPanelSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(3, 6, 2, 6);
            // 
            // meshPanelSplitContainer.Panel2
            // 
            this.meshPanelSplitContainer.Panel2.Controls.Add(this.lstUsers);
            this.meshPanelSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 6, 6, 6);
            this.meshPanelSplitContainer.Panel2MinSize = 100;
            this.meshPanelSplitContainer.Size = new System.Drawing.Size(738, 431);
            this.meshPanelSplitContainer.SplitterDistance = 498;
            this.meshPanelSplitContainer.SplitterWidth = 2;
            this.meshPanelSplitContainer.TabIndex = 12;
            this.meshPanelSplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer_SplitterMoved);
            // 
            // lstUsers
            // 
            this.lstUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(223)))));
            this.lstUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstUsers.Location = new System.Drawing.Point(3, 6);
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Padding = new System.Windows.Forms.Padding(1, 1, 2, 2);
            this.lstUsers.SeperatorSize = 0;
            this.lstUsers.Size = new System.Drawing.Size(229, 419);
            this.lstUsers.SortItems = true;
            this.lstUsers.TabIndex = 1;
            this.lstUsers.Title = "People";
            this.lstUsers.ItemMouseUp += new System.Windows.Forms.MouseEventHandler(this.lstUsers_ItemMouseUp);
            // 
            // mnuUserList
            // 
            this.mnuUserList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewUserProfile});
            this.mnuUserList.Name = "mnuUserList";
            this.mnuUserList.Size = new System.Drawing.Size(181, 48);
            // 
            // mnuViewUserProfile
            // 
            this.mnuViewUserProfile.Name = "mnuViewUserProfile";
            this.mnuViewUserProfile.Size = new System.Drawing.Size(180, 22);
            this.mnuViewUserProfile.Text = "&View Profile";
            this.mnuViewUserProfile.Click += new System.EventHandler(this.mnuViewUserProfile_Click);
            // 
            // MeshNetworkPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.Controls.Add(this.meshPanelSplitContainer);
            this.Name = "MeshNetworkPanel";
            this.Size = new System.Drawing.Size(738, 431);
            this.meshPanelSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.meshPanelSplitContainer)).EndInit();
            this.meshPanelSplitContainer.ResumeLayout(false);
            this.mnuUserList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer meshPanelSplitContainer;
        private System.Windows.Forms.ContextMenuStrip mnuUserList;
        private System.Windows.Forms.ToolStripMenuItem mnuViewUserProfile;
        private CustomListViewPanel lstUsers;
    }
}
