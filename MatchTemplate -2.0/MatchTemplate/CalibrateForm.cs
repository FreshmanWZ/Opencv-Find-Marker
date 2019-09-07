using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Threading;

namespace MatchTemplate
{
    public partial class CalibrateForm : Form
    {
        ImageBox imageBox = null;
        List<string> imageList = new List<string>();
        List<Image<Bgr, byte>> calibrateImages=new List<Image<Bgr, byte>>();
        PointF[][] cameraPoints;
        MCvPoint3D32f[][] objectPoints;
        VectorOfPointF[] vectorOfPointF;
        public CalibrateForm()
        {
            InitializeComponent();
            imageBox = new ImageBox();
            panel1.Controls.Add(imageBox);
            imageBox.Dock = DockStyle.Fill;
            imageBox.Show();
            DirectoryInfo dir = new DirectoryInfo(@"E:\C#项目\BlankCordovaApp1\TESTIMAGE\Calibrat");
            FileInfo[] images=dir.GetFiles();
            for (int i=0;i<images.Length;i++) {
                if (images[i].Extension==".jpg") {
                    imageList.Add(images[i].FullName);
                    calibrateImages.Add(new Image<Bgr, byte>(images[i].FullName));
                }
               
            }
            cameraPoints = new PointF[calibrateImages.Count][];
            objectPoints = new MCvPoint3D32f[calibrateImages.Count][];
            vectorOfPointF = new VectorOfPointF[calibrateImages.Count];
            rotateVec = new Mat[calibrateImages.Count];
            tVec = new Mat[calibrateImages.Count];

            for (int j = 0; j < calibrateImages.Count; j++)
            {
                vectorOfPointF[j] = new VectorOfPointF();
                rotateVec[j] = new Mat();
                tVec[j] = new Mat();
                cameraPoints[j] = new PointF[18*14];
                objectPoints[j] = new MCvPoint3D32f[18*14];
                for (int k=0;k<14;k++) {
                    for (int n=0;n<18;n++) {
                        objectPoints[j][k * 18+ n].X = (float)realSize * n; ;
                        objectPoints[j][k * 18+ n].Y = (float)realSize * k;
                        objectPoints[j][k * 18 + n].Z = 0;
                    }
                }
            }

        }
        private double realSize = 14.25;
 
