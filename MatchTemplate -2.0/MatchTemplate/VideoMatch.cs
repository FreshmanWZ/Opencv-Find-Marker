using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class VideoMatch : Form
    {
        ImageBox imageBox;
        Image<Bgra, byte> bgraImage;
        Image<Gray, byte> modelImage;
        public VideoMatch()
        {
            InitializeComponent();
            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            this.panel2.Controls.Add(imageBox);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                modelImage = new Image<Gray, byte>(fileDialog.FileNames[0]);
                MessageBox.Show("Model loaded!");
            }
        }
        Capture capture;
        Thread match;
        private void button2_Click(object sender, EventArgs e)
        {
            if (capture != null)
            {
                return;
            }
            capture = new Emgu.CV.Capture(1);
            // match = new Thread(matchLive);
            match = new Thread(myMatch);
            match.Start();
        }
        private static double score(double x)
        {

            return -x * x + 2 * x + 2;
        }
        private static double myScore(Image<Gray,byte> sourceImage,Image<Gray,byte> tempImage,double angle) {
            var temp = sourceImage.Rotate(angle,new Gray(0),true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(temp,tempImage,result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed,null);
            
            double min = 0, max = 0;
            Point loc = new Point();
            Point LOC = new Point();
            CvInvoke.MinMaxLoc(result,ref min,ref max,ref loc,ref LOC,null);
            return max;
        }
        public static double approch(double left, double right, double acu)
        {
            if (Math.Abs(right - left) < acu)
            {
                return score(left) > score(right) ? left : right;
            }
            double leftvalue = score(left);
            double rightvalue = score(right);
            if (leftvalue > rightvalue)
            {//如果左边要高则
                right = (left + right) / 2;
                return approch(left, right, acu);
            }
            else if (leftvalue == rightvalue)
            {//如果左右相等
                right = (left + right) / 2;
                return approch(left, right, acu);
            }
            else
            {//如果右边要高
                left = (right + left) / 2;
                return approch(left, right, acu);
            }
        }
        public static double MyApproch(Image<Gray, byte> sourceImage, Image<Gray, byte> tempImage,double left, double right, double acu)
        {
            if (Math.Abs(right - left) < acu)
            {
                return myScore(sourceImage,tempImage,left) > myScore(sourceImage,tempImage,right) ? left : right;
            }
            double leftvalue = myScore(sourceImage, tempImage, left);
            double rightvalue = myScore(sourceImage, tempImage, right);
            if (leftvalue > rightvalue)
            {//如果左边要高则
                right = (left + right) / 2;
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }
            else if (leftvalue == rightvalue)
            {//如果左右相等
                right = (left + right) / 2;
                return  MyApproch(sourceImage, tempImage, left, right, acu);
            }
            else
            {//如果右边要高
                left = (right + left) / 2;
                return  MyApproch(sourceImage, tempImage, left, right, acu);
            }
        }
        private void myMatch() {
            while (true)
            {
                bool isGoodMatch = false;
                Image<Gray, byte>[] images = new Image<Gray, byte>[4];
                Image<Gray, byte>[] templateImages = new Image<Gray, byte>[4];
                Image<Gray, byte>[] rotateObservedImages = new Image<Gray, byte>[Convert.ToInt32(textBox1.Text) + 1];
                int step = Convert.ToInt32(textBox1.Text);
                double maxAngle = Convert.ToDouble(textBox3.Text);
                double minAngle = Convert.ToDouble(textBox2.Text);
                double intervalAngle = (maxAngle - minAngle) / step;
                var capturedImage = new Image<Bgra, byte>(capture.QueryFrame().Bitmap);
                images[0] = new Image<Gray, byte>(capture.QueryFrame().Bitmap);
                images[1] = images[0].PyrDown();
                images[2] = images[1].PyrDown();
                templateImages[0] = modelImage;
                templateImages[1] = modelImage.PyrDown();
                templateImages[2] = templateImages[1].PyrDown();
                rotateObservedImages[0] = images[2];
               double myAngle= MyApproch(images[2], templateImages[2], minAngle, maxAngle, 0.1);
               
                double maxV = 0;
                Mat result = new Mat();
                Point bestLoc = new Point();
               
                var rotateObserved = images[2].Rotate(myAngle, new Gray(0), true);
                    CvInvoke.MatchTemplate(rotateObserved, templateImages[2], result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                    double minValue = 0;
                    double maxValue = 0;
                    Point minLoc = new Point();
                    Point maxLoc = new Point();

                    CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
                  
                        maxV = maxValue;
                        bestLoc = maxLoc;
                    
                if (maxV > Convert.ToDouble(textBox4.Text))
                {
                    isGoodMatch = true;
                }
          
                
                    bestLoc.X *= 4;
                    bestLoc.Y *= 4;
            

                if (isGoodMatch)
                {
                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    var roi = new Rectangle(bestLoc, templateImages[0].Size);
                    capturedImage.ROI = roi;
                    var temp = capturedImage.Convert<Gray, byte>();
                    temp = temp.Canny(50, 200);
                    CvInvoke.FindContours(temp, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                    for (int i = 0; i < contours.Size; i++)
                    {
                        CvInvoke.DrawContours(capturedImage, contours, i, new MCvScalar(0, 255, 0, 100), 1);
                    }

                    capturedImage.ROI = Rectangle.Empty;
                    CvInvoke.Rectangle(capturedImage, roi, new MCvScalar(0, 255, 0, 100), 3);

                    CvInvoke.PutText(capturedImage, "X:" + bestLoc.X + "Y:" + bestLoc.Y +"MyAngle:"+myAngle, new Point(20, 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 255, 0, 100), 1);

                }
                else
                {
                    CvInvoke.PutText(capturedImage, "Not Found!", new Point(20, 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
                }
                imageBox.Image = capturedImage;


            }
        }
        private void matchLive()
        {
            while (true)
            {
                bool isGoodMatch = false;
                Image<Gray, byte>[] images = new Image<Gray, byte>[4];
                Image<Gray, byte>[] templateImages = new Image<Gray, byte>[4];
                Image<Gray, byte>[] rotateObservedImages = new Image<Gray, byte>[Convert.ToInt32(textBox1.Text) + 1];
                int step = Convert.ToInt32(textBox1.Text);
                double maxAngle = Convert.ToDouble(textBox3.Text);
                double minAngle = Convert.ToDouble(textBox2.Text);
                double intervalAngle = (maxAngle - minAngle) / step;
                var capturedImage = new Image<Bgra, byte>(capture.QueryFrame().Bitmap);
                images[0] = new Image<Gray, byte>(capture.QueryFrame().Bitmap);
                images[1] = images[0].PyrDown();
                images[2] = images[1].PyrDown();
                templateImages[0] = modelImage;
                templateImages[1] = modelImage.PyrDown();
                templateImages[2] = templateImages[1].PyrDown();
                rotateObservedImages[0] = images[2];
                for (int i = 1; i < rotateObservedImages.Length; i++)
                {
                    rotateObservedImages[i] = rotateObservedImages[0].Rotate(minAngle + (i - 1) * intervalAngle, new Gray(0), true);

                }
                Image<Gray, byte>[] rotateModelImage = new Image<Gray, byte>[step + 1];
                rotateModelImage[0] = templateImages[2];
                for (int i = 1; i < rotateModelImage.Length; i++)
                {
                    rotateModelImage[i] = rotateModelImage[0].Rotate(minAngle + (i - 1) * intervalAngle, new Gray(0), true);
                }

                int bestIndex = -1;

                double maxV = 0;
                Mat result = new Mat();
                Point bestLoc = new Point();
                double angle = 0;
                for (int i = 0; i < rotateObservedImages.Length; i++)
                {
                    CvInvoke.MatchTemplate(rotateObservedImages[i], templateImages[2], result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                    double minValue = 0;
                    double maxValue = 0;
                    Point minLoc = new Point();
                    Point maxLoc = new Point();

                    CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
                    if (maxV < maxValue)
                    {
                        bestIndex = i;
                        maxV = maxValue;
                        bestLoc = maxLoc;
                    }
                }
                if (maxV > Convert.ToDouble(textBox4.Text))
                {
                    isGoodMatch = true;
                }
                for (int i = 0; i < rotateModelImage.Length; i++)
                {
                    CvInvoke.MatchTemplate(rotateObservedImages[0], rotateModelImage[i], result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                    double minValue = 0;
                    double maxValue = 0;
                    Point minLoc = new Point();
                    Point maxLoc = new Point();

                    CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
                    if (maxV < maxValue)
                    {
                        bestIndex = i;
                        maxV = maxValue;
                        bestLoc = maxLoc;
                    }
                }
                if (bestIndex == 0)
                {
                    angle = 0;
                    bestLoc.X *= 4;
                    bestLoc.Y *= 4;
                }
                else
                {
                    angle = minAngle + (bestIndex - 1) * intervalAngle;
                    //var tempModel = modelImage.Rotate(-angle/180*Math.PI,new Gray(0));
                    //CvInvoke.MatchTemplate(rotateObservedImages[0], tempModel, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
                    //double minValue = 0;
                    //double maxValue = 0;
                    //Point minLoc = new Point();
                    //Point maxLoc = new Point();
                    //CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
                    //bestLoc = maxLoc;
                    bestLoc.X *= 4;
                    bestLoc.Y *= 4;
                }

                if (isGoodMatch)
                {
                    VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                    var roi = new Rectangle(bestLoc, templateImages[0].Size);
                    capturedImage.ROI = roi;
                    var temp = capturedImage.Convert<Gray, byte>();
                    temp = temp.Canny(50, 200);
                    CvInvoke.FindContours(temp, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                    for (int i = 0; i < contours.Size; i++)
                    {
                        CvInvoke.DrawContours(capturedImage, contours, i, new MCvScalar(0, 255, 0, 100), 1);
                    }

                    capturedImage.ROI = Rectangle.Empty;
                    CvInvoke.Rectangle(capturedImage, roi, new MCvScalar(0, 255, 0, 100), 3);

                    CvInvoke.PutText(capturedImage, "X:" + bestLoc.X + "Y:" + bestLoc.Y + "Angle:" + angle, new Point(20, 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);

                }
                else
                {
                    CvInvoke.PutText(capturedImage, "Not Found!", new Point(20, 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
                }
                imageBox.Image = capturedImage;


            }
        }


    }
}
