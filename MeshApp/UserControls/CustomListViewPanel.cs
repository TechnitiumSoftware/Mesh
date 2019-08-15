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

using System;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class CustomListViewPanel : CustomPanel
    {
        #region events

        public event EventHandler ItemClick;
        public event EventHandler ItemDoubleClick;
        public event MouseEventHandler ItemMouseUp;
        public event KeyEventHandler ItemKeyDown;
        public event KeyEventHandler ItemKeyUp;
        public event KeyPressEventHandler ItemKeyPress;

        #endregion

        #region constructor

        public CustomListViewPanel()
        {
            InitializeComponent();
        }

        #endregion

        #region public

        public CustomListViewItem AddItem(CustomListViewItem item)
        {
            return customListView1.AddItem(item);
        }

        public bool RemoveItem(CustomListViewItem item)
        {
            return customListView1.RemoveItem(item);
        }

        public void RemoveAllItems()
        {
            customListView1.RemoveAllItems();
        }

        #endregion

        #region private

        private void customListView1_ItemClick(object sender, EventArgs e)
        {
            if (ItemClick != null)
                ItemClick(sender, e);
        }

        private void customListView1_ItemDoubleClick(object sender, EventArgs e)
        {
            if (ItemDoubleClick != null)
                ItemDoubleClick(sender, e);
        }

        private void customListView1_ItemKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemKeyDown != null)
                ItemKeyDown(sender, e);
        }

        private void customListView1_ItemKeyPress(object sender, KeyPressEventArgs e)
        {
            if (ItemKeyPress != null)
                ItemKeyPress(sender, e);
        }

        private void customListView1_ItemKeyUp(object sender, KeyEventArgs e)
        {
            if (ItemKeyUp != null)
                ItemKeyUp(sender, e);
        }

        private void customListView1_ItemMouseUp(object sender, MouseEventArgs e)
        {
            if (ItemMouseUp != null)
                ItemMouseUp(sender, e);
        }

        #endregion

        #region properties

        public CustomListViewItem SelectedItem
        { get { return customListView1.SelectedItem; } }

        public int SeperatorSize
        {
            get { return customListView1.Padding.Top; }
            set { customListView1.Padding = new Padding(value, value, value, 0); }
        }

        public ControlCollection Items
        { get { return customListView1.Controls; } }

        public bool SortItems
        {
            get { return customListView1.SortItems; }
            set { customListView1.SortItems = value; }
        }

        #endregion
    }
}
