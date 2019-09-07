using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace MatchTemplate
{
    public partial class Form1 : Form
    {
        ImageBox imageBox1 = null;
        ImageBox imageBox2 = null;
        Image<Bgr, byte> observedImage = null;
        Image<Bgr, byte> modelImage = null;
        VectorOfVectorOfPoint modelContours = new VectorOfVectorOfPoint();
        VectorOfVectorOfPoint observedContours = new VectorOfVectorOfPoint();
        public Form1()
        {
            InitializeComponent();
            imageBox1 = new ImageBox();
            imageBox1.Dock = DockStyle.Fill;
            imageBox2 = new ImageBox();
            imageBox2.Dock = DockStyle.Fill;
            panel1.Controls.Add(imageBox1);
            panel2.Controls.Add(imageBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                observedImage = new Image<Bgr, byte>(fileDialog.FileNames[0]);
                imageBox1.Image = observedImage;
               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                modelImage = new Image<Bgr, byte>(fileDialog.FileNames[0]);
                imageBox2.Image = modelImage;


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // panel3.Controls.Clear();
            //var result= observedImage.MatchTemplate(modelImage, Emgu.CV.CvEnum.TemplateMatchingType.Ccoeff);
            // ImageBox imageBox = new ImageBox();
            // imageBox.Dock = DockStyle.Fill;
            // panel3.Controls.Add(imageBox);

            // double[] minValues, maxValues;
            // Point[] minLocations, maxLocations;
            // result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            // observedImage.Draw(new Rectangle(maxLocations[0], new Size(100, 100)),new Bgr(0,255,0),2);
            //// observedImage.Draw(new Rectangle(minLocations[0], new Size(100, 100)), new Bgr(0, 255, 0), 2);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(observedImage,modelImage,result, Emgu.CV.CvEnum.TemplateMatchingType.Ccoeff);
            double a=0, b=0;
            Point minPoint = new Point();
            Point maxPoint = new Point();
            CvInvoke.MinMaxLoc(result,ref a,ref b,ref minPoint,ref maxPoint);
            //using (Graphics g=Graphics.FromHwnd(this.Handle)) {
            //    g.DrawRectangle(new Pen(Color.Green,2),new Rectangle(maxPoint,modelImage.Size));
            //}C
            var showImage = observedImage.Convert<Bgra, byte>();
            CvInvoke.Rectangle(showImage, new Rectangle(maxPoint, modelImage.Size),new MCvScalar(0,255,0),2);
            imageBox1.Image = showImage;
            //    // imageBox.Image = observedImage;
            //    var grayObserved = observedImage.Convert<Gray, byte>();
            //var grayModel = modelImage.Convert<Gray, byte>();
            //CvInvoke.Threshold(grayObserved, grayObserved, 230,255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);//阈值化
            //CvInvoke.FindContours(grayObserved, observedContours, null,  Emgu.CV.CvEnum.RetrType.Ccomp,  Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //CvInvoke.Threshold(grayModel,grayModel, 230, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);//阈值化
            //imageBox1.Image = grayObserved;
            
            //CvInvoke.FindContours(grayModel, modelContours, null,  Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
          
            //imageBox2.Image = grayModel;
            //int modelCount = modelContours.Size;
            //int observedCount = observedContours.Size;
            //double min = Double.PositiveInfinity;
            //int index = -1;
            //int j = 0;
            //for (int i = 0; i < observedCount; i++)
            //{
             
            //    double tempSocre = CvInvoke.MatchShapes(modelContours[j], observedContours[i], Emgu.CV.CvEnum.ContoursMatchType.I1);
            //    if (tempSocre < min)
            //    {
            //        min = tempSocre;
            //        index = i;
            //    }
            //}

            //MessageBox.Show(min.ToString());
            //CvInvoke.DrawContours(observedImage, observedContours, index, new MCvScalar(68));
          //  imageBox1.Image = observedImage;
        }
       
        
    }
}
