using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye;

namespace MatchTemplate
{
    public partial class CameraConfig : Form
    {
        ImageBox imageBox = new ImageBox();
        private uEye.Camera Camera;
        Image<Bgr, byte> currentFrame;
        bool bLive = false;
        Thread moniterThread;
        Socket socket;
        bool isListenning = false;
        Thread watchT = null;
        public CameraConfig()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            imageBox.Dock = DockStyle.Fill;
            panel3.Controls.Add(imageBox);
            imageBox.Show();
            InitCamera();
            Camera.Timing.Exposure.Set((double)numericUpDown2.Value);
            Camera.Focus.Auto.SetEnable(checkBox1.Checked);
            Camera.EventDeviceUnPlugged += Camera_EventDeviceUnPlugged;
            moniterThread = new Thread(moniterFocus);
            moniterThread.IsBackground = true;
            moniterThread.Start();

           
        }
        private void accpetClient()
        {
            while (isListenning)
            {
                Socket client = socket.Accept();

                showLog("接受到客户端的连接");
                Thread response = new Thread(handler);
                response.IsBackground = true;
                response.Start(client);
            }
        }
        private void handler(object client)
        {
            Socket soc = client as Socket;
            while (isListenning)
            {
                byte[] buffer = new byte[1024];
                int length = soc.Receive(buffer);
                if (length==0) {
                    showLog("客户端断开连接");
                    return;
                }
                string mes = Encoding.UTF8.GetString(buffer, 0, length);


                lock ("lock1")
                {
                    if (mes.ToUpper().Contains("XXYY")) {
                        grabOne();

                    }
                    soc.Send(Encoding.ASCII.GetBytes("1,+00001.000,+00000.000,+00000.000,+00000.000"));
                }
            }
        }
                    private void Camera_EventDeviceUnPlugged(object sender, EventArgs e)
        {
            MessageBox.Show("相机已经拔出");
        }
        private void moniterFocus() {
            while (true) {
                uint f = 0;
                Camera.Focus.Manual.Get(out f);
                label7.Text = f.ToString();
                Thread.Sleep(20);
            }
        }
        public void showLog(string text) {
            if (richTextBox1.Lines.Length > 100)
            {
                richTextBox1.Clear();
            }
            this.richTextBox1.Text += "\r\n" + text;
        }

        private void InitCamera()
        {
            Camera = new uEye.Camera();
            uEye.Defines.Status statusRet = 0;
            statusRet = Camera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

            //   Camera.Parameter.Load();   

            Camera.Trigger.Set(uEye.Defines.TriggerMode.Software);

            statusRet = Camera.Focus.Auto.SetEnable(true);
            if (statusRet == uEye.Defines.Status.Success)
            {
                showLog("自动对焦成功");
            }
            uEye.Types.ImageFormatInfo[] list;
            Camera.Size.ImageFormat.GetList(out list);
            Camera.PixelFormat.Set(uEye.Defines.ColorMode.BGR8Packed);
            uint formatId = (uint)list[0].FormatID;
            Camera.Size.ImageFormat.Set(formatId);
            statusRet = Camera.Memory.Allocate();

            //   Camera.Focus.Zone.SetAOI(new Rectangle(new Point(1100, 636), new Size(672, 596)));
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            Camera.EventFrame += onFrameEvent;
            Camera.EventAutoBrightnessFinished += onAutoShutterFinished;
            Camera.AutoFeatures.Software.WhiteBalance.Enabled = false;
            //Camera.Timing.Expoure.Set(80);
            Camera.Focus.Auto.Enabled = true;
     
            Camera.AutoFeatures.Software.Gain.Enabled = false;
            statusRet = Camera.AutoFeatures.Sensor.GainShutter.SetEnable(false);
            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("自动曝光设置关闭失败");
                Environment.Exit(-1);
            }

            statusRet = Camera.Timing.Exposure.Set(32);

            if (statusRet != uEye.Defines.Status.Success)
            {
                MessageBox.Show("曝光设置失败");
                Environment.Exit(-1);
            }

