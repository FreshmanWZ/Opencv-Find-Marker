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

namespace MatchTemplate
{
    public partial class CameraPropertyForm : Form
    {
        public ImageProcessForm paf;
        public CameraPropertyForm(ImageProcessForm pf)
        {
            this.paf = pf;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            Thread thread = new Thread(getFocusLength);
            thread.IsBackground = true;
            thread.Start();

        }

        private void CameraPropertyForm_Load(object sender, EventArgs e)
        {
       //    this.numericUpDown1.Value= (decimal)paf.capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure);
          //  this.numericUpDown2.Value = (decimal)paf.capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.paf.capture.SetCaptureProperty( Emgu.CV.CvEnum.CapProp.Exposure,(double)numericUpDown1.Value);
            
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
          
            paf.capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, (double)numericUpDown2.Value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                paf.capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
            }
            else {
                paf.capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AndroidFocusMode, 1);
            }
                
        }
        private void getFocusLength() {
            while (true) {
                label4.Text = paf.capture.GetCaptureProperty( Emgu.CV.CvEnum.CapProp.Focus).ToString();

                Thread.Sleep(10);
            }
        }
    }
}
