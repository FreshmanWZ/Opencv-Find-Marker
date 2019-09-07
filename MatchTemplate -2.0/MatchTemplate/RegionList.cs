using Emgu.CV;
using Emgu.CV.Structure;
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
    public partial class RegionList : Form
    {
        public Rectangle rectangleROI;
        public CircleF circleROI;
        public Emgu.CV.Structure.Ellipse ellipseROI;

        public RegionList()
        {
            InitializeComponent();
         
        }
        public void addOneRegion() {
        int index= this.listBox1.Items.Add("Region"+listBox1.Items.Count);
        
        }
    }
}