            //   CB_Auto_Gain_Balance.Enabled = Camera.AutoFeatures.Software.Gain.Supported;
            //  CB_Auto_White_Balance.Enabled = Camera.AutoFeatures.Software.WhiteBalance.Supported;
        }
        private void grabOne()
        {
          
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);

            sw.Stop();
            showLog("相机拍照耗时:" + sw.ElapsedMilliseconds + "ms");
            currentFrame = new Image<Bgr, byte>(bitmap);
            imageBox.Image = currentFrame;
        }

        private void onAutoShutterFinished(object sender, EventArgs e)
        {
           
        }

        private void onFrameEvent(object sender, EventArgs e)
        {
            GC.Collect();
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
         //   Stopwatch sw = new Stopwatch();
         //   sw.Start();
         //   statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);

           // sw.Stop();
          //  showLog("相机拍照耗时:" + sw.ElapsedMilliseconds + "ms");
            currentFrame = new Image<Bgr, byte>(bitmap);
            imageBox.Image = currentFrame;
        }
        /// <summary>
        /// 开始实时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Camera.Focus.Auto.SetEnable(true);
            showLog("开始实时显示");
            imageBox.Image = null;
            Camera.Trigger.Set(uEye.Defines.TriggerMode.Continuous);

            uEye.Types.ImageFormatInfo[] FormatInfoList;
            Camera.Size.ImageFormat.GetList(out FormatInfoList);

            int count = FormatInfoList.Count();

            Camera.Size.ImageFormat.Set((uint)FormatInfoList[0].FormatID);



            if (Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {
                bLive = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            grabOne();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {

                var status = Camera.Focus.Auto.SetEnable(true);
                if (status == uEye.Defines.Status.Success)
                {
                    showLog("自动聚焦打开");
                }
                else
                {
                    showLog("自动聚焦打开失败");
                }
            }
            else {
                var status = Camera.Focus.Auto.SetEnable(false);
                if (status == uEye.Defines.Status.Success)
                {
                    showLog("自动曝光关闭");
                }
                else
                {
                    showLog("自动聚焦关闭失败");
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

          var status=  Camera.Focus.Manual.Set((uint)numericUpDown1.Value);
            if (status == uEye.Defines.Status.Success)
            {
                showLog("Current Focus:"+numericUpDown1.Value);
            }
            else
            {
                showLog("设置焦距失败");
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            var status = Camera.Timing.Exposure.Set((double)numericUpDown2.Value);
            if (status == uEye.Defines.Status.Success)
            {
                showLog("Current Exposure:" + numericUpDown2.Value);
            }
            else
            {
                showLog("设置曝光失败");
            }
        }
        /// <summary>
        /// 停止实时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            showLog("停止实时显示");
            if (Camera.Acquisition.Stop() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Camera.Focus.Auto.SetEnable(true);
            Camera.Focus.Trigger();
            Thread.Sleep((int)numericUpDown3.Value);
            Bitmap bitmap = null;
            uEye.Defines.Status statusRet;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            statusRet = Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);
            Camera.Memory.ToBitmap(s32MemID, out bitmap);

            sw.Stop();
            showLog("相机拍照耗时:" + sw.ElapsedMilliseconds + "ms");
            currentFrame = new Image<Bgr, byte>(bitmap);
            imageBox.Image = currentFrame;
            Camera.Focus.Auto.SetEnable(checkBox1.Checked);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Camera.Focus.Trigger();
        }

        private void CameraConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            moniterThread.Abort();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (isListenning)
            {
                showLog("已开始监听");
                return;
            }
            IPEndPoint end = new IPEndPoint(IPAddress.Parse("192.168.1.30"), 10004);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(end);
            socket.Listen(1000);
            isListenning = true;
            watchT = new Thread(accpetClient);
            watchT.IsBackground = true;
            watchT.Start();
            showLog("已经开始监听------");
        }
    }
}
