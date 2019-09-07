using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class FormatImage : Form
    {
        public FormatImage()
        {
            InitializeComponent();
        }
        public void formatImage() {
            string posPath = @"E:\BarCode\pos";
            string negPath = @"E:\BarCode\neg";
            DirectoryInfo pos = new DirectoryInfo(posPath);
            DirectoryInfo neg = new DirectoryInfo(negPath);

            FileInfo[] posImages = pos.GetFiles();
            FileInfo[] negImages = neg.GetFiles();
            progressBar1.Maximum = posImages.Length + negImages.Length;

            for (int i=0;i<posImages.Length;i++) {
                Image<Gray, byte> tempImage = new Image<Gray, byte>(posImages[i].FullName);
                CvInvoke.Resize(tempImage, tempImage, new Size(100, 40),0,0, Emgu.CV.CvEnum.Inter.Area);
                tempImage.Save(@"E:\BarCode\pos\pos" + i + ".png");
                progressBar1.Value += 1;
            }
            for (int i=0;i<negImages.Length;i++) {
                Image<Gray, byte> tempImage = new Image<Gray, byte>(negImages[i].FullName);
                CvInvoke.Resize(tempImage, tempImage, new Size(100, 40), 0, 0, Emgu.CV.CvEnum.Inter.Area);
                tempImage.Save(@"E:\BarCode\neg\neg" + i + ".png");
                progressBar1.Value += 1;
            }
            progressBar1.Visible = false;
            MessageBox.Show("图像Resize完毕！");
        }

        private void button1_Click(object sender, EventArgs e)
        {
         //   formatImage();
        }
        private void writePosTxt() {

            StreamWriter streamWriter = new StreamWriter(@"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\机器学习\车牌检测\pos.txt");
           
            for (int i=0;i<1401;i++) {
                streamWriter.WriteLine("pos/pos" + (i+1) + ".jpg 1 0 0 136 36");
            }
            streamWriter.Flush();
            streamWriter.Close();
           
        }
        private void writeNegTxt() {
            StreamWriter streamWriter = new StreamWriter(@"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\机器学习\车牌检测\neg.txt");

            for (int i = 0; i < 2175; i++)
            {
                streamWriter.WriteLine("neg/neg" + (i+1) + ".jpg");
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            writePosTxt();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            writeNegTxt();
        }
    }
}
