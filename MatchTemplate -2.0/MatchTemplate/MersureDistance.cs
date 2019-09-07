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
using ZXing;
using uEye;
using uEye_DotNet_SimpleLive;

namespace MatchTemplate
{
    public partial class MersureDistance : Form
    {
        ImageBox imageBox = null;
        Image<Bgra, byte> currentFrame = null;
        private IDScam test = new IDScam();
        IntPtr displayHandle = IntPtr.Zero;
        uEye.Camera idsCamera;
        double knownWidth;
        double knownHeight;
        int cameraIndex;
        public MersureDistance()
        {
            InitializeComponent();
            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            test.displayHandle = imageBox.Handle;
            this.panel1.Controls.Add(imageBox);
        // test.displayHandle = DisplayWindow.Handle;
            test.InitCamera();

        }
        private RotatedRect find_marker(Image<Gray, byte> image)
        {
            var bluredImage = image.SmoothGaussian(5, 5, 0, 0);

            //  var rangeImage = image.InRange(new Gray(0),new Gray(100));
            //   rangeImage = rangeImage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Close, null, new Point(), 3, Emgu.CV.CvEnum.BorderType.Default, CvInvoke.MorphologyDefaultBorderValue);
            //var thresholdImage = bluredImage.ThresholdBinary(new Gray(120), new Gray(255));
            var canniedImage = bluredImage.Canny(35, 200);
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
            if (maxIndex == -1) {
                return new RotatedRect();
            }
            return CvInvoke.MinAreaRect(contours[maxIndex]);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        bool isLive = false;
        Capture capture;
        private void openCamera() {
            isLive = true;
            capture = new Emgu.CV.Capture(cameraIndex);
            while (isLive) {
                currentFrame = new Image<Bgra, byte>(capture.QueryFrame().Bitmap);
                this.imageBox.Image = currentFrame;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //isLive = false;
            //Thread thread = new Thread(openCamera);
            //thread.IsBackground = true;
            //thread.Start();
            //openCamera(cameraIndex);
            // currentFrame = new Image<Bgra, byte>(test.OneShot());
            test.displayHandle = imageBox.Handle;
            test.CaptureLive();
            if (test.Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {
                //test.
                //bLive = true;
            }
            // imageBox.Image = currentFrame;

        }
       
      
        private void button3_Click(object sender, EventArgs e)
        {
            try {
                knownWidth = Convert.ToDouble(textBox1.Text);
                knownHeight = Convert.ToDouble(textBox2.Text);
                cameraIndex = Convert.ToInt32(textBox3.Text);
                MessageBox.Show("Success");
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //isLive = false;
            //idsCamera.Exit();
            test.Stop_Video();
        }
        bool bLive = false;

        private void button2_Click(object sender, EventArgs e)
        {
            //  capture.ImageGrabbed += Capture_ImageGrabbed;
            //Thread thread = new Thread(liveDisplay);
            //thread.IsBackground = true;
            //thread.Start();
            // Open Camera and Start Live Video
            //if ( idsCamera.Acquisition.Capture() == uEye.Defines.Status.Success)
            //{

            //    bLive = true;
            //}
        }
        double p34 = 0;
        double f =502.631579;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
           RotatedRect marker= find_marker(currentFrame.Convert<Gray,byte>());
           
            PointF[] points = marker.GetVertices();
            for (int i=0;i<4;i++) {
                CvInvoke.Circle(currentFrame, new Point((int)points[i].X, (int)points[i].Y),3,new MCvScalar(0,255,0,100),2);
                CvInvoke.PutText(currentFrame, "P" + (i + 1), new Point((int)points[i].X, (int)points[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex,1,new MCvScalar(0,0,255,100),1);
                if (i == 3)
                {
                   
                    CvInvoke.Line(currentFrame, new Point((int)points[3].X, (int)points[3].Y), new Point((int)points[0].X, (int)points[0].Y), new MCvScalar(0, 255, 0, 100));
                    break;
                }
                CvInvoke.Line(currentFrame, new Point((int)points[i].X,(int)points[i].Y),new Point((int)points[i+1].X,(int)points[i+1].Y),new MCvScalar(0,255,0,100),2);
              

            }
            p34 =Math.Pow( Math.Pow((points[2].X - points[3].X), 2) + Math.Pow(points[2].Y-points[3].Y,2),0.5);
           double dis= f*knownWidth/ p34;
            CvInvoke.PutText(currentFrame, "distance:"+dis.ToString(), new Point(50, 50), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
            imageBox.Image = currentFrame;


        }
        private void drawRotatedRectangle(ResultPoint[] points) {
            for (int i = 0; i < 4; i++)
            {
                CvInvoke.Circle(currentFrame, new Point((int)points[i].X, (int)points[i].Y), 3, new MCvScalar(0, 255, 0, 100), 2);
                CvInvoke.PutText(currentFrame, "P" + (i + 1), new Point((int)points[i].X, (int)points[i].Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1, new MCvScalar(0, 0, 255, 100), 1);
                if (i == 3)
                {

                    CvInvoke.Line(currentFrame, new Point((int)points[3].X, (int)points[3].Y), new Point((int)points[0].X, (int)points[0].Y), new MCvScalar(0, 255, 0, 100));
                    break;
                }
                CvInvoke.Line(currentFrame, new Point((int)points[i].X, (int)points[i].Y), new Point((int)points[i + 1].X, (int)points[i + 1].Y), new MCvScalar(0, 255, 0, 100), 2);


            }



        }
        private void button5_Click(object sender, EventArgs e)
        {
            capture.ImageGrabbed -= Capture_ImageGrabbed;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            capture.ImageGrabbed += Capture_ImageGrabbed1;
        }/// <summary>
        /// 找条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        {
            BarcodeReader reader = new BarcodeReader();
            Result result = reader.Decode(currentFrame.Bitmap);
            if (result==null) {
                imageBox.Image = currentFrame;
                return;
            }
            drawRotatedRectangle(result.ResultPoints);
            imageBox.Image = currentFrame;
        }
        private RotatedRect findBar(Image<Gray,byte> barImage) {
            
            return new RotatedRect();
        }
        private void InitCamera()
        {
            idsCamera= new uEye.Camera();

            uEye.Defines.Status statusRet = 0;

            // Open Camera
            statusRet = idsCamera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

            // Allocate Memory
            statusRet = idsCamera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            // Start Live Video
            statusRet =idsCamera.Acquisition.Capture();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Start Live Video failed");
            }
            else
            {
                bLive = true;
            }

            // Connect Event
            idsCamera.EventFrame += onFrameEvent;
           idsCamera.EventAutoBrightnessFinished += onAutoShutterFinished;

         //   CB_Auto_Gain_Balance.Enabled = idsCamera.AutoFeatures.Software.Gain.Supported;
         //   CB_Auto_White_Balance.Enabled = idsCamera.AutoFeatures.Software.WhiteBalance.Supported;
        }

        private void onFrameEvent(object sender, EventArgs e)
        {
            uEye.Camera Camera = sender as uEye.Camera;

            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);

            Camera.Display.Render(s32MemID, displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);
        }

        private void onAutoShutterFinished(object sender, EventArgs e)
        {
            MessageBox.Show("AutoShutter finished...");
        }
    }
}
