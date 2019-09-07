using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatchTemplate
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           // CircleSeuence<int> circleSeuence = new CircleSeuence<int>(6);
            //CircleIterator<int> circleIterator = new CircleIterator<int>(circleSeuence);
            //for (int i=0;i<6;i++) {
            //    circleSeuence.setValue(i, i);

            //}
            //for (int i=0;i<circleSeuence.getSize();i++) {
            //   Console.WriteLine("The "+i+" times:"+ circleIterator.next());
            //}
            //circleSeuence.insertAfter(3, 890);
            //for (int i = 0; i < circleSeuence.getSize(); i++)
            //{
            //    Console.WriteLine("The " + i + " times:" + circleIterator.next());
            //}
        // Application.Run(new CameraConfig()); 
           Application.Run(new IDSCamera());
         // Application.Run(new CalibrateForm());
        }
    }
}
