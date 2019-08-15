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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace MeshApp.UserControls
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class CustomPanel : UserControl
    {
        #region variables

        const int BORDER_SIZE = 1;

        Color BorderColor = Color.FromArgb(224, 224, 223);
        Color BorderColorShadow = Color.FromArgb(199, 199, 198);

        #endregion

        #region constructor

        public CustomPanel()
        {
            InitializeComponent();
        }

        #endregion

        #region private

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         BorderColor, 0, ButtonBorderStyle.Solid,
                                         BorderColor, 0, ButtonBorderStyle.Solid,
                                         BorderColorShadow, BORDER_SIZE * 2, ButtonBorderStyle.Solid,
                                         BorderColorShadow, BORDER_SIZE * 2, ButtonBorderStyle.Solid);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         BorderColor, BORDER_SIZE, ButtonBorderStyle.Solid,
                                         BorderColor, BORDER_SIZE, ButtonBorderStyle.Solid,
                                         BorderColor, BORDER_SIZE, ButtonBorderStyle.Solid,
                                         BorderColor, BORDER_SIZE, ButtonBorderStyle.Solid);
        }

        protected override void OnResize(EventArgs e)
        {
            if (panel1.Height == 0)
                panel2.Size = new Size(this.Width - 1 - 2, this.Height - 1 - 2);
            else
                panel2.Size = new Size(this.Width - 1 - 2, this.Height - panel1.Height - 1 - 1 - 2);

            base.OnResize(e);

            this.Refresh();
        }

        #endregion

        #region properties

        public string Title
        {
            get
            { return label1.Text; }
            set
            {
                label1.Text = value;

                if (string.IsNullOrEmpty(value))
                {
                    panel1.Height = 0;
                    panel2.Location = new Point(1, 1);
                }
                else
                {
                    panel1.Height = 25;
                    panel2.Location = new Point(1, 1 + 25 + 1);
                }
            }
        }

        #endregion
    }
}
