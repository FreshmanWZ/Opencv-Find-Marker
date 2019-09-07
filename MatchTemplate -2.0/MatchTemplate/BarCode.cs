using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZBar;
using ZXing.QrCode;
using ZXing;
using ZXing.Datamatrix;

using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MatchTemplate
{
    public partial class BarCode : Form
    {
        System.Drawing.Image image = null;
        BarcodeReader reader = new BarcodeReader();
        public BarCode()
        {
            InitializeComponent();
        }
        public void ScanCode(System.Drawing.Image image) {
            ZBar.ImageScanner scanner = new ImageScanner();
            scanner.Scan(image);
            List<ZBar.Symbol> symbols = new List<Symbol>();
            ZXing.OneD.Code93Reader reader3 = new ZXing.OneD.Code93Reader();
            reader.ResultFound += Reader_ResultFound;
            if (symbols != null && symbols.Count > 0)
            {
                string r = string.Empty;
                foreach (var s in symbols)
                {
                    textBox1.Text = s.Data;
                    
                }
            }
            //ZBar.ImageScanner scanner= new ImageScanner();
            //scanner.SetConfiguration(ZBar.SymbolType.None, ZBar.Config.Enable, 0);
            //scanner.SetConfiguration(ZBar.SymbolType.CODE39, ZBar.Config.Enable, 1);
            //scanner.SetConfiguration(ZBar.SymbolType.CODE128, ZBar.Config.Enable, 1);
            //List<ZBar.Symbol> symbols = new List<Symbol>();
            //symbols = scanner.Scan(image);
            //if (symbols!=null&&symbols.Count>0) {
            //    string result = string.Empty;
            //    foreach (var s in symbols) {
            //        textBox1.Text=s.Data;
            //    }
            //}
            Result result =  reader.Decode((Bitmap)image);
            foreach (var point in result.ResultPoints) {
                Bitmap bitmap = new Bitmap(image);
            //    panel1.DrawToBitmap(bitmap, new Rectangle(new Point((int)point.X,(int)point.Y), new Size(result.)));
                panel1.BackgroundImage = bitmap;
            }
            textBox1.Text = result.Text;
        }

        private void Reader_ResultFound(Result obj)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (image!=null) {
                ScanCode(image);
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = ".JPG|*.JPG|.PNG|*.PNG|.BMP|*.BMP|.JPEG|*.JPEG";
            fileDialog.InitialDirectory = @"E:\C#项目\EmgucvDemo\EmgucvDemo\Properties\ImageDataBase";
            fileDialog.Multiselect = false;
            if (fileDialog.ShowDialog() == DialogResult.OK && fileDialog.FileNames.Length > 0)
            {
                image = System.Drawing.Image.FromFile(fileDialog.FileNames[0]);
                panel1.BackgroundImage = image;

            }
        }
    }
}
