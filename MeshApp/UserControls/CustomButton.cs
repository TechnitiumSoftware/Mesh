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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MeshApp.UserControls
{
    public class CustomButton : PictureBox
    {
        #region variables

        Image _image;

        #endregion

        #region constructor

        public CustomButton()
            : base()
        {
            this.SizeMode = PictureBoxSizeMode.AutoSize;
        }

        #endregion

        #region protected

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _image = this.Image;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            base.Image = this.ImageHover;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.Image = _image;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.Image = this.ImageMouseDown;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            this.Image = _image;
        }

        #endregion

        #region properties

        public Image ImageHover
        { get; set; }

        public Image ImageMouseDown
        { get; set; }

        #endregion
    }
}
