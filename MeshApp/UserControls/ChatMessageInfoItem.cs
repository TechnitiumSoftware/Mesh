/*
Technitium Mesh
Copyright (C) 2019  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using MeshCore.Message;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class ChatMessageInfoItem : CustomListViewItem, IChatMessageItem
    {
        #region variables

        MessageItem _message;

        #endregion

        #region constructor

        public ChatMessageInfoItem(MessageItem message)
        {
            InitializeComponent();

            _message = message;

            if (string.IsNullOrEmpty(_message.MessageText))
            {
                label1.Text = _message.MessageDate.ToLocalTime().ToString("dddd, MMMM d, yyyy");
            }
            else
            {
                label1.Text = _message.MessageText;
                toolTip1.SetToolTip(label1, _message.MessageDate.ToLocalTime().ToString());
            }

            OnResize(EventArgs.Empty);
        }

        #endregion

        #region form code

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (label1 != null)
            {
                this.SuspendLayout();

                if (label1.Width > (this.Width - 4))
                {
                    label1.AutoSize = false;
                    label1.Left = 2;
                    label1.Width = this.Width - 4;

                    Size msgSize = TextRenderer.MeasureText(label1.Text, label1.Font, new Size(label1.Width - 3 - 3, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

                    label1.Height = msgSize.Height + 3 + 3;
                    this.Height = label1.Height + 6 + 6;
                }
                else if (label1.Width < (this.Width - 4))
                {
                    label1.AutoSize = true;
                    label1.Left = (this.Width - label1.Width) / 2;
                }

                this.ResumeLayout();
            }

            this.Refresh();
        }

        private void copyInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_message.MessageText))
                    Clipboard.SetText(label1.Text);
                else
                    Clipboard.SetText("[" + _message.MessageDate.ToString("d MMM, yyyy HH:mm:ss") + "] " + label1.Text);
            }
            catch
            { }
        }

        public void DeliveryNotification(MessageItem msg)
        {
            //do nothing
        }

        #endregion

        #region properties

        public MessageItem Message
        { get { return _message; } }

        #endregion
    }
}
