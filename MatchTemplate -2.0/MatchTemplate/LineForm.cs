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
    public partial class LineForm : Form
    {
        public LineForm()
        {
            InitializeComponent();
        }

        private void LineForm_Load(object sender, EventArgs e)
        {
            StreamReader reader = new StreamReader(Application.StartupPath + "\\Run.txt");
            while (true) {
              string  pToDis= reader.ReadLine();
                if (pToDis==null) {
                    break;
                }
                string[] pds = pToDis.Split(',');
                chart1.Series[0].Points.AddXY(Convert.ToDouble(pds[0]), Convert.ToDouble(pds[1]));
                Application.DoEvents();
            }
           
        }
       
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button== MouseButtons.Left) {
                
            //    chart1.Size = new Size(Convert.ToInt32(chart1.Size.Width * 1.2), Convert.ToInt32(chart1.Size.Height * 1.2));
            //}
            //if (e.Button == MouseButtons.Right) {
            //    chart1.Size = new Size(Convert.ToInt32(chart1.Size.Width * 0.8), Convert.ToInt32(chart1.Size.Height * 0.8));
            //}
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {

                chart1.Size = new Size(Convert.ToInt32(chart1.Size.Width * 1.2), Convert.ToInt32(chart1.Size.Height * 1.2));
            }
            if (e.Button == MouseButtons.Right)
            {
                chart1.Size = new Size(Convert.ToInt32(chart1.Size.Width * 0.8), Convert.ToInt32(chart1.Size.Height * 0.8));
            }
        }
    }
}
