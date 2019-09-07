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
    public partial class Find1DBarcode : Form
    {
        ImageBox imageBox = null;
       
        Image orImage = null;
        Image<Gray, byte> image = null;
        Image<Bgra, byte> rgbImage = null;
        public Find1DBarcode()
        {
            InitializeComponent();

            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            panel1.Controls.Add(imageBox);
            imageBox.Show();

        }

      

        private void button1_Click(object sender, EventArgs e)
        {
           
            

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                image =new Image<Gray, byte>(fileDialog.FileNames[0]);
                orImage = Image.FromFile(fileDialog.FileNames[0]);
              
                rgbImage = new Image<Bgra, byte>(fileDialog.FileNames[0]);
                imageBox.Image =rgbImage;

             

            }
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var imagecopy = image.Copy();
            //CvInvoke.GaussianBlur(image, imagecopy, new Size(3, 3), 0);//高斯模糊
            //var imageX = image.Sobel(1, 0, 3);
            //var imageY = imagecopy.Sobel(0, 1, 3);
            //var verBar = image.InRange(new Gray(0), new Gray(90));
            //showImage1(verBar, "范围");
            //var blurImage = verBar.Copy();
            //blurImage = blurImage.SmoothGaussian(9);
            //showImage1(blurImage, "平滑");
            //var element = CvInvoke.GetStructuringElement(0, new Size(6, 6), new Point(-1, -1));
            //var diletImage = image.Copy();
            //CvInvoke.Dilate(verBar, diletImage, element, new Point(-1, -1), 8, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //showImage1(diletImage, "膨胀");
            //var closeImage = diletImage.Copy();
            //  CvInvoke.MorphologyEx(diletImage, closeImage, Emgu.CV.CvEnum.MorphOp.Close, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //  CvInvoke.MorphologyEx(closeImage, closeImage, Emgu.CV.CvEnum.MorphOp.Close, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            // showImage1(closeImage, "闭运算");
            //  findBarOne();
            //CvInvoke.ConvertScaleAbs(imageX,imageX,1,0);
            //CvInvoke.ConvertScaleAbs(imageY,imageY,1,0);
            // showImage(imageX, "X方向Sobel");
            // showImage(imageY, "Y方向Sobel");
            // //     var sobelout=imageX.Sub(imageY);
            // var sobelout = new Image<Gray, byte>(imageX.Width,imageX.Height);
            // CvInvoke.Subtract(imageX,imageY,sobelout,null, Emgu.CV.CvEnum.DepthType.Cv8U);
            //// var timage = sobelout.Convert<Gray, byte>();
            //// CvInvoke.ConvertScaleAbs(sobelout,sobelout,1,0);
            // showImage1(sobelout,"XY方向梯度差");
            //     timage = timage.ThresholdBinary(new Gray(100),new Gray(255));
            ////    timage = timage.ThresholdBinaryInv(new Gray(100),new Gray(255));
            //  //    sobelout._ThresholdBinary(new Gray(60),new Gray(255));
            //     showImage1(timage,"二值化");

            //var element = CvInvoke.GetStructuringElement(0,new Size(7,7),new Point(-1,-1));
            //CvInvoke.Dilate(sobelout, sobelout, element, new Point(-1, -1), 5, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //showImage1(sobelout, "膨胀");
            //CvInvoke.MorphologyEx(sobelout, sobelout, Emgu.CV.CvEnum.MorphOp.Close, element,new Point(-1,-1),1, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);

            //showImage1(sobelout,"闭运算");


            //CvInvoke.MorphologyEx(sobelout, sobelout, Emgu.CV.CvEnum.MorphOp.Close, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var copy1 = sobelout.Copy();
            // VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            // CvInvoke.FindContours(closeImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            // int maxIndex = -1;
            // double area = 0;
            // for (int i = 0; i < contours.Size; i++)
            // {
            //     double temp = CvInvoke.ContourArea(contours[i]);
            //     if (temp>area) {
            //         area = temp;
            //         maxIndex = i;

            //     }
            ////     CvInvoke.DrawContours(image,contours,i,new MCvScalar(60));

            // }
            // var rect = CvInvoke.BoundingRectangle(contours[maxIndex]);
            // image.Draw(rect,new Gray(60),3);
            // imageBox.Image = image;

            //CvInvoke.FindContours(copy1, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //int index = -1;
            //double primeter = -1;
            //for (int i = 0; i < contours.Size; i++)
            //{
            //  double temp=  CvInvoke.ArcLength(contours[i], true);
            //    if (temp>primeter) {
            //        index = i;
            //        primeter = temp;
            //    }
            //    //var rect = CvInvoke.BoundingRectangle(contours[i]);

            //    //image.Draw(rect, new Gray(0), 2);
            //    //CvInvoke.DrawContours(imageThreshold, contours, i, new MCvScalar(255));
            //}
            //var rect = CvInvoke.BoundingRectangle(contours[index]);
            //image.Draw(rect, new Gray(0), 2);
            //CvInvoke.DrawContours(imageThreshold, contours, index, new MCvScalar(255));
            //imageBox.Image = image;
            // findBarOne();
            // findBarTwo();
            // findBar3();
            findBar3();

        }
        public void findBarOne() {
            var imagecopy = image.Copy();
            var sobelX = image.Sobel(1,0,-1);
            showImage(sobelX,"sobelX");
            var sobelY = image.Sobel(0, 1, -1);
            showImage(sobelY,"sobleY");
            var gradient = sobelX - sobelY;

            CvInvoke.ConvertScaleAbs(gradient, gradient, 1, 0);
            showImage(gradient,"gradient");
            
          gradient._ThresholdBinary(new Gray(255),new Gray(255));
            showImage(gradient,"thresholdImage");
            var element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            var diletImage = gradient.Dilate(2);
            showImage(diletImage,"diletImage");

            var closeImage = new Image<Gray, byte>(diletImage.Width,diletImage.Height);
            
            CvInvoke.MorphologyEx(diletImage, closeImage, Emgu.CV.CvEnum.MorphOp.Close, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            showImage1(closeImage, "closeImage");
        }
        //public void findBarTwo() {
        //    var imagecopy = image.Copy();
        //   // var smoothImage = imagecopy.SmoothGaussian(3);
        //    var barLineImage = imagecopy.InRange(new Gray(0),new Gray(100));
        //    var lines=   barLineImage.HoughLinesBinary(1,Math.PI/45,10,10,10)[0];


        //    foreach (LineSegment2D line in lines) {
        //        barLineImage.Draw(line,new Gray(60),3);
        //        line.
        //    }
        //    imageBox.Image = barLineImage;
        //}
        public void findBar3() {
            var imagecopy = image.Copy();
            var sobelX = image.Sobel(1, 0, -1);
            var sobelY = image.Sobel(0,1,-1);

            var gradient = sobelX.Sub(sobelY);

            CvInvoke.ConvertScaleAbs(gradient,gradient,1,0);
            var bluredImage=gradient.SmoothBlur(9,9);
            var byteImage = bluredImage.Convert<Gray, byte>();
            var thresholdImage = byteImage.ThresholdBinary(new Gray(180),new Gray(255));
            var element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(21, 7), new Point(-1, -1));
            var closed = new Image<Gray, byte>(byteImage.Width,byteImage.Height);
            closed = thresholdImage.Dilate(2);
          //  CvInvoke.MorphologyEx(closed,closed, Emgu.CV.CvEnum.MorphOp.Close,element,new Point(-1,-1),2, Emgu.CV.CvEnum.BorderType.Default,CvInvoke.MorphologyDefaultBorderValue);
           // closed = closed.Dilate(2);
         //   closed = closed.Erode(4);
            

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
           // showImage1(closed, "Threshold");
            CvInvoke.FindContours(closed, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            double maxArea = 0;
            int maxIndex = -1;
            for (int i = 0; i < contours.Size; i++)
            {
                double tempArea = CvInvoke.ContourArea(contours[i]);
                if (tempArea>maxArea) {
                    maxArea = tempArea;
                    maxIndex = i;
                }
            
            }
            var rect = CvInvoke.BoundingRectangle(contours[maxIndex]);
            // CvInvoke.DrawContours(imagecopy, contours, maxIndex, new MCvScalar(60), 3);
            var imagecopy1 = rgbImage.Copy();
            imagecopy1.Draw(rect,new Bgra(0,255,0,100),3);
            CvInvoke.PutText(imagecopy1, "Found!", rect.Location, Emgu.CV.CvEnum.FontFace.HersheyComplex, 14,new MCvScalar(0,255,0));
            imageBox.Image = imagecopy1;
            showImage(sobelX, "SobelX");
            showImage(sobelY, "SobelY");
            showImage(gradient, "Gradient");
            showImage(bluredImage, "BluredImage");

            showImage1(byteImage, "ByteImage");
            showImage1(thresholdImage, "ThresholdImage");
            showImage1(closed, "Closed");





        }
        public void findBar4() {
            CascadeClassifier cascadeClassifier = new CascadeClassifier(@"E:\BarCode\data\cascade.xml");
           var rects= cascadeClassifier.DetectMultiScale(image);
            var ic = rgbImage.Copy();
            foreach (var rect in rects) {
                ic.Draw(rect, new Bgra(0, 255, 0, 100), 3);
            }
            imageBox.Image = ic;
        }

        public AfterProcessImage AfterProcessImage = null;
        public void showImage(Image<Gray,float> image,string name) {
            ImageBox imageBox = new ImageBox();
            imageBox.Width = image.Width;
            imageBox.Height = image.Height;
            imageBox.Image = image;
            imageBox.Name = name;
            AfterProcessImage = new AfterProcessImage();
            AfterProcessImage.imageBox = imageBox;
            AfterProcessImage.Show();
            AfterProcessImage.show();
            AfterProcessImage.Text = name;
            imageBox.Show();
        }
        public void showImage1(Image<Gray, byte> image, string name)
        {
            ImageBox imageBox = new ImageBox();
            imageBox.Width = image.Width;
            imageBox.Height = image.Height;
            imageBox.Image = image;
            imageBox.Name = name;
            AfterProcessImage = new AfterProcessImage();
            AfterProcessImage.imageBox = imageBox;
            AfterProcessImage.Show();
            AfterProcessImage.show();
            AfterProcessImage.Text = name;
            imageBox.Show();
        }
        private Rectangle rectangleROI;
        private CircleF circleROI;
        private RegionList regionList;
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Length == 0)
            {
                MessageBox.Show("没有选择区域类型");
                return;
            }
            else {
                if (regionList==null) {
                    regionList = new RegionList();
                    if (rectangleROI!=null) {
                        regionList.rectangleROI = rectangleROI;
                        regionList.addOneRegion();
                        regionList.Show();
                    }
                }
                switch (comboBox1.Text)
                {
                    case "矩形":
                       imageBox.MouseDown += ImageBox_MouseDown;
                        imageBox.MouseUp += ImageBox_MouseUp;
             
                        break;
                    case "圆形":

                        break;
                    case "椭圆形":
                        break;
                    case "多边形":
                        break;
                    default:
                        MessageBox.Show("无此种类型");
                        break;

                }
            }
           
        }
        //持续画
        private void ImageBox_MouseMove(object sender, MouseEventArgs e)
        {
            imageBox.Refresh();
            int width = Math.Abs(e.X - rectangleROI.X);
            int height = Math.Abs(e.Y - rectangleROI.Y);
            rectangleROI.Width = width;
            rectangleROI.Height = height;
            using (Graphics g=Graphics.FromHwnd(imageBox.Handle)) {
                g.DrawRectangle(new Pen(Color.Green,3),rectangleROI);
            }
        }
        //画完了
        private void ImageBox_MouseUp(object sender, MouseEventArgs e)
        {
           // imageBox.DrawToBitmap(imageBox.Image.Bitmap, rectangleROI);
            imageBox.MouseDown -= ImageBox_MouseDown;
            imageBox.MouseMove -= ImageBox_MouseMove;
            imageBox.MouseUp -= ImageBox_MouseUp;
        }
        //开始画
        private void ImageBox_MouseDown(object sender, MouseEventArgs e)
        {
            rectangleROI.Location = e.Location;
           imageBox.MouseMove += ImageBox_MouseMove;
        }

        Point lefttopPoint = new Point();
        Point rightBottomPoint = new Point();
        private void Find1DBarcode_MouseUp(object sender, MouseEventArgs e)
        {
            rightBottomPoint = e.Location;
            var rect= getRectangle(lefttopPoint,rightBottomPoint);
            using (Graphics g=Graphics.FromHwnd(imageBox.Handle)) {
                g.DrawRectangle(new Pen(Color.Green, 3), rect);
            }
                MouseDown -= Find1DBarcode_MouseDown;
            MouseUp -= Find1DBarcode_MouseUp;

        }

        private void Find1DBarcode_MouseDown(object sender, MouseEventArgs e)
        {
            lefttopPoint = e.Location;
        }
        public Rectangle getRectangle(Point point1,Point point2) {
            int centerX = (point1.X + point2.X) / 2;
            int centerY = (point1.Y + point2.Y) / 2;
            int width = Math.Abs(point1.X-point1.Y);
            int height = Math.Abs(point1.Y - point2.Y);
            Rectangle rectangle = new Rectangle(centerX,centerY,width,height);
            return rectangle;

        }

        private void Find1DBarcode_MouseMove(object sender, MouseEventArgs e)
        {
            
            
        }


    }
}
