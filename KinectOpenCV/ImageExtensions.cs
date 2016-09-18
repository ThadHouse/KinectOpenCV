using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace KinectOpenCV
{
    // Code from here
    //http://www.apress.com/9781430241041

    public static class ImageExtensions
    {

        public static Mat ToOpenCVMat(this ColorImageFrame image, ref byte[] data, ref Bitmap bitmap, ref Image<Bgr, byte> cvImage)
        {
            // convert color image to data array
            if (image == null || image.PixelDataLength == 0)
                return null;
            if (data == null || data.Length < image.PixelDataLength)
                data = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(data);

            if (bitmap == null || (bitmap.Width != image.Width && bitmap.Height != image.Height &&
                bitmap.PixelFormat != PixelFormat.Format32bppRgb))
            {
                bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppRgb);
            }

            // Copy data array to a bitmap
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            // copy bitmap to OpenCV Image Object
            if (cvImage == null)
            {
                cvImage = new Image<Bgr, byte>(bitmap);
            }
            else
            {
                cvImage.Bitmap = bitmap;
            }
            // Return Mat of OpenCV bitmap object
            return cvImage.Mat;
        }
    }
}
