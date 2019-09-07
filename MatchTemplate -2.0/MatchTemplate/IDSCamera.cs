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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class IDSCamera : Form
    {
        ImageBox imageBox = null;
        Image<Bgr, byte> currentFrame;
        private uEye.Camera Camera;
        IntPtr displayHandle = IntPtr.Zero;
        private bool bLive = false;
        private List<MarkConfig> markerList=new List<MarkConfig>();
        public IDSCamera()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            panel1.Controls.Add(imageBox);

            imageBox.Show();
            //  displayHandle = imageBox.Image.Ptr;
            InitCamera();
            loadMarkConfig();
           // grabOne();
        }
        Mat distof = new Mat();
        Mat intrict = new Mat();

        /// <summary>
        /// 加载相机内参
        /// </summary>
        private void loadCameraIntrist()
        {
            Image<Gray, float> dis = new Image<Gray, float>(1, 5);
            Image<Gray, float> intr = new Image<Gray, float>(3, 3);
            StreamReader reader = new StreamReader(Application.StartupPath + "\\CameraParameters.txt");

            for (int i = 0; i < 3; i++)
            {
                string[] abc = reader.ReadLine().Split(',');
                intr.Data[i, 0, 0] = Convert.ToSingle(abc[0]);
                intr.Data[i, 1, 0] = Convert.ToSingle(abc[1]);
                intr.Data[i, 2, 0] = Convert.ToSingle(abc[2]);

            }
            for (int i = 0; i < 5; i++)
            {
                string str = reader.ReadLine();
                dis.Data[i, 0, 0] = Convert.ToSingle(str);
            }
            distof = dis.Mat.Clone();
            intrict = intr.Mat.Clone();
            reader.Close();
            showLog("相机内参加载完成");
        }
        /// <summary>
        /// 初始化相机
        /// </summary>
        private void InitCamera()
        {
            Camera = new uEye.Camera();
            uEye.Defines.Status statusRet = 0;
            statusRet = Camera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

           
            Camera.Trigger.Set(uEye.Defines.TriggerMode.Software);

         //   statusRet = Camera.Focus.Auto.SetEnable(true);
            if (statusRet== uEye.Defines.Status.Success) {
                showLog("自动对焦成功");
            }
            uEye.Types.ImageFormatInfo[] list;
            Camera.Size.ImageFormat.GetList(out list);
            Camera.PixelFormat.Set(uEye.Defines.ColorMode.BGR8Packed);
            uint formatId = (uint)list[0].FormatID;
            Camera.Size.ImageFormat.Set(formatId);
            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }
         
            Camera.EventFrame += onFrameEvent;
            Camera.EventAutoBrightnessFinished += onAutoShutterFinished;
            Camera.AutoFeatures.Software.WhiteBalance.Enabled = false;
            statusRet=    Camera.AutoFeatures.Sensor.Gain.SetEnable(false);
          //  Camera.AutoFeatures.Software.Gain.Enabled = false;
            statusRet = Camera.AutoFeatures.Sensor.GainShutter.SetEnable(false);
    
        
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("自动曝光设置关闭失败");
                Environment.Exit(-1);
            }

            statusRet = Camera.Timing.Exposure.Set(32);
            
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("曝光设置失败");
                Environment.Exit(-1);
            }
        }

        private void Camera_EventDeviceRemove(object sender, EventArgs e)
        {
            MessageBox.Show("相机已经断开....");
        }

        private void Camera_EventDeviceUnPlugged(object sender, EventArgs e)
        {
            MessageBox.Show("相机已经断开....");
        }



        /// <summary>
        /// 计算非环性
        /// </summary>
        /// <param name="p"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public double calArc(double p, double area)
        {
            return Math.Abs(1 - (p * p / area) / (4 * Math.PI));
        }

       
        private int[] findTop3()
        {
            //int maxContour = -1;
            int index1 = -1, index2 = -1, index3 = -1;
            double maxP = -1;
            int[] indexs = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < contours.Size; j++)
                    {
                        double tempp = CvInvoke.ContourArea(contours[j], true);
                        if (tempp > maxP)
                        {
                            maxP = tempp;
                            index1 = j;
                        }
                    }
                }
                else if (i == 1)
                {
                    maxP = -1;
                    for (int j = 0; j < contours.Size; j++)
                    {
                        if (j == index1)
                        {
                            continue;
                        }
                        double tempp = CvInvoke.ContourArea(contours[j], true);
                        if (tempp > maxP)
                        {
                            maxP = tempp;
                            index2 = j;
                        }
                    }
                }
                else
                {
                    maxP = -1;
                    for (int j = 0; j < contours.Size; j++)
                    {
                        if (j == index1 || j == index2)
                        {
                            continue;
                        }
                        double tempp = CvInvoke.ContourArea(contours[j], true);
                        if (tempp > maxP)
                        {
                            maxP = tempp;
                            index3 = j;
                        }
                    }
                }
            }
            indexs[0] = index1;
            indexs[1] = index2;
            indexs[2] = index3;
            return indexs;
        }
        double liveDistance = 0;

        /// <summary>
        /// 图像采集到的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onFrameEvent(object sender, EventArgs e)
        {
            GC.Collect();
            uEye.Camera Camera = sender as uEye.Camera;

            Int32 s32MemID;
            int width = 0;
            int height = 0;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.GetSize(s32MemID, out width, out height);

            Bitmap bitmap;
            Camera.Memory.ToBitmap(s32MemID, out bitmap);

            currentFrame = new Image<Bgr, byte>(bitmap);
            var copy = currentFrame.Copy();

            CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            imageBox.Image = currentFrame;
            //var contour = find_marker2(currentFrame.Convert<Gray, byte>());

            //if (contour != -1)
            //{
            //    CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
            //    var rect = CvInvoke.MinAreaRect(contours[contour]);

            //    pixelPerimter = CvInvoke.ArcLength(contours[contour], true);
            //    liveDistance = realPerimter * f / pixelPerimter;
            //    CvInvoke.PutText(currentFrame, "distance:" + liveDistance + " pixelPerimeter:" + pixelPerimter, new Point(50, 50), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 255, 0, 100), 2
            //    );

            //}

            //  imageBox.Image = currentFrame;
        }

        /// <summary>
        /// 画旋转矩形
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rotatedRect"></param>
        public void drawRotatedRectangle(Image<Bgr, byte> image, RotatedRect rotatedRect)
        {
            PointF[] points = rotatedRect.GetVertices();
            for (int i = 0; i < 4; i++)
            {
                if (i == 3)
                {
                    CvInvoke.Line(image, new Point((int)points[3].X, (int)points[3].Y), new Point((int)points[0].X, (int)points[0].Y), new MCvScalar(0, 255, 0, 100), 2);
                    return;
                }
                CvInvoke.Line(image, new Point((int)points[i].X, (int)points[i].Y), new Point((int)points[i + 1].X, (int)points[i + 1].Y), new MCvScalar(0, 255, 0, 100), 2);
            }
        }

        private void onAutoShutterFinished(object sender, EventArgs e)
        {
            MessageBox.Show("AutoShutter finished...");
        }

        /// <summary>
        /// 开始实时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Camera.Focus.Auto.SetEnable(true);
            showLog("开始实时显示");
            imageBox.Image = null;
            Camera.Trigger.Set(uEye.Defines.TriggerMode.Continuous);

            uEye.Types.ImageFormatInfo[] FormatInfoList;
            Camera.Size.ImageFormat.GetList(out FormatInfoList);

            int count = FormatInfoList.Count();

            Camera.Size.ImageFormat.Set((uint)FormatInfoList[0].FormatID);



            if (Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {
                bLive = true;
            }

        }

        /// <summary>
        /// 停止实时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            showLog("停止实时显示");
            if (Camera.Acquisition.Stop() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        double p34 = 0;
        double f = 1933.46875;
        double knownWidth = 42.6;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Capture_ImageGrabbed(object sender, EventArgs e)
        //{
        //    //RotatedRect marker = find_marker(currentFrame.Convert<Gray, byte>());

        //    //PointF[] points = marker.GetVertices();
        //    //for (int i = 0; i < 4; i++)
        //    //{
        //    //    CvInvoke.Circle(currentFrame, new Point((int)points[i].X, (int)points[i].Y), 3, new MCvScalar(0, 255, 0, 100), 2);
        //    //    CvInvoke.PutText(currentFrame, "P" + (i + 1), new Point((int)points[i].X, (int)points[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
        //    //    if (i == 3)
        //    //    {

        //    //        CvInvoke.Line(currentFrame, new Point((int)points[3].X, (int)points[3].Y), new Point((int)points[0].X, (int)points[0].Y), new MCvScalar(0, 255, 0, 100));
        //    //        break;
        //    //    }
        //    //    CvInvoke.Line(currentFrame, new Point((int)points[i].X, (int)points[i].Y), new Point((int)points[i + 1].X, (int)points[i + 1].Y), new MCvScalar(0, 255, 0, 100), 2);


        //    //}
        //    var contour = find_marker2(currentFrame.Convert<Gray, byte>());
        //    CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 2);
        //    p34 = CvInvoke.ArcLength(contours[contour], true);
        //    //  p34 = Math.Pow(Math.Pow((points[2].X - points[3].X), 2) + Math.Pow(points[2].Y - points[3].Y, 2), 0.5);

        //    double dis = f * knownWidth / p34;
        //    CvInvoke.PutText(currentFrame, "distance:" + dis.ToString(), new Point(50, 50), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
        //    imageBox.Image = currentFrame;
        //}
        VectorOfVectorOfPoint contours;//轮廓


        private RotatedRect find_marker(Image<Gray, byte> image)
        {

            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);
            var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = thresholdImage.Canny(35, 200);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            double maxArea = -1;
            int maxIndex = -1;
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i]) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(contours[i]);
                    maxIndex = i;
                }
            }
            if (maxIndex == -1)
            {
                return new RotatedRect();
            }
            return CvInvoke.MinAreaRect(contours[maxIndex]);
        }

        private void findMark(Image<Gray, byte> image)
        {
            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);
            var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = thresholdImage.Canny(35, 200);

            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

        }
        private int find_marker2(Image<Gray, byte> image)
        {

            var thresholdImage = new Image<Gray, byte>(image.Size);
            CvInvoke.Threshold(image, thresholdImage, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);


            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(thresholdImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            double maxArea = -1;
            int maxIndex = -1;
            double score = 10;

            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                var rect = CvInvoke.MinAreaRect(contours[i]);
                double areaRatio = area / (rect.Size.Width * rect.Size.Height);
                double k = rect.Size.Width > rect.Size.Height ? rect.Size.Height / rect.Size.Width : rect.Size.Width / rect.Size.Height;
                if (areaRatio < 0.94)
                {
                    continue;
                }
                if (k < 0.95)
                {
                    continue;
                }
                //if (rect.Size.Width > image.Width / 4 || rect.Size.Height > image.Height / 4)
                //{
                //    continue;
                //}
                if (area < 200)
                {
                    continue;
                }
                if (CvInvoke.ContourArea(contours[i]) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(contours[i]);
                    maxIndex = i;
                }
            }

            return maxIndex;
        }
        VectorOfVectorOfPoint modelContours;
        private int find_marker2_model(Image<Gray, byte> image)
        {
            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);
            var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = thresholdImage.Canny(35, 200);

            modelContours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, modelContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            double maxArea = -1;
            int maxIndex = -1;

            for (int i = 0; i < modelContours.Size; i++)
            {
                if (CvInvoke.ContourArea(modelContours[i]) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(modelContours[i]);
                    maxIndex = i;
                }
            }
            if (maxIndex == -1)
            {
                return -1;
            }
            return maxIndex;
        }
        private int find_marker3(Image<Gray, byte> image)
        {
            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);
            var thresholdImage = bluredImage.InRange(new Gray(180), new Gray(255));
            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = thresholdImage.Canny(35, 200);

            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            double maxArea = -1;
            int maxIndex = -1;

            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                var rect = CvInvoke.MinAreaRect(contours[i]);
                double areaRatio = area / (rect.Size.Width * rect.Size.Height);
                double k = rect.Size.Width > rect.Size.Height ? rect.Size.Height / rect.Size.Width : rect.Size.Width / rect.Size.Height;
                if (areaRatio < 0.87)
                {
                    continue;
                }
                if (k < 0.86)
                {
                    continue;
                }
                if (rect.Size.Width > image.Width / 4 || rect.Size.Height > image.Height / 4)
                {
                    continue;
                }

                if (CvInvoke.ContourArea(contours[i]) > maxArea)
                {
                    maxArea = CvInvoke.ContourArea(contours[i]);
                    maxIndex = i;
                }
            }
            if (maxIndex == -1)
            {
                return -1;
            }
            return maxIndex;
        }
        List<int> list = new List<int>();
        private List<int> findCircle(Image<Gray, byte> image)
        {
            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);
            var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            List<int> maxs = new List<int>();
            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = thresholdImage.Canny(35, 200);
            contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(canniedImage, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            if (contours.Size < 3)
            {
                return null;
            }
            for (int i = 0; i < contours.Size; i++)
            {
                if (arc(contours[i]) < 0.1)
                {
                    maxs.Add(i);
                }
            }

            return maxs;


        }
        public double arc(VectorOfPoint contour)
        {
            double length = CvInvoke.ArcLength(contour, true);
            double are = CvInvoke.ContourArea(contour);

            return length / are;
        }
        public string cal_Distance()
        {

            return "";
        }
        Socket socket;
        bool isListenning = false;
        Thread watchT = null;
        private void button6_Click(object sender, EventArgs e)
        {
            if (isListenning) {
                showLog("已开始监听");
                return;
            }
            IPEndPoint end = new IPEndPoint(IPAddress.Parse("192.168.1.30"), 10004);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(end);
            socket.Listen(1000);
            isListenning = true;
            watchT = new Thread(accpetClient);
            watchT.IsBackground = true;
            watchT.Start();
            showLog("已经开始监听------");
        }
        /// <summary>
        /// 显示日志
        /// </summary>
        /// <param name="text"></param>
        private void showLog(string text)
        {
            if (richTextBox1.Lines.Length > 100)
            {
                richTextBox1.Clear();
            }
            this.richTextBox1.Text += "\r\n" + text;
        }
        /// <summary>
        /// 接收客户端线程
        /// </summary>
        private void accpetClient()
        {
            while (isListenning)
            {
                Socket client = socket.Accept();

                showLog("接受到客户端的连接");
                Thread response = new Thread(handler);
                response.IsBackground = true;
                response.Start(client);
            }
        }
        private double realDistance;
        private double realPerimter = 1000;
        private double pixelPerimter;
        PointF[] vetices = new PointF[4];
        private int runTime = 0;
        public void sortPoints()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i; j < 4; j++)
                {
                    if (vetices[i].X > vetices[j].X)
                    {
                        PointF tempPoint = vetices[i];
                        vetices[i] = vetices[j];
                        vetices[j] = tempPoint;
                    }
                }
            }
        }
        private void handler(object client)
        {
            Socket soc = client as Socket;
            while (isListenning)
            {
                byte[] buffer = new byte[1024];
                int length = soc.Receive(buffer);
                string mes = Encoding.UTF8.GetString(buffer, 0, length);


                lock ("lock1")
                {
                    if (mes.Length > 0)
                    {
                        showLog("收到命令" + mes);
                    }
                    else
                    {
                        //MessageBox.Show("套接字断开");
                        return;
                    }
                    if (mes.ToUpper().Contains("Z"))
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int count = 0;
                        while (count < 5) {

                            grabOne(5);




                            var contour = find_marker2(currentFrame.Convert<Gray, byte>());
                            string result = String.Empty;
                            if (contour != -1)
                            {
                                CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                                var rect = CvInvoke.MinAreaRect(contours[contour]);

                                vetices = rect.GetVertices();
                                for (int i = 0; i < 4; i++)
                                {
                                    CvInvoke.Circle(imageBox.Image, new Point((int)vetices[i].X, (int)vetices[i].Y), 2, new MCvScalar(0, 0, 255, 100), 2);
                                    CvInvoke.PutText(imageBox.Image, "P" + i, new Point((int)vetices[i].X, (int)vetices[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 2, new MCvScalar(0, 0, 255, 100), 2);
                                    imageBox.Image = imageBox.Image;
                                }
                                pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
                                // liveDistance = realPerimter * f / pixelPerimter;
                                int bestIndex = findNeast(pixelPerimter, new List<double>(perimerterList));
                                if (bestIndex == -1)
                                {
                                    liveDistance = myCalculate(pixelPerimter);

                                }
                                else
                                {
                                    if (exceptionIndexes.Contains(bestIndex))
                                    {
                                        liveDistance = (distanceList[bestIndex - 1] + distanceList[bestIndex + 1]) / 2;
                                    }
                                    else
                                    {
                                        liveDistance = distanceList[bestIndex];
                                    }
                                }
                                CvInvoke.PutText(currentFrame, "distance:" + liveDistance + " pixelPerimeter:" + pixelPerimter, new Point(100, 280), Emgu.CV.CvEnum.FontFace.HersheyComplex, 3, new MCvScalar(0, 255, 0, 100), 3
                                );

                                double k = liveDistance / 286.53;

                                var pointF = CvInvoke.PerspectiveTransform(new PointF[] { rect.Center }, homograph);
                                pointF[0].X = (float)k * pointF[0].X;
                                pointF[0].Y = (float)k * pointF[0].Y;

                                string dis = string.Format("{0:+00000.000}", liveDistance);
                                string a, b;
                                if (pointF[0].X >= 0)
                                {
                                    a = string.Format("{0:+00000.000}", pointF[0].X);
                                }
                                else
                                {
                                    a = string.Format("{0:00000.000}", pointF[0].X);
                                }
                                if (pointF[0].Y >= 0)
                                {
                                    b = string.Format("{0:+00000.000}", pointF[0].Y);
                                }
                                else
                                {
                                    b = string.Format("{0:00000.000}", pointF[0].Y);
                                }

                                imageBox.Image = imageBox.Image;
                                result = dis + "," + a + "," + b;
                                showLog("Position:" + dis + "," + a + "," + b);
                                // return "1," + a + "," + b + "," + c;
                                soc.Send(Encoding.ASCII.GetBytes(dis + "," + a + "," + b));

                                sw.Stop();
                                showLog("通信耗时" + sw.ElapsedMilliseconds);
                                showLog("回复:" + result);
                                StreamWriter writer = new StreamWriter(Application.StartupPath + "\\record.txt", true);
                                writer.WriteLine(mes + ": Run Time: " + sw.ElapsedMilliseconds + "ms Result:" + result);
                                writer.Flush();
                                writer.Close();

                                break;

                            }
                            else {
                                count++;

                                if (count >= 3) {
                                    soc.Send(Encoding.ASCII.GetBytes("No Found"));
                                    showLog("No found");
                                    break;

                                }
                            }

                        }



                    }
                    else if (mes.ToUpper().Contains("L")) {

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        int count = 0;
                        while (count < 5)
                        {

                            grabOne("abc");




                            var contour = find_marker2(currentFrame.Convert<Gray, byte>());
                            string result = String.Empty;
                            if (contour != -1)
                            {
                                CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                                var rect = CvInvoke.MinAreaRect(contours[contour]);

                                vetices = rect.GetVertices();
                                for (int i = 0; i < 4; i++)
                                {
                                    CvInvoke.Circle(imageBox.Image, new Point((int)vetices[i].X, (int)vetices[i].Y), 2, new MCvScalar(0, 0, 255, 100), 2);
                                    CvInvoke.PutText(imageBox.Image, "P" + i, new Point((int)vetices[i].X, (int)vetices[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 2, new MCvScalar(0, 0, 255, 100), 2);
                                    imageBox.Image = imageBox.Image;
                                }
                                pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
                                // liveDistance = realPerimter * f / pixelPerimter;
                                int bestIndex = findNeast(pixelPerimter, new List<double>(perimerterList));
                                if (bestIndex == -1)
                                {
                                    liveDistance = myCalculate(pixelPerimter);

                                }
                                else
                                {
                                    if (exceptionIndexes.Contains(bestIndex))
                                    {
                                        liveDistance = (distanceList[bestIndex - 1] + distanceList[bestIndex + 1]) / 2;
                                    }
                                    else
                                    {
                                        liveDistance = distanceList[bestIndex];
                                    }
                                }
                                CvInvoke.PutText(currentFrame, "distance:" + liveDistance + " pixelPerimeter:" + pixelPerimter, new Point(100, 280), Emgu.CV.CvEnum.FontFace.HersheyComplex, 3, new MCvScalar(0, 255, 0, 100), 3
                                );

                                double k = liveDistance / 286.53;

                                var pointF = CvInvoke.PerspectiveTransform(new PointF[] { rect.Center }, homograph);
                                pointF[0].X = (float)k * pointF[0].X;
                                pointF[0].Y = (float)k * pointF[0].Y;

                                string dis = string.Format("{0:+00000.000}", liveDistance);
                                string a, b;
                                if (pointF[0].X >= 0)
                                {
                                    a = string.Format("{0:+00000.000}", pointF[0].X);
                                }
                                else
                                {
                                    a = string.Format("{0:00000.000}", pointF[0].X);
                                }
                                if (pointF[0].Y >= 0)
                                {
                                    b = string.Format("{0:+00000.000}", pointF[0].Y);
                                }
                                else
                                {
                                    b = string.Format("{0:00000.000}", pointF[0].Y);
                                }

                                imageBox.Image = imageBox.Image;
                                result = dis + "," + a + "," + b;
                                showLog("Position:" + dis + "," + a + "," + b);
                                // return "1," + a + "," + b + "," + c;
                                soc.Send(Encoding.ASCII.GetBytes(dis + "," + a + "," + b));

                                sw.Stop();
                                showLog("通信耗时" + sw.ElapsedMilliseconds);
                                showLog("回复:" + result);
                                StreamWriter writer = new StreamWriter(Application.StartupPath + "\\record.txt", true);
                                writer.WriteLine(mes + ": Run Time: " + sw.ElapsedMilliseconds + "ms Result:" + result);
                                writer.Flush();
                                writer.Close();

                                break;

                            }
                            else
                            {
                                count++;

                                if (count >= 3)
                                {
                                    soc.Send(Encoding.ASCII.GetBytes("No Found"));
                                    showLog("No found");
                                    break;

                                }
                            }

                        }



                    }
                    else if (mes.ToUpper().Contains("F"))
                    {

                        realDistance = Convert.ToDouble(mes.Remove(0, 1));

                        label5.Text = "标定高度:" + realDistance.ToString();
                        button5_Click(null, null);
                        int contour = find_marker2(currentFrame.Convert<Gray, byte>());
                        CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 0, 255, 100), 2);
                        imageBox.Image = currentFrame;
                        Thread.Sleep(20);
                        pixelPerimter = CvInvoke.ArcLength(contours[contour], true);
                        textBox2.Text = pixelPerimter.ToString();
                        textBox3.Text = realDistance.ToString();
                        button8_Click(null, null);
                        button3_Click(null, null);
                        soc.Send(Encoding.ASCII.GetBytes("Mark OK！"));

                    }
                    else if (mes.ToUpper().Contains("XXYY"))
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        string result = String.Empty;
                        if (mes.Length >= 5)
                        {

                            string num = mes.Substring(4, 1);
                            if (num == "5")
                            {
                                grabOne(5);
                                var contour = find_marker2(currentFrame.Convert<Gray, byte>());

                                if (contour != -1)
                                {
                                    CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                                    var rect = CvInvoke.MinAreaRect(contours[contour]);
                                    vetices = rect.GetVertices();
                                    for (int i = 0; i < 4; i++)
                                    {
                                        CvInvoke.Circle(imageBox.Image, new Point((int)vetices[i].X, (int)vetices[i].Y), 2, new MCvScalar(0, 0, 255, 100), 2);
                                        CvInvoke.PutText(imageBox.Image, "P" + i, new Point((int)vetices[i].X, (int)vetices[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 2, new MCvScalar(0, 0, 255, 100), 2);
                                        imageBox.Image = imageBox.Image;
                                    }
                                    pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
                                    // liveDistance = realPerimter * f / pixelPerimter;
                                    int bestIndex = findNeast(pixelPerimter, new List<double>(perimerterList));
                                    if (bestIndex == -1)
                                    {
                                        liveDistance = myCalculate(pixelPerimter);

                                    }
                                    else
                                    {
                                        if (exceptionIndexes.Contains(bestIndex))
                                        {
                                            liveDistance = (distanceList[bestIndex - 1] + distanceList[bestIndex + 1]) / 2;
                                        }
                                        else
                                        {
                                            liveDistance = distanceList[bestIndex];
                                        }
                                    }
                                    CvInvoke.PutText(currentFrame, "distance:" + liveDistance + " pixelPerimeter:" + pixelPerimter, new Point(100, 280), Emgu.CV.CvEnum.FontFace.HersheyComplex, 3, new MCvScalar(0, 255, 0, 100), 3
                                    );


                                    var pointF = CvInvoke.PerspectiveTransform(new PointF[] { rect.Center }, homograph);
                                    pointF[0].X = pointF[0].X;
                                    pointF[0].Y = pointF[0].Y;
                                    sortPoints();//排序点

                                    float LeftPointX = (vetices[0].X + vetices[1].X) / 2;
                                    float leftPointY = (vetices[0].Y + vetices[1].Y) / 2;
                                    float rightPointX = (vetices[2].X + vetices[3].X) / 2;
                                    float rightPointY = (vetices[2].Y + vetices[3].Y) / 2;
                                    CvInvoke.Line(currentFrame, new Point((int)LeftPointX, (int)leftPointY), new Point((int)rightPointX, (int)rightPointY), new MCvScalar(0, 0, 255, 100), 2);
                                    double angle = (Math.Atan((rightPointY - leftPointY) / (rightPointX - LeftPointX)) / Math.PI) * 180; ;

                                    string dis = string.Format("{0:+00000.000}", liveDistance);
                                    string a, b, c;
                                    if (pointF[0].X >= 0)
                                    {
                                        a = string.Format("{0:+00000.000}", pointF[0].X);
                                    }
                                    else
                                    {
                                        a = string.Format("{0:00000.000}", pointF[0].X);
                                    }
                                    if (pointF[0].Y >= 0)
                                    {
                                        b = string.Format("{0:+00000.000}", pointF[0].Y);
                                    }
                                    else
                                    {
                                        b = string.Format("{0:00000.000}", pointF[0].Y);
                                    }
                                    if (angle > 0)
                                    {
                                        c = string.Format("{0:+00000.000}", angle);
                                    }
                                    else
                                    {
                                        c = string.Format("{0:00000.000}", angle);
                                    }

                                    imageBox.Image = currentFrame;
                                    showLog("Position:" + a + "," + b + "," + c);
                                    result = "1," + a + "," + b + "," + c;
                                    // return "1," + a + "," + b + "," + c;
                                    soc.Send(Encoding.ASCII.GetBytes(result));
                                }
                                else
                                {
                                    result = "No Found";
                                    soc.Send(Encoding.ASCII.GetBytes(result));
                                }
                                //      string distance = liveDistance.ToString();
                                // string str = string.Format(distance, "{0:0.000}");
                                showLog("回复:" + result);
                                imageBox.Image = currentFrame;
                            }//测Z
                            else {
                                grabOne(markerList[Convert.ToInt32(num) - 1].Exposure, Convert.ToUInt32(markerList[Convert.ToInt32(num) - 1].Focus));
                                num = Application.StartupPath + "\\Mark\\Mark" + num + ".jpg";
                                Image<Gray, byte> modelImage = new Image<Gray, byte>(num);

                                result = Mark(currentFrame.Convert<Gray, byte>(), modelImage);
                                if (result == "+00000.000,+00000.000,+00000.000,+00000.000")
                                {
                                    int count = 0;
                                    while (count < 3)
                                    {
                                        count++;
                                        grabOne((double)numericUpDown3.Value, (uint)numericUpDown5.Value);
                                        result = Mark(currentFrame.Convert<Gray, byte>(), modelImage);
                                        if (result != "+00000.000,+00000.000,+00000.000,+00000.000")
                                        {
                                            break;
                                        }

                                    }
                                }
                                imageBox.Show();
                                soc.Send(Encoding.ASCII.GetBytes(result));//普通XXYY
                            }
                        }
                        sw.Stop();
                        showLog("通信耗时" + sw.ElapsedMilliseconds);
                        StreamWriter writer = new StreamWriter(Application.StartupPath + "\\record.txt", true);
                        writer.WriteLine(mes + ": Run Time: " + sw.ElapsedMilliseconds + "ms Result:" + result);
                        writer.Flush();
                        writer.Close();
                    }
                    else if (mes.ToUpper().Contains("N"))
                    {
                        if (runTime < 1000)
                        {


                            runTime++;
                            string runStr = string.Format("{0:+00000.000}", runTime * 0.8);
                            string cmd = "Move,(+00000.000,+00000.000," + runStr + ")";
                            soc.Send(Encoding.ASCII.GetBytes(cmd));
                            showLog("发送给客户端:" + cmd);
                        }
                        else
                        {
                            showLog("已经完成深度位置记录");
                        }
                    }
                    else if (mes.ToUpper().Contains("OK"))
                    {
                        string[] strs = mes.Split(',');
                        double currentHeight = Convert.ToDouble(strs[1]) - 69.22;
                        grabOne();
                        var contour = find_marker2(currentFrame.Convert<Gray, byte>());
                        if (contour != -1)
                        {
                            CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                            var rect = CvInvoke.MinAreaRect(contours[contour]);
                            pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
                            StreamWriter writer = new StreamWriter(Application.StartupPath + "\\Run.txt", true);
                            writer.WriteLine(pixelPerimter.ToString() + "," + currentHeight.ToString());
                            writer.Flush();
                            writer.Close();
                            showLog("写入数据" + pixelPerimter.ToString() + "," + currentHeight.ToString());
                        }
                        showLog("完成一次移动数据记录");
                        soc.Send(Encoding.ASCII.GetBytes("HAO"));
                    }
                    else
                    {

                        soc.Send(Encoding.ASCII.GetBytes("UnkownCommand"));
                    }

                }
            }

        }
        private void moniterFocus() {
            while (true) {
                uint f = 0;
                Camera.Focus.Manual.Get(out f);
                label12.Text = "F:"+f;
                Thread.Sleep(20);
            }
        }
        /// <summary>
        /// 加载拍照参数
        /// </summary>
        private void loadMarkConfig() {
            StreamReader reader = new StreamReader(Application.StartupPath+"\\Config.txt");
            while (true) {
                string entry = reader.ReadLine();
                if (entry==null) {
                    break;
                }
                string[] strs = entry.Split(',');
                MarkConfig mc = new MarkConfig();
                mc.MarkName = strs[0];
                mc.Exposure = Convert.ToDouble(strs[1]);
                mc.Focus = strs[2];
                mc.Delay = Convert.ToInt32(strs[3]);
                markerList.Add(mc);

            }
            reader.Close();
            showLog("加载Marker配置成功");
            
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
        PointF[] rawPoints = new PointF[4];
        PointF[] correctPoints = new PointF[4];
        Mat homograph;
        private void loadHomograph()
        {

            StreamReader streamReader = new StreamReader(Application.StartupPath + "\\Verification.txt");
            for (int i = 0; i < 4; i++)
            {
                string pointpair = streamReader.ReadLine();
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
        Point defaultPoint = new Point(100, 300);
        public string Mark(Image<Gray, byte> sourceImage, Image<Gray, byte> modelImage, int x = 100, int y = 300)
        {

            Image<Gray, byte> sourceImage1 = sourceImage.Copy();
            Image<Gray, byte> modelImage1 = modelImage.Copy();
            sourceImage1 = sourceImage1.PyrDown().PyrDown().PyrDown();
            modelImage1 = modelImage1.PyrDown().PyrDown().PyrDown();
            bool isGoodMatch = false;
            double left = (double)this.numericUpDown1.Value;
            double right = (double)numericUpDown2.Value;
            double acc = 0.1;
            double angle = MyApproch(sourceImage1, modelImage1, left, right, acc);
            double socre = Convert.ToDouble(textBox1.Text);
            var rotatedObsered = sourceImage1.Rotate(angle, new Gray(0), true);
            Mat result = new Mat();
            CvInvoke.MatchTemplate(rotatedObsered, modelImage1, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
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

            if (isGoodMatch)
            {
                maxLoc.X = maxLoc.X * 8;
                maxLoc.Y = maxLoc.Y * 8;
                Point rectOringinal = new Point();
                Point point2 = new Point(maxLoc.X + modelImage.Width, maxLoc.Y);
                Point point3 = new Point(maxLoc.X, maxLoc.Y + modelImage.Height);
                Point point4 = new Point(maxLoc.X + modelImage.Width, maxLoc.Y + modelImage.Height);
                Point rotateCenter = new Point(sourceImage.Width / 2, sourceImage.Height / 2);
                rectOringinal.X = (int)
                    (
                        (maxLoc.X - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );
                rectOringinal.Y = (int)((maxLoc.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point2.X = (int)((point2.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point2.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point2.Y = (int)((point2.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point2.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point3.X = (int)((point3.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point3.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point3.Y = (int)((point3.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point3.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                point4.X = (int)((point4.X - rotateCenter.X) * Math.Cos((-angle / 180) * Math.PI) - (point4.Y - rotateCenter.Y) * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X);
                point4.Y = (int)((point4.X - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (point4.Y - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                PointF accPoint = new PointF();
                accPoint.X =
                       Convert.ToSingle((maxLoc.X + modelImage.Width / 2 - rotateCenter.X)
                       * Math.Cos((-angle / 180) * Math.PI)
                       - (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y)
                       * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                   );
                accPoint.Y = Convert.ToSingle(((maxLoc.X + modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y));
                maxLoc.X = (int)
                    (
                        (maxLoc.X + modelImage.Width / 2 - rotateCenter.X)
                        * Math.Cos((-angle / 180) * Math.PI)
                        - (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y)
                        * Math.Sin((-angle / 180) * Math.PI) + rotateCenter.X
                    );

                maxLoc.Y = (int)((maxLoc.X + modelImage.Width / 2 - rotateCenter.X) * Math.Sin((-angle / 180) * Math.PI) + (maxLoc.Y + modelImage.Height / 2 - rotateCenter.Y) * Math.Cos((-angle / 180) * Math.PI) + rotateCenter.Y);
                Rectangle rect = new Rectangle(rectOringinal, modelImage.Size);
                sourceImage.ROI = rect;
                sourceImage.ROI = Rectangle.Empty;
                CvInvoke.Line(imageBox.Image, rectOringinal, point2, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point2, point4, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point4, point3, new MCvScalar(0, 255, 0, 100), 3);
                CvInvoke.Line(imageBox.Image, point3, rectOringinal, new MCvScalar(0, 255, 0, 100), 3);
              
                CvInvoke.PutText(imageBox.Image, "X:" + (maxLoc.X) + " Y:" + (maxLoc.Y) + " Angle:" + (-angle), new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyPlain, 5, new MCvScalar(255, 0, 0, 100), 5);
                PointF[] pointF = new PointF[1];
                pointF[0] = accPoint;
                string a, b, c;
                pointF = CvInvoke.PerspectiveTransform(pointF, homograph);
                if (pointF[0].X >= 0)
                {
                    a = string.Format("{0:+00000.000}", pointF[0].X);
                }
                else
                {
                    a = string.Format("{0:00000.000}", pointF[0].X);
                }
                if (pointF[0].Y >= 0)
                {
                    b = string.Format("{0:+00000.000}", pointF[0].Y);
                }
                else
                {
                    b = string.Format("{0:00000.000}", pointF[0].Y);
                }
                if ((-angle) >= 0)
                {
                    c = string.Format("{0:+00000.000}", (-angle));
                }
                else
                {
                    c = string.Format("{0:00000.000}", (-angle));
                }
                imageBox.Image = imageBox.Image;
                showLog("1," + a + "," + b + "," + c);
                return "1," + a + "," + b + "," + c;

            }
            else
            {
                CvInvoke.PutText(imageBox.Image, "No Found", new Point(100, 500), Emgu.CV.CvEnum.FontFace.HersheyComplex, 10, new MCvScalar(0, 0, 255, 100), 5);
                imageBox.Image = imageBox.Image;
                return "+00000.000,+00000.000,+00000.000,+00000.000";
            }


        }

        private string calculateDistance()
        {

            double distance = realPerimter * f / pixelPerimter;
            //   distance += Convert.ToDouble(textBox4.Text);
            return distance.ToString();
        }
        /// <summary>
        /// 用特定的曝光和焦距拍照
        /// </summary>
        /// <param name="expoure"></param>
        /// <param name="foucs"></param>
        private void grabOne(double expoure,uint foucs) {
            Camera.Focus.Auto.SetEnable(false);
          Camera.Focus.Manual.Set(foucs);
            Camera.Timing.Exposure.Set(expoure);
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);
            sw.Stop();
            showLog("相机拍照耗时:" + sw.ElapsedMilliseconds + "ms");
            currentFrame = new Image<Bgr, byte>(bitmap);
            var copy = currentFrame.Copy();
            CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            imageBox.Image = currentFrame;
        }

        private void grabOne()
        {
         
            Camera.Timing.Exposure.Set(markerList[3].Exposure);
            Camera.Focus.Auto.SetEnable(true);
            Camera.Focus.Trigger();
            Thread.Sleep(markerList[4].Delay);
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);
            sw.Stop();
            showLog("相机拍照耗时:" + sw.ElapsedMilliseconds + "ms");
            currentFrame = new Image<Bgr, byte>(bitmap);
            var copy = currentFrame.Copy();
            CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            imageBox.Image = currentFrame;
        }
        private void grabOne(int index) {
          //  Camera.Focus.Auto.SetEnable(true);
          //  uEye.Defines.FocusStatus fs= uEye.Defines.FocusStatus.Focusing;
          
            //while (fs!= uEye.Defines.FocusStatus.Focused) {
            //    uEye.Defines.Status foc = Camera.Focus.Auto.GetStatus(out fs);
            //    showLog("聚焦状态:"+fs.ToString());
            //}
           
            Camera.Timing.Exposure.Set(markerList[4].Exposure);
         
             Camera.Focus.Trigger();
            Thread.Sleep(markerList[4].Delay);
            uEye.Defines.Status statusRet;
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Bitmap bitmap = null;
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);
            currentFrame = new Image<Bgr, byte>(bitmap);
            var copy = currentFrame.Copy();
            CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            imageBox.Image = currentFrame;
        }
        private void grabOne(string s) {
            Camera.Focus.Manual.Set(156);
            uEye.Defines.Status statusRet;
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Bitmap bitmap = null;
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);
            currentFrame = new Image<Bgr, byte>(bitmap);
            var copy = currentFrame.Copy();
            CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            imageBox.Image = currentFrame;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            isListenning = false;
            socket.Close();
            showLog("停止监听------");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            grabOne();

            int contour = find_marker2(currentFrame.Convert<Gray, byte>());


            if (contour == -1)
            {
                showLog("Mark doesn't exist in the FOV");
                return;
            }
            CvInvoke.DrawContours(imageBox.Image, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
            var rect = CvInvoke.MinAreaRect(contours[contour]);
            // pixelPerimter = CvInvoke.ArcLength(contours[contour], true);
            pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
            //realPerimter = 1800;

            var result = MessageBox.Show("是否为要查找的轮廓？", "Yes or No?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return;
            }
            //  CvInvoke.PutText();
            textBox2.Text = pixelPerimter.ToString();
            //  modelContour = new VectorOfPoint();
            realDistance = Convert.ToDouble(textBox3.Text);
            label5.Text = realDistance.ToString();
            f = realDistance * pixelPerimter / realPerimter;
            textBox2.Text = pixelPerimter.ToString();
            StreamWriter stream = new StreamWriter(Application.StartupPath + "\\DistanceF.txt", false);
            stream.WriteLine(f.ToString());
            stream.WriteLine(realDistance.ToString());
            stream.Flush();
            stream.Close();
            showLog("计算比例系数完成 f=" + f);

            //modelContour = modelContours[contour];
            //

            // showLog("模板轮廓存储完成！");
            // textBox3.Text = realDistance.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
        //    var status = Camera.Focus.Trigger();
         //   Thread.Sleep(800);
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);
            currentFrame = new Image<Bgr, byte>(bitmap);

            //  var copy = currentFrame.Copy();
            //  CvInvoke.Undistort(copy, currentFrame, intrict, distof);
            // imageBox.Image = currentFrame;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();

        }

        Mat pointF;
        double k, b;

        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            //double y = k * Convert.ToDouble(textBox4.Text) + b;
            //showLog("线性距离:" + y);
            grabOne();
            var contour = find_marker2(currentFrame.Convert<Gray, byte>());
            string result = String.Empty;
            if (contour != -1)
            {
                CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                var rect = CvInvoke.MinAreaRect(contours[contour]);
                vetices = rect.GetVertices();
                for (int i = 0; i < 4; i++)
                {
                    CvInvoke.Circle(imageBox.Image, new Point((int)vetices[i].X, (int)vetices[i].Y), 2, new MCvScalar(0, 0, 255, 100), 2);
                    CvInvoke.PutText(imageBox.Image, "P" + i, new Point((int)vetices[i].X, (int)vetices[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 2, new MCvScalar(0, 0, 255, 100), 2);
                    imageBox.Image = imageBox.Image;
                }
                pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
                // liveDistance = realPerimter * f / pixelPerimter;
                int bestIndex = findNeast(pixelPerimter, new List<double>(perimerterList));
                if (bestIndex == -1)
                {
                    liveDistance = myCalculate(pixelPerimter);

                }
                else
                {
                    if (exceptionIndexes.Contains(bestIndex))
                    {
                        liveDistance = (distanceList[bestIndex - 1] + distanceList[bestIndex + 1]) / 2;
                    }
                    else
                    {
                        liveDistance = distanceList[bestIndex];
                    }
                }
                CvInvoke.PutText(currentFrame, "distance:" + liveDistance + " pixelPerimeter:" + pixelPerimter, new Point(100, 280), Emgu.CV.CvEnum.FontFace.HersheyComplex, 3, new MCvScalar(0, 255, 0, 100), 3
                );


                var pointF = CvInvoke.PerspectiveTransform(new PointF[] { rect.Center }, homograph);
                pointF[0].X = pointF[0].X;
                pointF[0].Y = pointF[0].Y;
                sortPoints();//排序点

                float LeftPointX = (vetices[0].X + vetices[1].X) / 2;
                float leftPointY = (vetices[0].Y+ vetices[1].Y) / 2;
                float rightPointX = (vetices[2].X + vetices[3].X) / 2;
                float rightPointY = (vetices[2].Y + vetices[3].Y) / 2;
                CvInvoke.Line(currentFrame, new Point((int)LeftPointX, (int)leftPointY), new Point((int)rightPointX, (int)rightPointY), new MCvScalar(0, 0, 255, 100), 2);
                double angle = (Math.Atan((rightPointY - leftPointY) / (rightPointX - LeftPointX)) / Math.PI) * 180; ;

                string dis = string.Format("{0:+00000.000}", liveDistance);
                string a, b, c;
                if (pointF[0].X >= 0)
                {
                    a = string.Format("{0:+00000.000}", pointF[0].X);
                }
                else
                {
                    a = string.Format("{0:00000.000}", pointF[0].X);
                }
                if (pointF[0].Y >= 0)
                {
                    b = string.Format("{0:+00000.000}", pointF[0].Y);
                }
                else
                {
                    b = string.Format("{0:00000.000}", pointF[0].Y);
                }
                if (angle > 0)
                {
                    c = string.Format("{0:+00000.000}", angle);
                }
                else
                {
                    c = string.Format("{0:00000.000}", angle);
                }

                imageBox.Image = currentFrame;
                showLog("Position:" + dis + "," + a + "," + b + "," + c);
                // return "1," + a + "," + b + "," + c;
                //soc.Send(Encoding.ASCII.GetBytes(dis + "," + a + "," + b + "," + c));
            }
            else
            {
                result = "No Found";
              //  soc.Send(Encoding.ASCII.GetBytes(result));
            }
            //      string distance = liveDistance.ToString();
            // string str = string.Format(distance, "{0:0.000}");
            showLog("回复:" + result);
            imageBox.Image = currentFrame;
        }
        private void loadPara()
        {
            StreamReader reader = new StreamReader(Application.StartupPath + "\\Run.txt");
            while (true)
            {
                string pToDis = reader.ReadLine();
                if (pToDis == null)
                {
                    break;
                }
                string[] pds = pToDis.Split(',');
                perimerterList.Add(Convert.ToDouble(pds[0]));
                distanceList.Add(Convert.ToDouble(pds[1]));
            }
            showLog("加载高度信息完毕");
        }
        List<int> exceptionIndexes = new List<int>();
        private void checkDistanceInfo() {
            for (int i=0;i<perimerterList.Count-1;i++) {
                if (perimerterList[i]<perimerterList[i+1]) {
                    exceptionIndexes.Add(i);
                }
            }

        }
        Thread moniterThread = null;
        private void IDSCamera_Load(object sender, EventArgs e)
        {
            loadPara();
            loadCameraIntrist();
            loadHomograph();
            checkDistanceInfo();
            try
            {
                StreamReader streamReader = new StreamReader(Application.StartupPath + "\\DistanceF.txt");
                f = Convert.ToDouble(streamReader.ReadLine());
                realDistance = Convert.ToDouble(streamReader.ReadLine());
                streamReader.Close();
                showLog("Load f done. f=" + f);
            }
            catch
            {
                showLog("f not found");
            }
            grabOne();
            moniterThread = new Thread(moniterFocus);
            moniterThread.IsBackground = true;
            moniterThread.Start();
        }

        private void IDSCamera_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (f==0) {
            //    return;
            //}
            //StreamWriter streamWriter = new StreamWriter(Application.StartupPath+"\\DistanceF.txt",false);
            //streamWriter.WriteLine(f.ToString());
            //streamWriter.Flush();
            //streamWriter.Close();
            //MessageBox.Show("F ratio saved successfully");
            if (moniterThread.IsAlive) {
                moniterThread.Abort();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var modelImage = new Image<Gray, byte>(Application.StartupPath + "\\Mark\\Mark1.jpg");
            Mark(currentFrame.Convert<Gray, byte>(), modelImage);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            var modelImage = new Image<Gray, byte>(Application.StartupPath + "\\Mark\\Mark2.jpg");
            Mark(currentFrame.Convert<Gray, byte>(), modelImage, 100, 500);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Camera.EventFrame -= Camera_EventFrame;
            Camera.EventFrame += Camera_EventFrame;

        }
        /// <summary>
        /// 画十字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Camera_EventFrame(object sender, EventArgs e)
        {
            Point p1 = new Point();
            Point p2 = new Point();
            Point p3 = new Point();
            Point p4 = new Point();
            p1.X = 0;
            p1.Y = imageBox.Image.Bitmap.Height / 2;
            p2.X = imageBox.Image.Bitmap.Width;
            p2.Y = imageBox.Image.Bitmap.Height / 2;
            p3.X = imageBox.Image.Bitmap.Width / 2;
            p3.Y = 0;
            p4.X = imageBox.Image.Bitmap.Width / 2;
            p4.Y = imageBox.Image.Bitmap.Height;
            CvInvoke.Line(imageBox.Image, p1, p2, new MCvScalar(0, 0, 255, 100), 1);
            CvInvoke.Line(imageBox.Image, p3, p4, new MCvScalar(0, 0, 255, 100), 1);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Camera.EventFrame -= Camera_EventFrame;
        }
        double markSize = 40.9;
        MCvPoint3D32f[] markConer = new MCvPoint3D32f[] {
            new MCvPoint3D32f(-20.45f,-20.45f,0),
            new MCvPoint3D32f(-20.45f,20.45f,0),
            new MCvPoint3D32f(20.45f,20.45f,0),
            new MCvPoint3D32f(20.45f,-20.45f,0)
        };
        private void button15_Click(object sender, EventArgs e)
        {
            grabOne();
            var contour = find_marker2(currentFrame.Convert<Gray, byte>());
            string result = String.Empty;
            if (contour != -1)
            {
                CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 1);
                var rect = CvInvoke.MinAreaRect(contours[contour]);
                vetices = rect.GetVertices();
                for (int i = 0; i < 4; i++)
                {
                    CvInvoke.Circle(currentFrame, new Point((int)vetices[i].X, (int)vetices[i].Y), 2, new MCvScalar(0, 0, 255, 100), 2);
                    CvInvoke.PutText(currentFrame, "P" + i, new Point((int)vetices[i].X, (int)vetices[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 2, new MCvScalar(0, 0, 255, 100), 2);

                }
            }
            imageBox.Image = currentFrame;
        }

        private MCvPoint3D32f codeRotateByZ(MCvPoint3D32f point, double thetaz)
        {
            MCvPoint3D32f aPoint = new MCvPoint3D32f();
            double rz = thetaz *  Math.PI / 180;
            aPoint.X = (float)(Math.Cos(rz) * point.X - Math.Sin(rz) * point.Y);
            aPoint.Y = (float)(Math.Sin(rz) * point.X + Math.Cos(rz) * point.Y);
            aPoint.Z = point.Z;
            return aPoint;
        }
        private MCvPoint3D32f codeRotateByX(MCvPoint3D32f point, double thetaz)
        {
            MCvPoint3D32f aPoint = new MCvPoint3D32f();
            double rz = thetaz * Math.PI / 180;
            aPoint.Y = (float)(Math.Cos(rz) * point.Y - Math.Sin(rz) * point.Z);
            aPoint.Z = (float)(Math.Cos(rz) * point.Z + Math.Sin(rz) * point.Y);
            aPoint.X = point.X;
         
            return aPoint;
        }
        private MCvPoint3D32f codeRotateByY(MCvPoint3D32f point, double thetaz)
        {
            MCvPoint3D32f aPoint = new MCvPoint3D32f();
            double rz = thetaz * Math.PI / 180;
            aPoint.X = (float)(Math.Cos(rz) * point.X + Math.Sin(rz) * point.Z);
            aPoint.Z = (float)(Math.Cos(rz) * point.Z - Math.Sin(rz) * point.X);
            aPoint.Y = point.Y;
            return aPoint;
        }
        private void solve()
        {
         
           
            Mat intrisic = new Mat();
            Mat dit = new Mat();

            //P1 P2 P3 P4的世界坐标
            MCvPoint3D32f[][] objectPoint = new MCvPoint3D32f[1][];
            objectPoint[0] = new MCvPoint3D32f[] {
         new MCvPoint3D32f(-20.45f,-20.45f,0),
            new MCvPoint3D32f(-20.45f,20.45f,0),
            new MCvPoint3D32f(20.45f,20.45f,0),
            new MCvPoint3D32f(20.45f,-20.45f,0)
            };

            //图像上P1 P2 P3 P4的世界坐标
            PointF[][] imagePoints = new PointF[1][];
            imagePoints[0] = vetices;

            //坐标原点，和X轴 Y轴 Z轴
            MCvPoint3D32f[] coor = new MCvPoint3D32f[] {
                 new MCvPoint3D32f(0,0,0),
            new MCvPoint3D32f(20.45f,0,0),
            new MCvPoint3D32f(0,20.45f,0),
            new MCvPoint3D32f(0,0,20.45f)
            };

            Mat rv = new Mat();
            Mat tv = new Mat();
            CvInvoke.SolvePnP(objectPoint[0], vetices, intrict, distof, rv, tv, true, Emgu.CV.CvEnum.SolvePnpMethod.Iterative);

            Image<Gray, float> tvI = tv.ToImage<Gray, float>();//拿到平移矩阵
            Image<Gray, float> rvI = rv.ToImage<Gray, float>();//拿到旋转矩阵

            Mat rotMat = new Mat();

            CvInvoke.Rodrigues(rv, rotMat); //转换成Rodrigues旋转矩阵

            Image<Gray, float> rMatI = rotMat.ToImage<Gray, float>();//拿到旋转矩阵的值
    

            double r11 = rMatI.Data[0, 0, 0];
            double r12 = rMatI.Data[0, 1, 0];
            double r13 = rMatI.Data[0, 2, 0];
            double r21 = rMatI.Data[1, 0, 0];
            double r22 = rMatI.Data[1, 1, 0];
            double r23 = rMatI.Data[1, 2, 0];
            double r31 = rMatI.Data[2, 0, 0];
            double r32 = rMatI.Data[2, 1, 0];
            double r33 = rMatI.Data[2, 2, 0];

            double thetaz = -(Math.Atan2(r21, r11) / Math.PI) * 180;
            double thetay =-( Math.Atan2(-1 * r31, Math.Sqrt(r31 * r31 + r33 * r33))/Math.PI)*180;
            double thetax = -(Math.Atan2(r32,r33)/Math.PI)*180;
            showLog("坐标系旋转角度:X:"+thetax+" Y:"+thetay+" Z:"+thetaz);
            MCvPoint3D32f cameraPoint = new MCvPoint3D32f(tvI.Data[0,0,0],tvI.Data[1,0,0],tvI.Data[2,0,0]);
            MCvPoint3D32f aPoint= codeRotateByZ(cameraPoint, thetaz);
            aPoint = codeRotateByY(aPoint,thetay);
            aPoint = codeRotateByX(aPoint,thetax);
            aPoint.X = -aPoint.X;
            aPoint.Y = -aPoint.Y;
            aPoint.Z = -aPoint.Z;
            showLog("相机在世界坐标系的坐标：X:"+aPoint.X+" Y:"+aPoint.Y+" Z:"+aPoint.Z);
            //CvInvoke.Eigen(tv, T);
            // CvInvoke.Eigen(rotMat,R);
            PointF[] imageZhou = CvInvoke.ProjectPoints(coor, rv, tv, intrict, dit);
            CvInvoke.Line(imageBox.Image, new Point((int)imageZhou[0].X, (int)imageZhou[0].Y), new Point((int)imageZhou[1].X, (int)imageZhou[1].Y), new MCvScalar(255, 0, 0, 100), 2);
            CvInvoke.Line(imageBox.Image, new Point((int)imageZhou[0].X, (int)imageZhou[0].Y), new Point((int)imageZhou[2].X, (int)imageZhou[2].Y), new MCvScalar(0, 255, 0, 100), 2);
            CvInvoke.Line(imageBox.Image, new Point((int)imageZhou[0].X, (int)imageZhou[0].Y), new Point((int)imageZhou[3].X, (int)imageZhou[3].Y), new MCvScalar(0, 0, 255, 100), 2);

            //  showLog("X:" + tvI.Data[0, 0, 0] + "Y:" + tvI.Data[1, 0, 0] + "Z:" + tvI.Data[2, 0, 0]);

            Matrix<float> ro = new Matrix<float>(3,1);
            Matrix<float> tvm = new Matrix<float>(3, 1);
            tvm[0, 0] = tvI.Data[0,0,0];
            tvm[1, 0] = tvI.Data[1,0,0];
            tvm[2, 0] = tvI.Data[2,0,0];

            CvInvoke.Eigen(rotMat, ro);
           // var a=  ro.Transpose() * tvm;
          //  showLog("转置得到的坐标: X:"+a[0,0]+" Y:"+a[1,0]+" Z:"+a[2,0]);

        }
        Socket clientSoc;
        int height = 0;
        private bool start;
        private Thread runDataThread = null;
        public void runData()
        {
            clientSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSoc.Connect("192.168.1.12", 10004);
            runDataThread = new Thread(runFunction);
            runDataThread.IsBackground = true;
            runDataThread.Start();
          

        }
        public void runFunction()
        {
            while (start)
            {
                if (height >= 569)
                {
                    break;
                }
                SendToRobert("MOVEZ", "+00001.000");
                height++;
            }
        }
        List<double> perimerterList = new List<double>();
        List<double> distanceList = new List<double>();
        private int findNeast(double p)
        {
            int index = -1;
            double margin = 100;
            for (int i = 0; i < perimerterList.Count; i++)
            {
                double temp = Math.Abs(p - perimerterList[i]);
                if (temp < margin)
                {
                    margin = temp;
                    index = i;
                }
            }

            if (margin > 90)
            {
                return -1;
            }
            return index;
        }

        /// <summary>
        /// 递归除掉异常点
        /// </summary>
        /// <param name="p"></param>
        /// <param name="perimerterList"></param>
        /// <returns></returns>
        private int findNeast(double p,List<double> perimerterList) {
            int index = -1;
            double margin = 100;
            for (int i = 0; i < perimerterList.Count; i++)
            {
                double temp = Math.Abs(p - perimerterList[i]);
                if (temp < margin)
                {
                    margin = temp;
                    index = i;
                }
            }
            if (exceptionIndexes.Contains(index)) {
                perimerterList.RemoveAt(index);
                return   findNeast(p,perimerterList);
            }
            if (margin > 90)
            {
                return -1;
            }
            return index;
        }

        private double myCalculate(double perimeter)
        {
            return 64.0943496577988 + (391547.457458611 / perimeter);
        }
        private void SendToRobert(string cmd, string value)
        {
            //if (start) {
            //    string completeCMD = cmd + "," + value;
            //    byte[] buffer = new byte[200];
            //    soc.Send(Encoding.ASCII.GetBytes(completeCMD));
            //    Thread.Sleep(20);
            //    int length=  clientSoc.Receive(buffer);
            //    string response = Encoding.UTF8.GetString(buffer);
            //    if (response.ToUpper().Contains("OK")) {
            //        string[] strs = response.Split(',');
            //        double currentHeight = Convert.ToDouble(strs[1])-19.78;
            //        grabOne();
            //        var contour = find_marker2(currentFrame.Convert<Gray, byte>());

            //        if (contour != -1)
            //        {
            //            CvInvoke.DrawContours(currentFrame, contours, contour, new MCvScalar(0, 255, 0, 100), 2);
            //            var rect = CvInvoke.MinAreaRect(contours[contour]);
            //            pixelPerimter = rect.Size.Width * 2 + rect.Size.Height * 2;
            //            StreamWriter writer = new StreamWriter(Application.StartupPath + "\\Run.txt");

            //            writer.WriteLine(currentHeight+","+pixelPerimter);
            //            showLog("写入数据："+ currentHeight + "," + pixelPerimter);
            //            writer.Flush();
            //            writer.Close();
            //        }
            //    }
            //}
        }
        private void button16_Click(object sender, EventArgs e)
        {
            LineForm line = new LineForm();
            line.ShowDialog();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            solve();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
           var status= Camera.Timing.Exposure.Set((double)numericUpDown3.Value);
            if (status == uEye.Defines.Status.Success)
            {
                showLog("曝光设置成功:Exposure="+numericUpDown3.Value);
            }
            else {
                showLog("曝光设置失败");
            }
        }

        /// <summary>
        ///  设置固定焦距
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Camera.Focus.Manual.Set((uint)numericUpDown5.Value);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            VectorOfPointF points = new VectorOfPointF();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                try
                {
                    PointF pf = new PointF();
                    pf.X = Convert.ToSingle(dataGridView1.Rows[i].Cells[1].Value.ToString());
                    pf.Y = Convert.ToSingle(dataGridView1.Rows[i].Cells[0].Value.ToString());
                    points.Push(new PointF[] { pf });
                }
                catch
                {
                    //MessageBox.Show(ex.ToString());
                }
            }
            //     PointF[] pointF = new PointF[4];
            //  VectorOfPointF pointF = new VectorOfPointF();
            Mat pointF = new Mat();
            CvInvoke.FitLine(points, pointF, Emgu.CV.CvEnum.DistType.L2, 0, 0.01, 0.01);
            this.pointF = pointF;
            var image = pointF.ToImage<Gray, float>();
            k = (double)image.Data[1, 0, 0] / image.Data[0, 0, 0];
            b = (double)image.Data[3, 0, 0] - k * image.Data[2, 0, 0];

            //   k = (double)image.] / image[0][0];
            //  k =(double) pointF.GetOutputArray()[1]/pointF.GetOutputArray()[0];
            //b = (double)pointF[1].Y - k * pointF[1].X;

            //CvInvoke.FitLine(,)
        }
    }
    public class MarkConfig {
        public string MarkName;
        public double Exposure;
        public string Focus;
        public int Delay;
    }
}