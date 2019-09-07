using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace uEye_DotNet_SimpleLive
{
    class IDScam
    {
        public uEye.Camera Camera;
        private bool bLive = false;
        public IntPtr displayHandle = IntPtr.Zero;

        public void InitCamera()
        {
            

            Camera = new uEye.Camera();
            

            uEye.Defines.Status statusRet = 0;

            // Open Camera
            statusRet = Camera.Init();
            if (statusRet != uEye.Defines.Status.Success)
            {
                //MessageBox.Show("Camera initializing failed");
                Environment.Exit(-1);
            }

            Camera.Trigger.Set(uEye.Defines.TriggerMode.Software);
           
            uEye.Types.ImageFormatInfo[] FormatInfoList;
            Camera.Size.ImageFormat.GetList(out FormatInfoList);

            int count = FormatInfoList.Count();

            Camera.Size.ImageFormat.Set((uint)FormatInfoList[10].FormatID);

            // Allocate Memory
            statusRet = Camera.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.Success)
            {
               // MessageBox.Show("Allocate Memory failed");
                Environment.Exit(-1);
            }

            
            //
            //Camera.Size.ImageFormat.Set(4192*3104);
            //// Start Live Video
            //statusRet = Camera.Acquisition.Capture();
            //if (statusRet != uEye.Defines.Status.Success)
            //{
            //    //MessageBox.Show("Start Live Video failed");
            //}
            //else
            //{
            //    bLive = true;
            //}

            // Connect Event
            Camera.EventFrame += onFrameEvent;
            
        }

        private void onFrameEvent(object sender, EventArgs e)
        {
            uEye.Camera Camera = sender as uEye.Camera;

            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);

           // Bitmap bitmap;
            //Camera.Memory.ToBitmap(s32MemID, out bitmap);

            Camera.Display.Render(s32MemID, displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);
        }

        public void CaptureLive()
        {
            if (Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            {

                bLive = true;
            }
        }

        public void Stop_Video()
        {
            // Stop Live Video
            if (Camera.Acquisition.Stop() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }

        public void Freeze_Video( )
        {
            if (Camera.Acquisition.Freeze() == uEye.Defines.Status.Success)
            {
                bLive = false;
            }
        }

        public Bitmap OneShot()
        {
            Bitmap bitmap = null;

            //Camera.Size.AOI.Set(0, 0, 4192, 3104);

            //System.Drawing.Rectangle rect;
            //Camera.Size.AOI.Get(out rect);

            //rect.Width = 4192;
            //rect.Height = 3104;

            // Camera.Size.AOI.Set(rect);

            

            uEye.Defines.Status statusRet;

            statusRet=Camera.Acquisition.Freeze(uEye.Defines.DeviceParameter.Wait);
           


            Int32 s32MemID;
            Camera.Memory.GetActive(out s32MemID);

            Camera.Memory.ToBitmap(s32MemID, out bitmap);

            Camera.Display.Render(s32MemID, displayHandle, uEye.Defines.DisplayRenderMode.FitToWindow);

            //if (Camera.Acquisition.Capture() == uEye.Defines.Status.Success)
            //{
            //    System.Threading.Thread.Sleep(100);
            //    Camera.Acquisition.Stop();






            //    bLive = true;
            //}
            return bitmap;
        }

    }
}
