using Emgu.CV;
using Emgu.CV.Structure;
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

namespace MatchTemplate
{
    public partial class _2DTo3DForm : Form
    {
        ImageBox imageBox = new ImageBox();
        Image<Bgra, byte> image = null;
        Mat reflection = null;
        public _2DTo3DForm()
        {
            InitializeComponent();
            imageBox = new ImageBox();
            imageBox.Dock = DockStyle.Fill;
            this.panel2.Controls.Add(imageBox);
        }
        /// <summary>
        /// 打开图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                //bitmap = new Bitmap(fileDialog.FileNames[0]);
                //this.imageBox.Image = new Image<Bgra, byte>(bitmap);
                image = new Image<Bgra, byte>(fileDialog.FileNames[0]);
                imageBox.Image = image;

            }
        }
        /// <summary>
        /// 透视变换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            PointF[] points = new PointF[4];
            points[0].X = Convert.ToSingle(textBox1.Text);
            points[0].Y = Convert.ToSingle(textBox2.Text);
            points[1].X = Convert.ToSingle(textBox4.Text);
            points[1].Y = Convert.ToSingle(textBox3.Text);
            points[2].X = Convert.ToSingle(textBox6.Text);
            points[2].Y = Convert.ToSingle(textBox5.Text);
            points[3].X = Convert.ToSingle(textBox8.Text);
            points[3].Y = Convert.ToSingle(textBox7.Text);
            reflection = new Mat();
            PointF[] rawpoints = new PointF[] {new PointF(0,0),new PointF(100,0),new PointF(0,100),new PointF(100,100) };
          
            CvInvoke.FindHomography(rawpoints, points, reflection, Emgu.CV.CvEnum.HomographyMethod.Default);  //找到单应矩阵
            
            CvInvoke.WarpPerspective(image, image, reflection,image.Size);
            this.imageBox.Image = image;
        }
        /// <summary>
        /// 点云图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            PointF[] points = new PointF[4];
            points[0].X = Convert.ToSingle(textBox1.Text);
            points[0].Y = Convert.ToSingle(textBox2.Text);
            points[1].X = Convert.ToSingle(textBox4.Text);
            points[1].Y = Convert.ToSingle(textBox3.Text);
            points[2].X = Convert.ToSingle(textBox6.Text);
            points[2].Y = Convert.ToSingle(textBox5.Text);
            points[3].X = Convert.ToSingle(textBox8.Text);
            points[3].Y = Convert.ToSingle(textBox7.Text);
            
          

            Image<Xyz, byte> pointCloudImage = new Image<Xyz, byte>(image.Width,image.Height);
            MCvPoint3D32f[] realPoint = new MCvPoint3D32f[4];
            Mat cameraMatrix = new Mat();
            Mat distortionCoffe = new Mat();
            Mat rotationMatrix = new Mat();
            Mat translationMatrix = new Mat();

          //  CvInvoke.CalibrateCamera(realPoint,points, image.Size, cameraMatrix, distortionCoffe, rotationMatrix, translationMatrix, Emgu.CV.CvEnum.CalibType.Default, new MCvTermCriteria());
          //CvInvoke.StereoRectify(,,,,,,,,,,,,  Emgu.CV.CvEnum.StereoRectifyType.Default,,,,)
         
            
            CvInvoke.ReprojectImageTo3D(image.Convert<Gray,byte>(), pointCloudImage, reflection, false,  Emgu.CV.CvEnum.DepthType.Default);
            imageBox.Image = pointCloudImage;
        }
        /// <summary>
        /// 2D转3D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
