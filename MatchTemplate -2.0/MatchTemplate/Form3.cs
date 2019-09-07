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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Basler.Pylon;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace MatchTemplate
{
    public partial class Form3 : Form
    {
        ImageBox imageBox1;
        ImageBox imageBox2;
        Image<Bgra, byte> sourceImage;
        Image<Bgra, byte> modelImage;
        public Form3()
        {
            InitializeComponent();
            imageBox1 = new ImageBox();
            imageBox1.Dock = DockStyle.Fill;
            panel3.Controls.Add(imageBox1);
            imageBox2 = new ImageBox();
            imageBox2.Dock = DockStyle.Fill;
            panel4.Controls.Add(imageBox2);
            CheckForIllegalCrossThreadCalls = false;
        }
        PointF[] rawPoints = new PointF[4];
        PointF[] correctPoints = new PointF[4];
        Mat homograph;
        private void loadHomograph() {

            StreamReader streamReader = new StreamReader(Application.StartupPath+"\\Verification.txt");
            for (int i=0;i<4;i++) {
                string pointpair=streamReader.ReadLine();
                string[] points = pointpair.Split(',');
                rawPoints[i].X = Convert.ToSingle(points[0]);
                rawPoints[i].Y = Convert.ToSingle(points[1]);
                correctPoints[i].X = Convert.ToSingle(points[2]);
                correctPoints[i].Y = Convert.ToSingle(points[3]);
            }
            homograph = new Mat();
            CvInvoke.FindHomography(rawPoints, correctPoints, homograph, Emgu.CV.CvEnum.HomographyMethod.Default);
            showLog("坐标系单应矩阵计算完成---");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                sourceImage = new Image<Bgra, byte>(fileDialog.FileNames[0]);
                imageBox1.Image = sourceImage;
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
                modelImage = new Image<Bgra, byte>(fileDialog.FileNames[0]);
                imageBox2.Image = modelImage;
            }
        }
        public string Mark(Image<Gray,byte> sourceImage,Image<Gray,byte> modelImage,double scoreThreshold,double minAngle,double maxAngle,double accruration,double hierarachyLevel) {
            //降采样
            bool isGoodMatch = false;
            for (int i=0;i<hierarachyLevel;i++) {
                modelImage = modelImage.PyrDown();
                sourceImage = sourceImage.PyrDown();
            }
            double angle = MyApproch(sourceImage,modelImage,minAngle,maxAngle,accruration);
            var rotatedObsered = sourceImage.Rotate(angle,new Gray(0),true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(rotatedObsered, modelImage, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
            double minValue = 0, maxValue = 0;
            Point bestLoc = new Point();
            Point minLoc = new Point();
            Point maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
            if (maxValue>=scoreThreshold) {
                isGoodMatch = true;
            }
            double times = Math.Pow(2,hierarachyLevel);//得到倍数
            if (isGoodMatch) {//如果得分超过阈值

            }

            return "";
        }
        public string Mark() {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Image<Gray, byte> sourceImage = this.sourceImage.Convert<Gray, byte>();
            Image<Gray, byte> modelImage = this.modelImage.Convert<Gray, byte>();
            sourceImage = sourceImage.PyrDown().PyrDown().PyrDown();
            modelImage = modelImage.PyrDown().PyrDown().PyrDown();
            bool isGoodMatch = false;
            double left = (double)this.numericUpDown1.Value;
            double right = (double)numericUpDown2.Value;
            double acc = Convert.ToDouble(textBox3.Text);
            double angle = MyApproch(sourceImage, modelImage, left, right, acc);
            double socre = Convert.ToDouble(textBox1.Text);
            var rotatedObsered = sourceImage.Rotate(angle, new Gray(0), true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(rotatedObsered, modelImage, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
            double minValue = 0;
            double maxValue = 0;

            Point bestLoc = new Point();
            Point minLoc = new Point();
            Point maxLoc = new Point();

            CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
            bestLoc = maxLoc;

            if (maxValue >= socre)
            {
                isGoodMatch = true;
            }
            sw.Stop();
            label5.Text = sw.Elapsed.TotalMilliseconds.ToString();
            if (isGoodMatch)
            {
                maxLoc.X = maxLoc.X * 8;
                maxLoc.Y = maxLoc.Y * 8;
                Point rectOringinal = new Point();
                Point point2 = new Point(maxLoc.X + this.modelImage.Width,maxLoc.Y);
                Point point3 = new Point(maxLoc.X, maxLoc.Y + this.modelImage.Height);
                Point point4 = new Point(maxLoc.X + this.modelImage.Width,maxLoc.Y+ this.modelImage.Height);
                Point rotateCenter = new Point(this.sourceImage.Width / 2, this.sourceImage.Height / 2);
                rectOringinal.X= (int)
                    (
                        (maxLoc.X - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );
              rectOringinal.Y= (int)((maxLoc.X  - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point2.X= (int)((point2.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) -(point2.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point2.Y = (int)((point2.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point2.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point3.X = (int)((point3.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point3.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point3.Y = (int)((point3.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point3.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point4.X = (int)((point4.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point4.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point4.Y = (int)((point4.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point4.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                PointF accPoint = new PointF();
                accPoint.X = 
                       Convert.ToSingle((maxLoc.X + this.modelImage.Width / 2 - rotateCenter.X)
                       * Math.Cos((-angle / 180) * Math.PI)
                       - (maxLoc.Y + this.modelImage.Height / 2 - rotateCenter.Y)
                       * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                   );
                accPoint.Y = Convert.ToSingle(((maxLoc.X + this.modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + this.modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y));
                maxLoc.X = (int)
                    (
                        (maxLoc.X + this.modelImage.Width / 2 - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y + this.modelImage.Height / 2 - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );
                
                maxLoc.Y = (int)((maxLoc.X + this.modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + this.modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
               
                Rectangle rect = new Rectangle(rectOringinal, this.modelImage.Size);
                this.sourceImage.ROI = rect;

                // var display=this.sourceImage.Canny(60,100);
                //// display.ROI = Rectangle.Empty;
                // var contours = new VectorOfVectorOfPoint();
                //// display.ROI = rect;
                // CvInvoke.FindContours(display, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                // this.sourceImage.ROI = rect;
                // for (int i=0;i<contours.Size;i++) {
                //     CvInvoke.DrawContours(this.sourceImage,contours,i,new MCvScalar(0,255,0,100),3, Emgu.CV.CvEnum.LineType.AntiAlias);
                // }
                // display.ROI = Rectangle.Empty;
                // this.sourceImage.ROI = Rectangle.Empty;
                // imageBox1.Image = this.sourceImage;
                //var tempImage = this.sourceImage.Canny(60, 100);
                //var contours = new VectorOfVectorOfPoint();
                //CvInvoke.FindContours(tempImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                //for (int i = 0; i < contours.Size; i++)
                //{
                //    CvInvoke.DrawContours(imageBox1.Image, contours, i, new MCvScalar(0, 255, 0, 100), 2);
                //}
                this.sourceImage.ROI = Rectangle.Empty;
                CvInvoke.Line(imageBox1.Image, rectOringinal, point2,new MCvScalar(0,255,0,100) ,3);
                CvInvoke.Line(imageBox1.Image, point2, point4, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox1.Image, point4, point3, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox1.Image, point3, rectOringinal, new MCvScalar(0, 255, 0, 100), 3);
                // CvInvoke.Rectangle(imageBox1.Image, rect, new MCvScalar(0, 255, 0, 100), 10);
                CvInvoke.PutText(imageBox1.Image, "X:" + (maxLoc.X )+ " Y:" + (maxLoc.Y) + " Angle:" + (-angle), new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyPlain, 10, new MCvScalar(0, 255, 0, 100), 5);
                imageBox1.Image = imageBox1.Image;
                PointF[] pointF = new PointF[1];
                pointF[0] =accPoint;
                string a, b, c;
                pointF = CvInvoke.PerspectiveTransform(pointF, homograph);
                if (pointF[0].X >= 0)
                {
                    a = string.Format("{0:+00000.000}", pointF[0].X);
                }
                else {
                    a= string.Format("{0:00000.000}", pointF[0].X);
                }
                if (pointF[0].Y >= 0)
                {
                    b = string.Format("{0:+00000.000}",pointF[0].Y);
                }
                else {
                    b = string.Format("{0:00000.000}", pointF[0].Y);
                }
                if ((-angle) >=0)
                {
                   c = string.Format("{0:+00000.000}",(-angle));
                }
                else
                {
                    c = string.Format("{0:00000.000}", (-angle));
                }
                showLog("1," + a + "," + b + "," + c);
                return "1,"+a+ "," +b + "," + c;

            }
            else
            {
                CvInvoke.PutText(imageBox1.Image, "No Found", new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyComplex, 10, new MCvScalar(0, 0, 255, 100), 5);
                imageBox1.Image = imageBox1.Image;
                return "0,0,0,0";
            }
         
    
        }
        /// <summary>
        /// 定位匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            // Stopwatch sw = new Stopwatch();
            // sw.Start();
            // Image<Gray, byte> sourceImage = this.sourceImage.Convert<Gray, byte>();
            // Image<Gray, byte> modelImage = this.modelImage.Convert<Gray, byte>();
            // sourceImage = sourceImage.PyrDown().PyrDown().PyrDown();
            // modelImage = modelImage.PyrDown().PyrDown().PyrDown();
            // bool isGoodMatch = false;
            //double left = (double)this.numericUpDown1.Value;
            // double right = (double)numericUpDown2.Value;
            // double acc = Convert.ToDouble(textBox3.Text);
            // double angle=MyApproch(sourceImage, modelImage, left, right, acc);
            // double socre = Convert.ToDouble(textBox1.Text);
            // var rotatedObsered = sourceImage.Rotate(angle,new Gray(0),true);
            // Mat result = new Mat();
            // CvInvoke.MatchTemplate(rotatedObsered, modelImage, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
            // double minValue = 0;
            // double maxValue = 0;

            // Point bestLoc = new Point();
            // Point minLoc = new Point();
            // Point maxLoc = new Point();

            // CvInvoke.MinMaxLoc(result, ref minValue, ref maxValue, ref minLoc, ref maxLoc);
            // bestLoc = maxLoc;

            // if (maxValue>=socre) {
            //     isGoodMatch = true;
            // }
            // sw.Stop();
            // label5.Text = sw.Elapsed.TotalMilliseconds.ToString();
            // if (isGoodMatch)
            // {
            //     maxLoc.X = maxLoc.X * 8;
            //     maxLoc.Y = maxLoc.Y * 8;

            //     Point rotateCenter = new Point(this.sourceImage.Width/2,this.sourceImage.Height/2);
            //     maxLoc.X = (int)((maxLoc.X - rotateCenter.X) * Math.Cos((-angle/180)*Math.PI) - (maxLoc.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
            //     maxLoc.Y = (int)((maxLoc.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
            // Rectangle rect= new Rectangle(maxLoc, this.modelImage.Size);
            //      this.sourceImage.ROI = rect;

            //     // var display=this.sourceImage.Canny(60,100);
            //     //// display.ROI = Rectangle.Empty;
            //     // var contours = new VectorOfVectorOfPoint();
            //     //// display.ROI = rect;
            //     // CvInvoke.FindContours(display, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            //     // this.sourceImage.ROI = rect;
            //     // for (int i=0;i<contours.Size;i++) {
            //     //     CvInvoke.DrawContours(this.sourceImage,contours,i,new MCvScalar(0,255,0,100),3, Emgu.CV.CvEnum.LineType.AntiAlias);
            //     // }
            //     // display.ROI = Rectangle.Empty;
            //     // this.sourceImage.ROI = Rectangle.Empty;
            //     // imageBox1.Image = this.sourceImage;
            //     var tempImage = this.sourceImage.Canny(60,100);
            //     var contours = new VectorOfVectorOfPoint();
            //     CvInvoke.FindContours(tempImage,contours,null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            //     for (int i=0;i<contours.Size;i++) {
            //         CvInvoke.DrawContours(imageBox1.Image,contours,i,new MCvScalar(0,255,0,100),2);
            //     }
            //     this.sourceImage.ROI = Rectangle.Empty;

            //     CvInvoke.Rectangle(imageBox1.Image, rect, new MCvScalar(0, 255, 0, 100), 10);
            //     CvInvoke.PutText(imageBox1.Image, "X:" + (maxLoc.X +this.modelImage.Width/2) + " Y:" + (maxLoc.Y+this.modelImage.Height/2) + " Angle:" + (-angle), new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyPlain,10, new MCvScalar(0, 255, 0, 100),5);
            // }
            // else {
            //     CvInvoke.PutText(imageBox1.Image, "No Found", new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyComplex, 10, new MCvScalar(0, 0, 255, 100),5);
            // }
            // // imageBox1.Refresh();
            // imageBox1.Image = imageBox1.Image;
            Mark();
          
        }
        private static double myScore(Image<Gray, byte> sourceImage, Image<Gray, byte> tempImage, double angle)
        {
            var temp = sourceImage.Rotate(angle, new Gray(0), true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(temp, tempImage, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed, null);

            double min = 0, max = 0;
            Point loc = new Point();
            Point LOC = new Point();
            CvInvoke.MinMaxLoc(result, ref min, ref max, ref loc, ref LOC, null);
            return max;
        }
        public static double MyApproch(Image<Gray, byte> sourceImage, Image<Gray, byte> tempImage, double left, double right, double acu)
        {
            if (Math.Abs(right - left) < acu)
            {
                return myScore(sourceImage, tempImage, left) > myScore(sourceImage, tempImage, right) ? left : right;
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
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }
            else
            {//如果右边要高
                left = (right + left) / 2;
                return MyApproch(sourceImage, tempImage, left, right, acu);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        Socket socket;
        private void button4_Click(object sender, EventArgs e)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.1.31"),10004);
         
            socket.Bind(endPoint);
            socket.Listen(10);
            showLog("开始监听----");
            Thread thread = new Thread(acceptClient);
            thread.IsBackground = true;
            thread.Start();
        }
        private void showLog(string text) {
            textBox2.Text += "\r\n" + text;
        }

        private void acceptClient() {
            while (true) {
                lock ("lock1") {
                Socket client=socket.Accept();
                    showLog("接收到客户端"+client.RemoteEndPoint.ToString()
                        );
                byte[] buffer = new byte[1024];
                int length=  client.Receive(buffer);
                    
                String cmd = Encoding.UTF8.GetString(buffer,0,length);
                    if (length>0) {
                        showLog("接收到消息" + cmd);
                        
                    }
                
                if (cmd.ToUpper().Contains("XXYY")) {
                    grabOne();//拍照
                    
                    sourceImage = new Image<Bgra, byte>(Application.StartupPath + "\\test.bmp");
                    imageBox1.Image = sourceImage;
                    if (File.Exists(Application.StartupPath + "\\test.bmp"))
                    {
                        File.Delete(Application.StartupPath + "\\test.bmp");
                    }
                    string response = Mark();
                    client.Send(Encoding.ASCII.GetBytes(response));
                        showLog("回复"+response);
                  }
                }
            }
        }
        Basler.Pylon.Camera camera;
        private string mark() {
            try
            {
                camera = new Camera();
                camera.Open();
                // camera.StreamGrabber.Start();
                grabOne();
            }
            catch {
                MessageBox.Show("相机不可用");
            }
            return "0,0,0";
        }
        private void showLog() {

        }
        private void grabOne() {
            var result = camera.StreamGrabber.GrabOne(200);
            ImagePersistence.Save(ImageFileFormat.Bmp, Application.StartupPath + "\\test.bmp", result);
        }
        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            if (e.GrabResult.GrabSucceeded) {
              var result=  camera.StreamGrabber.RetrieveResult(3000, TimeoutHandling.Return);
              byte[] buffer=  result.PixelData as byte[];
                ImagePersistence.Save(ImageFileFormat.Bmp, Application.StartupPath + "\\test.bmp", result);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            mark();
            showLog("相机打开成功");
        }

        private void button6_Click(object sender, EventArgs e)
        {

            grabOne();
          sourceImage = new Image<Bgra, byte>(Application.StartupPath+"\\test.bmp");
            imageBox1.Image = sourceImage;
            //CvInvoke.Line(imageBox1.Image,new Point(imageBox1.Image.Bitmap.Width/2,0),new Point(imageBox1.Image.Bitmap.Width/2,imageBox1.Image.Bitmap.Height),new MCvScalar(0,0,255,100),2, Emgu.CV.CvEnum.LineType.AntiAlias);
           // CvInvoke.Line(imageBox1.Image,new Point(0,imageBox1.Image.Bitmap.Height/2),new Point(imageBox1.Image.Bitmap.Width,imageBox1.Image.Bitmap.Height/2),new MCvScalar(0,0,255,100),2, Emgu.CV.CvEnum.LineType.AntiAlias);
            if (File.Exists(Application.StartupPath+"\\test.bmp")) {
                File.Delete(Application.StartupPath + "\\test.bmp");
            }
            showLog("拍照成功");

        }
        public void stop() {
            camera.StreamGrabber.Stop();
            camera.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // stop();
            live = false;
        }
        bool live = false;
        Thread liveThread = null;
        private void liveDisplay() {
            try
            {
                while (live)
                {
                    grabOne();
                    sourceImage = new Image<Bgra, byte>(Application.StartupPath + "\\test.bmp");
                    imageBox1.Image = sourceImage;
                    CvInvoke.Line(imageBox1.Image, new Point(imageBox1.Image.Bitmap.Width / 2, 0), new Point(imageBox1.Image.Bitmap.Width / 2, imageBox1.Image.Bitmap.Height), new MCvScalar(0, 0, 255, 100), 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                    CvInvoke.Line(imageBox1.Image, new Point(0, imageBox1.Image.Bitmap.Height / 2), new Point(imageBox1.Image.Bitmap.Width, imageBox1.Image.Bitmap.Height / 2), new MCvScalar(0, 0, 255, 100), 2, Emgu.CV.CvEnum.LineType.AntiAlias);
                    if (File.Exists(Application.StartupPath + "\\test.bmp"))
                    {
                        File.Delete(Application.StartupPath + "\\test.bmp");
                    }
                    //  Thread.Sleep(10);
                }
            }
            catch {
                showLog("连接中断，尝试重新连接");
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            loadHomograph();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            liveThread = new Thread(liveDisplay);
            live = true;
            liveThread.IsBackground = true;
            liveThread.Start();
           
            
            showLog("开始实时显示...");
        }
        private RotatedRect find_marker(Image<Gray,byte> image) {
            var bluredImage = image.SmoothGaussian(5,5,0,0);
            var canniedImage = bluredImage.Canny(35, 125);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            double maxArea = -1;
            int maxIndex = -1;
            for (int i=0;i<contours.Size;i++) {
                if (CvInvoke.ContourArea(contours[i])>maxArea) {
                    maxArea = CvInvoke.ContourArea(contours[i]);
                    maxIndex = i; 
                }
            }
           return CvInvoke.MinAreaRect(contours[maxIndex]);
        }
    }
}
