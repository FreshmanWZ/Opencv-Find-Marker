using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.UI;
using System.Windows.Forms;

namespace MatchTemplate
{
    public partial class AfterProcessImage : Form
    {
        public ImageBox imageBox = null;
        public AfterProcessImage()
        {
            InitializeComponent();
        }
        public void show() {
            this.panel1.Controls.Clear();
            this.panel1.Controls.Add(imageBox);
        }
    }
}
