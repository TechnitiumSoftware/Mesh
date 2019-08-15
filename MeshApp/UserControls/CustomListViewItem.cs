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
using System.Drawing;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public partial class CustomListViewItem : UserControl
    {
        #region event

        public event EventHandler SortList;

        #endregion

        #region variables

        const int BORDER_SIZE = 1;

        bool _selected = false;

        #endregion

        #region constructor

        public CustomListViewItem()
        {
            InitializeComponent();
        }

        #endregion

        #region private

        private void CustomListViewItem_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.Click += ctrl_Click;
                ctrl.DoubleClick += ctrl_DoubleClick;
                ctrl.MouseUp += ctrl_MouseUp;
                ctrl.MouseLeave += ctrl_MouseLeave;
                ctrl.MouseEnter += ctrl_MouseEnter;
                ctrl.KeyDown += ctrl_KeyDown;
                ctrl.KeyUp += ctrl_KeyUp;
                ctrl.KeyPress += ctrl_KeyPress;
            }
        }

        private void ctrl_Click(object sender, EventArgs e)
        {
            OnClick(EventArgs.Empty);
        }

        private void ctrl_DoubleClick(object sender, EventArgs e)
        {
            OnDoubleClick(EventArgs.Empty);
        }

        private void ctrl_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender != this)
            {
                Control obj = sender as Control;
                e = new MouseEventArgs(e.Button, e.Clicks, obj.Location.X + e.X, obj.Location.Y + e.Y, e.Delta);
            }

            OnMouseUp(e);
        }

        private void ctrl_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        private void ctrl_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        private void ctrl_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void ctrl_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void ctrl_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        #endregion

        #region protected

        protected virtual void OnSelected()
        { }

        protected virtual void OnMouseOver(bool hovering)
        { }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         SeparatorColor, 0, ButtonBorderStyle.Solid,
                                         SeparatorColor, 0, ButtonBorderStyle.Solid,
                                         SeparatorColor, 0, ButtonBorderStyle.Solid,
                                         SeparatorColor, BORDER_SIZE, ButtonBorderStyle.Solid);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
            {
                base.OnMouseEnter(e);
                OnMouseOver(true);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
            {
                base.OnMouseLeave(e);
                OnMouseOver(false);
            }
        }

        protected void SortListView()
        {
            if (SortList != null)
                SortList(this, EventArgs.Empty);
        }

        public virtual bool AllowTriming()
        {
            return true;
        }

        #endregion

        #region properties

        public Color SeparatorColor
        { set; get; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;

                    this.SuspendLayout();

                    OnSelected();

                    this.ResumeLayout();
                }
            }
        }

        #endregion
    }
}
