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
    public partial class ThresholdParameterForm : Form
    {
       
        public static int threshold = 0;
        public static int maxValue = 255;
        public ThresholdParameterForm()
        {
            InitializeComponent();
        }
        public event ImageProcessForm.doThreshold doT;
        private void button1_Click(object sender, EventArgs e)
        {
            try {
                threshold = Convert.ToInt32(textBox1.Text.Trim());
                maxValue = Convert.ToInt32(textBox2.Text.Trim());
                this.Hide();
                doT();
            }
            catch {
                MessageBox.Show("非法输入");
            }
        }
    }
}