        public void calibrateCamera() {
            for (int i=0;i<calibrateImages.Count;i++) {
               
                CvInvoke.FindChessboardCorners(calibrateImages[i],new Size(10,8),vectorOfPointF[i], Emgu.CV.CvEnum.CalibCbType.FastCheck);
                imageBox.Image = calibrateImages[i];
                CvInvoke.DrawChessboardCorners(imageBox.Image, new Size(10, 8), vectorOfPointF[i], false);
                imageBox.Image = imageBox.Image;
                Application.DoEvents();
                Thread.Sleep(1000);
            }
           
        }
        int currentIndex = 0;
        public void findCornersOne() {
            if (currentIndex>=calibrateImages.Count) {
                MessageBox.Show("已经是最后一张图片");

                return;
            }
            CvInvoke.FindChessboardCorners(calibrateImages[currentIndex].Convert<Gray,byte>(), new Size(18,14),vectorOfPointF[currentIndex], Emgu.CV.CvEnum.CalibCbType.AdaptiveThresh);
            imageBox.Image = calibrateImages[currentIndex];

            for (int i=0;i<vectorOfPointF[currentIndex].Size;i++) {
                cameraPoints[currentIndex][i].X = vectorOfPointF[currentIndex][i].X;
                cameraPoints[currentIndex][i].Y = vectorOfPointF[currentIndex][i].Y;
            }
            CvInvoke.DrawChessboardCorners(imageBox.Image, new Size(18,14), vectorOfPointF[currentIndex], true);
            imageBox.Image = imageBox.Image;
            currentIndex ++;
            Application.DoEvents();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            findCornersOne();
        }
        Mat cameraMatrix = new Mat();
        Mat distof=new Mat();
        Mat[] tVec ;
        Mat[] rotateVec ;
        List<Image<Gray,float>> tVectors = new List<Image<Gray,float>>();
        List<Image<Gray, float>> rotateVectors = new List<Image<Gray, float>>();
        private void startCalibrate() {
            CvInvoke.CalibrateCamera(objectPoints,cameraPoints,calibrateImages[0].Size,cameraMatrix,distof, Emgu.CV.CvEnum.CalibType.Default,new MCvTermCriteria(3),out rotateVec,out tVec);
            Image<Gray, float> cameraIn = cameraMatrix.ToImage<Gray, float>();
            Image<Gray, float> dof = distof.ToImage<Gray, float>();
            
            for (int i=0;i<tVec.Length;i++) {
                Image<Gray, float> tImage = tVec[i].ToImage<Gray,float>();
                Image<Gray, float> rImage = rotateVec[i].ToImage<Gray, float>();
                tVectors.Add(tImage);
                rotateVectors.Add(rImage);

            }
            
            StreamWriter writer = new StreamWriter(Application.StartupPath+"\\CameraParameters.txt");
            writer.WriteLine(cameraIn.Data[0, 0, 0] + "," + cameraIn.Data[0, 1, 0] + "," + cameraIn.Data[0, 2, 0]);
            writer.WriteLine(cameraIn.Data[1, 0, 0] + "," + cameraIn.Data[1, 1, 0] + "," + cameraIn.Data[1, 2, 0]);
            writer.WriteLine(cameraIn.Data[2, 0, 0] + "," + cameraIn.Data[2, 1, 0] + "," + cameraIn.Data[2, 2, 0]);
            writer.WriteLine(dof.Data[0,0,0]);
            writer.WriteLine(dof.Data[0, 1, 0]);
            writer.WriteLine(dof.Data[0, 2, 0]);
            writer.WriteLine(dof.Data[0, 3, 0]);
            writer.WriteLine(dof.Data[0,4,0]);
            writer.Flush();
            writer.Close();
            MessageBox.Show("计算畸变以及相机内参完毕,储存完毕");
        }

        private Image<Gray, byte> drawChessboard(int dotSize,int blockNum) {
            int imageSize = blockNum * dotSize;
            Image<Gray, byte> image = new Image<Gray, byte>(imageSize,imageSize);
            image.SetValue(new Gray(255));
            bool flag = true;
            for (int i=0;i<imageSize;i+=dotSize) {
                for (int j=0;j<imageSize; j+=dotSize) {
                   
                    Rectangle roi = new Rectangle(i,j,dotSize,dotSize);
                    image.ROI = roi;
                    if (flag)
                    {
                        image.SetValue(0);
                    }
                    else {
                        image.SetValue(255);
                    }
                    flag = !flag;
                }
            }
            image.ROI = Rectangle.Empty;
            return image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var image = drawChessboard(50, 19);
            imageBox.Image = image;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startCalibrate();
        }
        Image<Bgr, byte> showImage;
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                showImage= new Image<Bgr, byte>(fileDialog.FileNames[0]);
                imageBox.Image= showImage;
              
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(imageBox.Image.Size);
            Mat rv = new Mat();
            Mat tv=new Mat();
            Image<Gray, float> intri = cameraMatrix.ToImage<Gray, float>();
            Image<Gray, float> dis = distof.ToImage<Gray, float>();
          //  var a=CameraCalibration.SolvePnP(objectPoints[0],cameraPoints[0], new IntrinsicCameraParameters(5), Emgu.CV.CvEnum.SolvePnpMethod.Iterative);
           // CvInvoke.SolvePnP(objectPoints[0],cameraPoints[0],cameraMatrix,distof,rv,tv);
         //   Image<Gray, float> rvI = rv.ToImage<Gray, float>();
           // Image<Gray, float> tvI = tv.ToImage<Gray, float>();
            CvInvoke.Undistort(imageBox.Image, image, cameraMatrix, distof);
            
            imageBox.Image = image;
            MessageBox.Show("矫正图像完毕");
        }
    }
}
