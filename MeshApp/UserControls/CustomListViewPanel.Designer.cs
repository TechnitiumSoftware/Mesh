namespace MeshApp.UserControls
{
    partial class CustomListViewPanel
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
            this.customListView1 = new MeshApp.UserControls.CustomListView();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.customListView1);
            // 
            // customListView1
            // 
            this.customListView1.AutoScroll = true;
            this.customListView1.AutoScrollToBottom = false;
            this.customListView1.BackColor = System.Drawing.Color.White;
            this.customListView1.BorderColor = System.Drawing.Color.Empty;
            this.customListView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customListView1.Location = new System.Drawing.Point(0, 0);
            this.customListView1.Name = "customListView1";
            this.customListView1.SeparatorColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(223)))));
            this.customListView1.Size = new System.Drawing.Size(297, 121);
            this.customListView1.TabIndex = 4;
            this.customListView1.ItemClick += new System.EventHandler(this.customListView1_ItemClick);
            this.customListView1.ItemDoubleClick += new System.EventHandler(this.customListView1_ItemDoubleClick);
            this.customListView1.ItemMouseUp += new System.Windows.Forms.MouseEventHandler(this.customListView1_ItemMouseUp);
            this.customListView1.ItemKeyDown += new System.Windows.Forms.KeyEventHandler(this.customListView1_ItemKeyDown);
            this.customListView1.ItemKeyUp += new System.Windows.Forms.KeyEventHandler(this.customListView1_ItemKeyUp);
            this.customListView1.ItemKeyPress += new System.Windows.Forms.KeyPressEventHandler(this.customListView1_ItemKeyPress);
            // 
            // CustomListView2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "CustomListView2";
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CustomListView customListView1;

    }
}
