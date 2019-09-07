using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Threading;
using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class ContoursForm : Form
    {
        public VectorOfVectorOfPoint modelContour = null;
        public Image<Gray, byte> sourceImage = null;
        ImageBox imageBox = new ImageBox();
        public event ImageProcessForm.SACFindMarkHandler doSACFind;
        public event ImageProcessForm.FindContoursHandler doFindContours;
        
        public ContoursForm()
        {
            InitializeComponent();
            this.panel1.Controls.Clear();
            panel1.Controls.Add(imageBox);
            imageBox.Dock = DockStyle.Fill;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
                doFindContours(Convert.ToInt32(textBox1.Text), Convert.ToDouble(textBox2.Text));

            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            
        }
        public void showContoursImage(Image<Bgra,byte> image) {
            this.imageBox.Image = image;
            imageBox.Show();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Drawing.Point point = new System.Drawing.Point();
            double angle = 0;
            double socre = Convert.ToDouble(textBox6.Text);
            doSACFind(sourceImage, new Image<Gray, byte>(imageBox.Image.Bitmap), Convert.ToDouble(textBox3.Text.Trim()),Convert.ToDouble(textBox4.Text.Trim()),Convert.ToInt16(textBox5.Text.Trim()),out point,out angle,socre);
     
        }
        //void keepFinding() {
        //    while (true) {
        //        System.Drawing.Point point = new System.Drawing.Point();
        //        double angle = 0;
        //        //doSACFind(sourceImage, new Image<Gray, byte>(imageBox.Image.Bitmap), Convert.ToDouble(textBox3.Text.Trim()), Convert.ToDouble(textBox4.Text.Trim()), Convert.ToInt16(textBox5.Text.Trim()), out point, out angle,socre);
        //    }
           
        //}

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                this.imageBox.Image = new Image<Gray, byte>(fileDialog.FileNames[0]);


            }
        }
    }
}
