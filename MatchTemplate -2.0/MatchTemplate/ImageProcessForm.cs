using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class ImageProcessForm : Form
    {
        Bitmap bitmap = null;
        ImageBox imageBox = null;
        Rectangle rectangleROI;
        bool hasROI = false;
        PictureBox pictureBox = null;
        public delegate void FindMarkHandler(Image<Gray,byte> image);

        public ImageProcessForm()
        {
            InitializeComponent();
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            ContainerPlaner.Controls.Add(pictureBox);
            pictureBox.MouseHover += ImageBox_MouseHover;
        }
        /// <summary>
        /// 判断当前鼠标是否在矩形边上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageBox_MouseHover(object sender, EventArgs e)
        {
            if (this.Cursor.HotSpot.X > rectangleROI.Left && this.Cursor.HotSpot.X < rectangleROI.Left + 4 || Cursor.HotSpot.X < rectangleROI.Right && Cursor.HotSpot.X > rectangleROI.Right - 4)
            {
                this.Cursor = Cursors.UpArrow;
            }
            else {
                Cursor = Cursors.Default;
            }

        }

        private void 选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 打开图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                bitmap = new Bitmap(fileDialog.FileNames[0]);
                this.pictureBox.Image = bitmap;


            }
        }

        private void 选择矩形区域ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null) {
                MessageBox.Show("没有选择图片", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            pictureBox.MouseDown += ImageBox_MouseDown; 
            pictureBox.MouseUp += ImageBox_MouseUp;
            pictureBox.Image = bitmap;
          //  pictureBox.SetZoomScale(1, imageBox.Location);
            this.Cursor = Cursors.Hand;
        }
        /// <summary>
        /// 矩形画完了的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageBox_MouseUp(object sender, MouseEventArgs e)
        {
            //imageBox.Enabled = false;
        //   pictureBox.Refresh();
            pictureBox.MouseDown -= ImageBox_MouseDown;
            pictureBox.MouseMove -= ImageBox_MouseMove;
            pictureBox.MouseUp -= ImageBox_MouseUp;
            // imageBox.Enabled = true;
            this.Cursor = Cursors.Default;
          //  imageBox.SetZoomScale(1, imageBox.Location);
            //using (var g = Graphics.FromHdc(pictureBox.Handle)) {
            //    g.DrawRectangle(new Pen(Color.Green), rectangleROI);
               
            //}
            hasROI = true;


        }

        /// <summary>
        /// 画矩形函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        Point oringinal = new Point();
        private void ImageBox_MouseDown(object sender, MouseEventArgs e)
        {
            //   imageBox.Enabled = false;
            oringinal = e.Location;
            int x = pictureBox.Image.Width / pictureBox.Width * e.Location.X;
            int y = pictureBox.Image.Height / pictureBox.Height * e.Location.Y;
            
            rectangleROI.Location =e.Location;
            pictureBox.MouseMove += ImageBox_MouseMove;
        }
        /// <summary>
        /// 鼠标在imageBox上移动画图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageBox_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox.Refresh();
            int width = Math.Abs(e.X - oringinal.X);
            int height = Math.Abs(e.Y - oringinal.Y);
            rectangleROI.Width = width;
            rectangleROI.Height = height;
            using (Graphics g = Graphics.FromHwnd(pictureBox.Handle)) {
                g.DrawRectangle(new Pen(Color.Green), rectangleROI);//把选定的区域画出来
            }

        }
        ThresholdParameterForm thresholdParameterForm = null;
        public delegate void doThreshold();
        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 全局二值化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bitmap==null) {
                MessageBox.Show("没有打开图片...");
                return;
            }
            if (hasROI) {
                if (MessageBox.Show( "当前ROI Location X:" + rectangleROI.X + "\r\nLocation Y:" + rectangleROI.Y + "\r\nWidth:" + rectangleROI.Width + "\r\n" + "Height:" + rectangleROI.Height, "ROI", MessageBoxButtons.OKCancel)== DialogResult.Cancel)
                {
                    return;
                }
                
            }
          
            thresholdParameterForm = new ThresholdParameterForm();
            thresholdParameterForm.doT += ThresholdParameterForm_doT;
            thresholdParameterForm.Show();


        }

        private void ThresholdParameterForm_doT()
        {
            Image<Bgra, byte> image = new Image<Bgra, byte>(bitmap);
            var grayImage = image.Convert<Gray, byte>();
            if (hasROI) {
                grayImage.ROI = rectangleROI;
            }
            var thresholdImage= grayImage.ThresholdBinary(new Gray(ThresholdParameterForm.threshold),new Gray (ThresholdParameterForm.maxValue));
            this.imageBox.Image = thresholdImage;
        }

        /// <summary>
        /// 取消选择ROI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 取消选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hasROI = false;
            rectangleROI.X = 0;
            rectangleROI.Y = 0;
            rectangleROI.Width = 0;
            rectangleROI.Height = 0;
        }
        /// <summary>
        /// 清除所有处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 还原初始图像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageBox.Image = new Image<Bgra, byte>(bitmap);
            hasROI = false;
            rectangleROI.X = 0;
            rectangleROI.Y = 0;
            rectangleROI.Width = 0;
            rectangleROI.Height = 0;
        }

        ContoursForm contoursForm = null;
        public delegate void FindContoursHandler(int threshold,double parameter);
        /// <summary>
        /// 找轮廓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 查找轮廓ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bitmap == null)
            {
                MessageBox.Show("没有打开图片...");
                return;
            }
            if (hasROI)
            {
                if (MessageBox.Show("当前ROI Location X:" + rectangleROI.X + "\r\nLocation Y:" + rectangleROI.Y + "\r\nWidth:" + rectangleROI.Width + "\r\n" + "Height:" + rectangleROI.Height, "ROI", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }


            }
            //if (contoursForm==null) {
                contoursForm = new ContoursForm();
            //}
            
            contoursForm.doFindContours += ContoursForm_doFindContours;
          //  contoursForm.doFindMark += ContoursForm_doFindMark;
            contoursForm.doSACFind += ContoursForm_doSACFind;
            contoursForm.sourceImage = new Image<Gray, byte>(bitmap);
            contoursForm.Show();

            
        }

        private void ContoursForm_doSACFind(Image<Gray, byte> sourceImage, Image<Gray, byte> templateImage, double minAngle, double maxAngle, int step, out Point centerOfMatch, out double angle, double socre)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            myMatch(sourceImage,templateImage,minAngle,maxAngle,step,out centerOfMatch,out angle,socre);
            sw.Stop();
            Image<Bgr, byte> tempImage = new Image<Bgr, byte>(bitmap);
            CvInvoke.Rectangle(tempImage, new Rectangle(centerOfMatch, templateImage.Size), new MCvScalar(255, 0, 0, 100), 3, Emgu.CV.CvEnum.LineType.AntiAlias);
            CvInvoke.PutText(tempImage, "Marker Found X:" + centerOfMatch.X + "\r\nY:" + centerOfMatch.Y+"\r\nAngle："+angle, new Point(pictureBox.Location.X, pictureBox.Location.Y + 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 255, 0, 100), 3);
            pictureBox.Image = tempImage.Bitmap;

        }

        public VectorOfVectorOfPoint allContours = new VectorOfVectorOfPoint();
        //public static void biggerSort(VectorOfVectorOfPoint vp) {

        //    for (int i=0;i<vp.Size;i++) {
        //        for (int j=i;j<vp.Size;j++) {
                    
        //            if (CvInvoke.ContourArea(vp[i])<CvInvoke.ContourArea(vp[j])) {
        //                var temp = vp[i];
        //                vp[i]. = vp[j];
        //                vp[j] = temp;
        //            }
        //        }
        //    }
        //}
        private void ContoursForm_doFindMark(Image<Gray,byte> image)
        {
            //if (hasROI) {
            //    if (MessageBox.Show("存在图片范围限制，是否删除ROI？", "删除？", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
            //        hasROI = false;
            //        MessageBox.Show("已删除ROI.");

            //    }
            //}



            ////double modelPermeiter = CvInvoke.ArcLength(modelContours[0],true);
            //double socre = 5;//形状得分
            //int index = -1;//轮廓的索引
            //List<int> indexes = new List<int>();
            //if (modelContours.Size > 0 && allContours.Size > 0)
            //{

            //    for (int i=0;i<allContours.Size;i++) {
            //        for (int j=0;j<modelContours.Size;j++) {
            //            double tempArea = CvInvoke.ContourArea(allContours[i]);
            //            double modelArea = CvInvoke.ContourArea(modelContours[j]);
            //            double tempPermeiter = CvInvoke.ArcLength(allContours[i], true);
            //            //if (tempArea > modelArea * 1.2 || tempArea < modelArea * 0.8)
            //            //{
            //            //    continue;
            //            //}
            //            double tempsocre = CvInvoke.MatchShapes(modelContours[j], allContours[i], Emgu.CV.CvEnum.ContoursMatchType.I1);
            //            if (tempsocre < socre)
            //            {
            //                socre = tempsocre;
            //                index = i;
            //                indexes.Add(i);
            //            }
            //        }


            //    }
            //    Bitmap bitmap1 = (Bitmap)bitmap.Clone();
            //    for (int i=0;i<indexes.Count;i++) {
            //        RotatedRect rotatedRect = CvInvoke.MinAreaRect(allContours[indexes[i]]);
            //        var rect = rotatedRect.MinAreaRect();
            //        using (var g = Graphics.FromImage(bitmap1))
            //        {
            //            g.DrawRectangle(new Pen(Color.Blue, 3), rect);
            //        }
            //    }


            //    imageBox.Image = new Image<Bgra, byte>(bitmap1);

            // }
            //else {
            //    MessageBox.Show("没有提供轮廓信息...");
            //}
            Mat result = new Mat();
            var obseredImage = new Image<Gray, byte>(bitmap);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
           
            CvInvoke.MatchTemplate(obseredImage,image,result, Emgu.CV.CvEnum.TemplateMatchingType.Ccoeff,null);
            stopWatch.Stop();
            double a = 0, b = 0;
            Point loc = new Point();
            Point LOC = new Point();
            CvInvoke.MinMaxLoc(result, ref a, ref b, ref loc, ref LOC, null);
            Bitmap bitmap1 =(Bitmap) bitmap.Clone();
            MessageBox.Show("算法耗时:" + stopWatch.ElapsedMilliseconds + "ms\r\nPosition: X:"+LOC.X+"\r\nY:"+LOC.Y);
            CvInvoke.Rectangle(imageBox.Image,new Rectangle(LOC,image.ROI.Size),new MCvScalar(255,0,0,100),3, Emgu.CV.CvEnum.LineType.AntiAlias);
            CvInvoke.PutText(imageBox.Image,"Marker Found X:"+LOC.X+"\r\nY:"+LOC.Y,new Point(imageBox.Location.X,imageBox.Location.Y+100), Emgu.CV.CvEnum.FontFace.HersheyComplex,1,new MCvScalar(0,255,0,100),3);
            //  imageBox.Image = new Image<Bgra, byte>(bitmap1);
          
        }
        public delegate void SACFindMarkHandler(Image<Gray, byte> sourceImage, Image<Gray, byte> templateImage, double minAngle, double maxAngle, int step, out Point centerOfMatch, out double angle,double socre);
        public static bool isFineMatch = false;
        public static void myMatch(Image<Gray,byte> sourceImage,Image<Gray,byte> templateImage,double minAngle,double maxAngle,int step,out Point centerOfMatch,out double angle, double sorce) {
            if (sourceImage.Size.Width<templateImage.Size.Width||sourceImage.Height<templateImage.Height) {
                throw new Exception("图像大小不符合");
            }
            Image<Gray, byte>[] images = new Image<Gray, byte>[4];
            Image<Gray, byte>[] templateImages = new Image<Gray, byte>[4];

            List<double> matchSocreList = new List<double>();
            templateImages[0] = templateImage;
            templateImages[1] = templateImages[0].PyrDown();
            templateImages[2] = templateImages[1].PyrDown();
          //  templateImages[3] = templateImages[2].PyrDown();

            images[0] = sourceImage;
            images[1] = images[0].PyrDown();
            images[2] = images[1].PyrDown();

            Mat result = new Mat();//结果Mat

            Image<Gray, byte>[] rotatedModelImages = new Image<Gray, byte>[step+1];
            Image<Gray, byte>[] rotatedObserveImages = new Image<Gray, byte>[step+1];
            rotatedObserveImages[0] = images[2];
            rotatedModelImages[0] = templateImages[2];
            double marginAngle = maxAngle - minAngle;//角度范围
            double angleInteval = marginAngle / step;//步长角度
            for (int i=0;i<step;i++) {
                rotatedObserveImages[i + 1] = images[2].Rotate((minAngle + step * i) / 180 * Math.PI, new PointF(images[2].Width / 2, images[2].Height / 2), Emgu.CV.CvEnum.Inter.Linear, new Gray(255),true);
            }
            double maxValue =-1;
            int bestAngleIndex = -1;
            Point bestAnglePoint=new Point();

            //旋转图像进行匹配
            for (int i=0;i<rotatedObserveImages.Length;i++) {
                Mat tempResult = new Mat();
                CvInvoke.MatchTemplate(rotatedObserveImages[i], templateImages[2], tempResult, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);//最大值处匹配
                double a1 = 0, b1 = 0;
                Point loc1 = new Point();
                Point LOC1= new Point();
                CvInvoke.MinMaxLoc(tempResult, ref a1, ref b1, ref loc1, ref LOC1, null);
                matchSocreList.Add(b1);
                if (b1>maxValue) {
                    maxValue = b1;
                    bestAngleIndex = i;//最好的角度索引记录下来
                    bestAnglePoint = LOC1;//最佳匹配位置记录下来
                }

            }

            //无旋转是最佳的匹配
            if (bestAngleIndex == 0)
            {
                angle = 0;
                centerOfMatch = bestAnglePoint;
                centerOfMatch.X *= 4;
                centerOfMatch.Y *= 4;
            }
   
            //有旋转的最佳匹配
            else {
                angle = minAngle + (bestAngleIndex - 1) * angleInteval;
               // bestAnglePoint.X += templateImages[2].Width / 2;
               // bestAnglePoint.Y += templateImages[2].Height / 2;
                double x0 =( bestAnglePoint.X - rotatedObserveImages[0].Width / 2)*Math.Cos(-angle/180*Math.PI)-(bestAnglePoint.Y-rotatedObserveImages[0].Height/2)*Math.Sin(-angle / 180 * Math.PI) +rotatedObserveImages[0].Width/2;
                double y0 = (bestAnglePoint.X - rotatedObserveImages[0].Width / 2) * Math.Sin(-angle / 180 * Math.PI) + (bestAnglePoint.Y - rotatedObserveImages[0].Height / 2) * Math.Cos(-angle / 180 * Math.PI) + rotatedObserveImages[0].Height / 2;

                bestAnglePoint.X = (int)x0*4;
                bestAnglePoint.Y = (int)y0*4;
                centerOfMatch = bestAnglePoint;
            }
            if (maxValue<sorce)
            {//如果分数阈值没达到
                isFineMatch = false;
                centerOfMatch = bestAnglePoint;
                return;
            }
         
            isFineMatch = true;
        }
        private void ContoursForm_doFindContours(int threshold, double parameter)
        {
            Image<Bgra, byte> image1 = new Image<Bgra, byte>(bitmap);
            Image<Gray, byte> image = new Image<Gray, byte>(bitmap);
            if (hasROI) {
                image.ROI = rectangleROI;
                image1.ROI = rectangleROI;
            }
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            var thresholdImage = image.ThresholdBinary(new Gray(threshold),new Gray(255));
          //  var cannyImage = thresholdImage.Canny(threshold, parameter);
            CvInvoke.FindContours(thresholdImage, contours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            
            for (int i=0;i<contours.Size;i++) {
                CvInvoke.DrawContours(image1, contours, i, new MCvScalar(0, 255, 0));
            }
           
            contoursForm.modelContour = contours;
            contoursForm.showContoursImage(image1);
            image.ROI = Rectangle.Empty;
            CvInvoke.FindContours(image, allContours, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);

        }

        private void 轮廓匹配ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
      public   Emgu.CV.Capture capture;
        Thread task;
        /// <summary>
        /// OpenCamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 打开相机ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            capture = new Emgu.CV.Capture(0);
             task = new Thread(showCaptureImage);
            task.Start();
          
           
        }
        void showCaptureImage() {
            lock (lock1)
            {
                while (true)
                {
                    Mat a = capture.QueryFrame();
                    
                    //pictureBox.Image = a.Bitmap;
                    pictureBox.Image = a.Bitmap;
                    currentImage = new Image<Bgr, byte>(a.Bitmap);
                    Thread.Sleep(5);

                }
            }
        }
        /// <summary>
        /// 抓到图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
           
        }

        private void 关闭相机ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.camp!=null) {
                camp.Close();
            }
            task.Abort();
            capture.ImageGrabbed -= Capture_ImageGrabbed;
            capture.Stop();
            capture.Dispose();
        }

        private void 实时匹配ToolStripMenuItem_Click(object sender, EventArgs e)
        {
          ContoursForm contoursForm=  new ContoursForm();
            contoursForm.doSACFind += ContoursForm_doSACFind1;
            contoursForm.Show();
        }
        public Image<Gray, byte> modelImage = null;
        public double minAngle,maxAngle;
        public int step;
        public Point centerOfMatch;
        public double angle;
        public double socre;
        public Image<Bgr, byte> currentImage = new Image<Bgr, byte>(new Size());
        private object lock1 = new object();
        private void ContoursForm_doSACFind1(Image<Gray, byte> sourceImage, Image<Gray, byte> templateImage, double minAngle, double maxAngle, int step, out Point centerOfMatch, out double angle, double socre)
        {
            

            myMatch(new Image<Gray, byte>(new Bitmap(pictureBox.Image)),templateImage,minAngle,maxAngle,step,out centerOfMatch,out angle, socre);
            Image<Bgr, byte> image = new Image<Bgr, byte>(new Bitmap(pictureBox.Image));
            CvInvoke.Rectangle(image, new Rectangle(centerOfMatch, templateImage.Size), new MCvScalar(255, 0, 0, 100), 3, Emgu.CV.CvEnum.LineType.AntiAlias);
            CvInvoke.PutText(image, "Marker Found X:" + centerOfMatch.X + "\r\nY:" + centerOfMatch.Y + "\r\nAngle：" + angle, new Point(pictureBox.Location.X, pictureBox.Location.Y + 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 255, 0, 100), 3);
            pictureBox.Image = image.Bitmap;
       
            modelImage = templateImage;
            this.minAngle = minAngle;
            this.maxAngle = maxAngle;
            this.step = step;
            this.centerOfMatch = centerOfMatch;
            this.angle = angle;
            this.socre = socre;
            capture.ImageGrabbed += Capture_ImageGrabbed1;

        }
     
      

        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        {
           
            if (modelImage == null)
            {
                
            }
            else
            {
                try
                {
                    Image<Bgr, byte> image = currentImage;
                    myMatch(image.Convert<Gray, byte>(), this.modelImage, this.minAngle, this.maxAngle, this.step, out this.centerOfMatch, out this.angle, this.socre);
                    if (!isFineMatch) {
                       
                        CvInvoke.PutText(image, "No Find", new Point(pictureBox.Location.X, pictureBox.Location.Y + 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 2);
                        pictureBox.Image = image.Bitmap;
                        return;
                    }
                  
                    CvInvoke.Rectangle(image, new Rectangle(centerOfMatch, modelImage.Size), new MCvScalar(0,255, 0, 100), 3, Emgu.CV.CvEnum.LineType.AntiAlias);
                    CvInvoke.PutText(image, "Marker Found X:" + centerOfMatch.X + "Y:" + centerOfMatch.Y + " Angle:" + angle, new Point(pictureBox.Location.X, pictureBox.Location.Y + 100), Emgu.CV.CvEnum.FontFace.HersheyComplex,1, new MCvScalar(0, 255, 0, 100), 2);
                    pictureBox.Image = image.Bitmap;
                }
                catch {

                }
            }
        }

        Thread b = null;
        CameraPropertyForm camp;
        private void 相机配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camp = new CameraPropertyForm(this);
            camp.Show();
  
        }

        void keepFinding() {
            while (true) {
                Image<Bgr, byte> image = new Image<Bgr, byte>(new Bitmap(pictureBox.Image));
                myMatch(image.Convert<Gray,byte>(),this.modelImage,this.minAngle,this.maxAngle,this.step,out this.centerOfMatch,out this.angle,this.socre);
                CvInvoke.Rectangle(image, new Rectangle(centerOfMatch, modelImage.Size), new MCvScalar(255, 0, 0, 100), 3, Emgu.CV.CvEnum.LineType.AntiAlias);
                CvInvoke.PutText(image, "Marker Found X:" + centerOfMatch.X + "\r\nY:" + centerOfMatch.Y + "\r\nAngle：" + angle, new Point(pictureBox.Location.X, pictureBox.Location.Y + 100), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 255, 0, 100), 3);
                pictureBox.Image = image.Bitmap;
            }
        }
    }
}
