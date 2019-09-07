using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using System.Xml;
using Emgu.Util;
using Emgu.CV.Util;
using EmgucvDemo;

namespace MatchTemplate
{
    public partial class Form2 : Form
    {
        List<MyPoint> rawPoints = null;
        List<MyPoint> correctPoints = null;
        Mat reflection = null;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                dataGridView1.Rows.Add();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PointF[] rawPoints = new PointF[dataGridView1.Rows.Count-1];
            PointF[] correctPoints = new PointF[dataGridView1.Rows.Count-1];
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
                {
                    if (dataGridView1.Rows[i].Cells[0].Value==null) {
                        break;
                    }
                    rawPoints[i].X = Convert.ToSingle(dataGridView1.Rows[i].Cells[0].Value);
                    rawPoints[i].Y =Convert.ToSingle(dataGridView1.Rows[i].Cells[1].Value);
                    correctPoints[i].X = Convert.ToSingle(dataGridView1.Rows[i].Cells[2].Value);
                    correctPoints[i].Y = Convert.ToSingle(dataGridView1.Rows[i].Cells[3].Value);
                   
                   
                }
                reflection = new Mat();
                CvInvoke.FindHomography(rawPoints, correctPoints, reflection, Emgu.CV.CvEnum.HomographyMethod.Default);
                PointF[] test = new PointF[1];
                test[0].X = Convert.ToSingle(textBox1.Text);
                test[0].Y = Convert.ToSingle(textBox2.Text);
                
                var result=CvInvoke.PerspectiveTransform(test, reflection);
                textBox1.Text = result[0].X.ToString();
                textBox2.Text = result[0].Y.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try {
                dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 2);
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".xml配置文件|*.xml";
            if (openFileDialog.ShowDialog()== DialogResult.OK&&openFileDialog.FileNames.Length>0) {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(openFileDialog.FileNames[0]);
                var childs= xmlDocument.DocumentElement.ChildNodes;
                dataGridView1.Rows.Clear();
                foreach(XmlNode node in childs){
                   int index= dataGridView1.Rows.Add();
                   dataGridView1.Rows[index].Cells[0].Value= node.ChildNodes[0].InnerText;
                    dataGridView1.Rows[index].Cells[1].Value = node.ChildNodes[1].InnerText;
                    dataGridView1.Rows[index].Cells[2].Value = node.ChildNodes[2].InnerText;
                    dataGridView1.Rows[index].Cells[3].Value = node.ChildNodes[3].InnerText;
                }
            }
        }
    }
    class MyPoint
    {
        public double x;
        public double y;
        public MyPoint()
        {

        }
        public MyPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
