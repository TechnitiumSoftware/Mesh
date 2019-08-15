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
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace MeshApp
{
    public partial class frmImageDialog : Form
    {
        #region variables

        Pen cropPen = new Pen(Color.White, 2) { DashStyle = DashStyle.Dash };
        Rectangle cropRectangle;

        Image selectedImage;

        #endregion

        #region constructor

        public frmImageDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region form code

        private void frmImageDialog_Load(object sender, EventArgs e)
        {
            btnBrowse_Click(null, null);
        }

        private static Image CropImage(PictureBox box, Rectangle cropRectangle)
        {
            //find virtual box size and fix crop rectangle X & Y
            double imageRatio = box.Image.Width / (double)box.Image.Height;
            double boxRatio = box.Width / (double)box.Height;

            int vBoxWidth;
            int vBoxHeight;
            Rectangle vCropRectangle = cropRectangle;

            if (imageRatio > boxRatio)
            {
                //box height is more
                vBoxWidth = box.Width;
                vBoxHeight = (int)(box.Width / imageRatio);

                int padding = (box.Height - vBoxHeight) / 2;
                vCropRectangle.Y -= padding;
            }
            else
            {
                //box width is more
                vBoxWidth = (int)(box.Height * imageRatio);
                vBoxHeight = box.Height;

                int padding = (box.Width - vBoxWidth) / 2;
                vCropRectangle.X -= padding;
            }

            //scale crop rectangle
            double widthRatio = box.Image.Width / (double)vBoxWidth;
            double heightRatio = box.Image.Height / (double)vBoxHeight;

            vCropRectangle.X = (int)(vCropRectangle.X * widthRatio);
            vCropRectangle.Y = (int)(vCropRectangle.Y * heightRatio);
            vCropRectangle.Width = (int)(vCropRectangle.Width * widthRatio);
            vCropRectangle.Height = (int)(vCropRectangle.Height * heightRatio);

            return CropImage(box.Image, vCropRectangle);
        }

        private static Image CropImage(Image original, Rectangle cropRectangle)
        {
            Bitmap image = new Bitmap(cropRectangle.Width, cropRectangle.Height);
            image.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            Graphics g = Graphics.FromImage(image);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.DrawImage(original, 0, 0, cropRectangle, GraphicsUnit.Pixel);

            return image;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog oFD = new OpenFileDialog())
            {
                if (oFD.ShowDialog(this) == DialogResult.OK)
                {
                    oFD.CheckFileExists = true;
                    oFD.CheckPathExists = true;
                    oFD.Multiselect = false;
                    oFD.Title = "Select Image";

                    try
                    {
                        using (FileStream fS = new FileStream(oFD.FileName, FileMode.Open, FileAccess.Read))
                        {
                            picImage.Image = Image.FromStream(fS);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error occured while opening the selected file:\n\n" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void picImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                cropRectangle.X = e.X;
                cropRectangle.Y = e.Y;
                cropRectangle.Width = 0;
                cropRectangle.Height = 0;

                picImage.Refresh();
            }
        }

        private void picImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (picImage.Image != null)
                {
                    picImage.Refresh();

                    cropRectangle.Width = e.X - cropRectangle.X;
                    cropRectangle.Height = e.Y - cropRectangle.Y;

                    if (cropRectangle.Width > cropRectangle.Height)
                        cropRectangle.Height = cropRectangle.Width;
                    else
                        cropRectangle.Width = cropRectangle.Height;

                    picImage.CreateGraphics().DrawRectangle(cropPen, cropRectangle);
                }
            }
        }

        private void picImage_MouseUp(object sender, MouseEventArgs e)
        {
            btnOK.Enabled = (cropRectangle.Width > 0);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            using (Image croppedImage = CropImage(picImage, cropRectangle))
            {
                selectedImage = new Bitmap(croppedImage, 256, 256);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region properties

        public Image SelectedImage
        { get { return selectedImage; } }

        #endregion
    }
}
