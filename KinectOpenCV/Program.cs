using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace KinectOpenCV
{
    class Program
    {
        private static KinectSensor sensor;

        static void Main(string[] args)
        {
            foreach (KinectSensor potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    sensor = potentialSensor;
                    break;
                }
            }

            if (sensor != null)
            {
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                sensor.ColorFrameReady += SensorFrameReady;

                try
                {
                    sensor.Start();
                }
                catch (IOException)
                {
                    sensor = null;
                }
            }

            if (sensor == null)
            {
                Console.WriteLine("The kinect did not start properly. Press any key to exit");
                Console.ReadKey();
            }
            else
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
        // Data, bitmap and image stored to avoid lots of allocations
        private static byte[] data;
        private static Bitmap bitmap;
        private static Image<Bgr, byte> image;
        // DisplayMat stored so we can always use the last image to display
        private static Mat displayMat = new Mat();
        private static readonly object mutex = new object();

        private static void SensorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                {
                    return;
                }

                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(mutex, ref lockTaken);
                    if (!lockTaken)
                    {
                        // Return if someone else already has the lock. OK to miss
                        return;
                    }

                    // Convert to OpenCV mat
                    using (var rawImage = frame.ToOpenCVMat(ref data, ref bitmap, ref image))
                    {
                        if (rawImage == null) return;
                        CvInvoke.Flip(rawImage, displayMat, FlipType.Horizontal);
                    }

                    CvInvoke.Imshow("Image", displayMat);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(mutex);
                    }
                }
                CvInvoke.WaitKey(1);
            }
        }
    }
}
