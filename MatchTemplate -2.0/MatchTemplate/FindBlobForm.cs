using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System.Windows.Forms;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace MatchTemplate
{
    public partial class FindBlobForm : Form
    {
        ImageBox imageBox = new ImageBox();
        Image<Gray, byte> image = null; 
        public FindBlobForm()
        {
            InitializeComponent();
            this.panel1.Controls.Clear();
            imageBox.Dock = DockStyle.Fill;
            panel1.Controls.Add(imageBox);

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                image = new Image<Gray, byte>(fileDialog.FileNames[0]);
                imageBox.Image = image;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CvInvoke.Threshold(image,image,100,255, Emgu.CV.CvEnum.ThresholdType.Binary);
            Mat b = new Mat();
            VectorOfVectorOfPointF contours = new VectorOfVectorOfPointF();
            Image<Gray, byte> cannyImage = new Image<Gray, byte>(image.Width,image.Height);
           // CvInvoke.Canny(image,cannyImage,100,3);
            //CvInvoke.FindContours(cannyImage,contours,b,Emgu.CV.CvEnum.RetrType.Ccomp,Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.FindContours(cannyImage,contours,null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for (int i=0;i<contours.Size;i++) {
                CvInvoke.DrawContours(image,contours,i,new MCvScalar(130));
            }
            imageBox.Image = image;
        }
    }
}
