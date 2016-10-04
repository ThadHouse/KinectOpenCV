using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Kinect;
using NetworkTables;
using NetworkTables.Tables;
using OpenTK;
using TankTracker.Base.Structs;
using TankTracker.Networking;
using Vector3 = System.Numerics.Vector3;

namespace KinectOpenCV
{


    struct Line
    {
        public double Length { get; }
        public double Slope { get; }
        public double OtherSlope { get; }

        public Line(double len, double slope, double otherSlope)
        {
            Length = len;
            OtherSlope = otherSlope;
            Slope = slope;
        }
    }

    class Program
    {
        private static KinectSensor sensor;

        private static ITable table;

        const string HSVLow = "HSVLow";
        const string HSVHigh = "HSVHigh";

        static MCvScalar Red = new MCvScalar(0, 0, 255);
        static MCvScalar Green = new MCvScalar(0, 255, 0);
        static MCvScalar Blue = new MCvScalar(255, 0, 0);

        static VectorOfDouble arrayLow = new VectorOfDouble(3);
        static VectorOfDouble arrayHigh = new VectorOfDouble(3);

        static double[] defaultLow = new double[] { 50, 44, 193 };
        static double[] defaultHigh = new double[] { 90, 255, 255 };

        static void Main(string[] args)
        {
            //NetworkTable.SetServerMode();
            //NetworkTable.Initialize();
            //table = NetworkTable.GetTable("OpenCV");

            NetworkTableProvider provider = new NetworkTableProvider();
            Settings settings = new Settings(provider);

            const double defaultExposure = 22;
            const double defaultContrast = 1.0;
            const int defaultWhiteBalance = 2700;

            const double minExposure = 0.0;
            const double maxExposure = 40000;

            const double minContrast = 0.5;
            const double maxContrast = 2.0;

            const int minWhiteBalance = 2700;
            const int maxWhiteBalance = 6500;

            settings.SetupContrast(new NetworkedValue<double>(minContrast, maxContrast, defaultContrast));
            settings.SetupExposureTime(new NetworkedValue<double>(minExposure, maxExposure, defaultExposure));
            settings.SetupWhiteBalance(new NetworkedValue<int>(minWhiteBalance, maxWhiteBalance, defaultWhiteBalance));

            Thread.Sleep(Timeout.Infinite);

            GC.KeepAlive(settings);
            GC.KeepAlive(provider);
            /*
            table = provider.GetRootTable.GetSubTable("OpenCV");

            table.SetDefaultNumberArray(HSVLow, defaultLow);
            table.SetDefaultNumberArray(HSVHigh, defaultHigh);

            //table.SetDefaultNumber("exposure", 22);

            //table.SetPersistent("exposure");

            

            table.SetPersistent(HSVLow);
            table.SetPersistent(HSVHigh);

            table.SetDefaultNumber(nameof(minSize), minSize);
            table.SetDefaultNumber(nameof(maxSize), maxSize);

            table.SetPersistent(nameof(minSize));
            table.SetPersistent(nameof(maxSize));

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
                settings = new KinectNetworkSettings(provider, sensor.ColorStream.CameraSettings);
                Thread.Sleep(Timeout.Infinite);
            }
            */
        }

        private static KinectNetworkSettings settings;

        // Data, bitmap and image stored to avoid lots of allocations
        private static byte[] data;
        private static Bitmap bitmap;
        private static Image<Bgr, byte> image;
        // DisplayMat stored so we can always use the last image to display
        private static Mat rawDisplayImage = new Mat();
        private static Mat hsvConvert = new Mat();
        private static Mat inRangeMat = new Mat();
        private static readonly object mutex = new object();

        static VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

        private static double minSize = 100;
        private static double maxSize = 500;

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

                    var ntLow = table.GetNumberArray(HSVLow, defaultLow);
                    var ntHigh = table.GetNumberArray(HSVHigh, defaultHigh);

                    double localMinSize = table.GetNumber(nameof(minSize), minSize);
                    double localMaxSize = table.GetNumber(nameof(maxSize), maxSize);

                    if (ntLow.Count != 3)
                        ntLow = defaultLow;
                    if (ntHigh.Count != 3)
                        ntHigh = defaultHigh;

                    arrayLow.Clear();
                    arrayLow.Push(ntLow.ToArray());
                    arrayHigh.Clear();
                    arrayHigh.Push(ntHigh.ToArray());

                    // Convert to OpenCV mat
                    using (var rawImage = frame.ToOpenCVMat(ref data, ref bitmap, ref image))
                    using (var display = new Mat())
                    //using (var hsvOut = new Mat())
                    using (var temp = new Mat())

                    {
                        if (rawImage == null) return;
                        CvInvoke.Flip(rawImage, display, FlipType.Horizontal);

                        CvInvoke.CvtColor(display, hsvConvert, ColorConversion.Bgr2Hsv);

                        

                        CvInvoke.InRange(hsvConvert, arrayLow, arrayHigh, inRangeMat);

                        inRangeMat.ConvertTo(temp, DepthType.Cv8U);

                        CvInvoke.FindContours(temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                        //CvInvoke.DrawContours(rawDisplayImage, contours, -1, new MCvScalar(0, 0, 255));

                        VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint(contours.Size);

                        for (int i = 0; i < contours.Size; i++)
                        {
                            CvInvoke.ConvexHull(contours[i], convexHulls[i]);
                        }


                        // Filter convex hulls
                        for (int i = 0; i < convexHulls.Size; i++)
                        {
                            VectorOfPoint hull = convexHulls[i];
                            VectorOfPoint polygon = new VectorOfPoint(convexHulls.Size);
                            CvInvoke.ApproxPolyDP(hull, polygon, 10, true);

                            if (polygon.Size != 3)
                            {
                                polygon.Dispose();
                                continue;
                            }

                            if (!CvInvoke.IsContourConvex(polygon))
                            {
                                polygon.Dispose();
                                continue;
                            }

                            double area = CvInvoke.ContourArea(hull);

                            if (area < localMinSize || area > localMaxSize)
                            {
                                polygon.Dispose();
                                continue;
                            }

                            // Find the shortest line

                            //List<Line> lines = new List<Line>();

                            Line shortestLine = new Line(double.MaxValue, 0, 0);

                            for (int k = 0; k < 3; k++)
                            {
                                double dx = polygon[k].X - polygon[(k + 1) % 3].X;
                                double dy = polygon[k].Y - polygon[(k + 1) % 3].Y;

                                double lineLen = Math.Sqrt(dx * dx + dy * dy);

                                double slopeRad = Math.Atan2(dy, dx);

                                if (lineLen < shortestLine.Length)
                                {
                                    // Find the opposite point
                                    int oppositeIndex = GetOppositePoint(k, (k + 1) % 3);
                                    /*
                                    Point opPoint = new Point(polygon[oppositeIndex].X, polygon[oppositeIndex].Y);

                                    Matrix matrix = new Matrix();
                                    matrix.Rotate((float)ToDegrees(slopeRad), MatrixOrder.Append);

                                    Mat tmpMat = new Mat();
                                    CvInvoke.GetRotationMatrix2D(new PointF(), ToDegrees(slopeRad), 1, tmpMat);

                                    VectorOfPoint rotPoitns = new VectorOfPoint(3);

                                    CvInvoke.WarpAffine(polygon, rotPoitns, tmpMat, );
                                    */

                                    /*
                                    Point p1 = polygon[k];
                                    Point p2 = polygon[(k + 1) % 3];
                                    Point p3 = polygon[oppositeIndex];

                                    double dotprod = (p3.X - p1.X) * (p2.X - p1.X) + (p3.Y - p1.Y) * (p2.Y - p1.Y);
                                    double len1squared = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
                                    double len2squared = (p3.X - p1.X) * (p3.X - p1.X) + (p3.Y - p1.Y) * (p3.Y - p1.Y);
                                    double angle = Math.Acos(dotprod / Math.Sqrt(len1squared * len2squared));
                                    */

                                    //double slopeDegrees = (ToDegrees(slopeRad));

                                    Vector2d pa = new Vector2d(polygon[oppositeIndex].X - polygon[k].X,
                                        polygon[oppositeIndex].Y - polygon[k].Y);
                                    ;
                                    Vector2d d = new Vector2d(dx, dy);
                                    d.Normalize();
                                    double dot = Vector2d.Dot(pa, d);

                                    Vector2d multiplied = Vector2d.Multiply(d, dot);

                                    Point x = new Point((int)(polygon[k].X + multiplied.X), (int)(polygon[k].Y + multiplied.Y));


                                    double newDx = polygon[oppositeIndex].X - x.X;
                                    double newDy = polygon[oppositeIndex].Y - x.Y;




                                    //if (polygon[oppositeIndex].X ) 




                                    shortestLine = new Line(lineLen, ToDegrees(Math.Atan2(newDy, newDx)), ToDegrees(slopeRad));
                                }



                            }



                            // Shortest line in our base
                            CvInvoke.PutText(display, "Length " + shortestLine.Length, TextPoint, FontFace.HersheyPlain, 2, Blue);
                            CvInvoke.PutText(display, "Degrees " + shortestLine.Slope, TextPoint2, FontFace.HersheyPlain, 2, Blue);
                            CvInvoke.PutText(display, "Degrees2 " + shortestLine.OtherSlope, TextPoint3, FontFace.HersheyPlain, 2, Blue);

                            VectorOfVectorOfPoint cont = new VectorOfVectorOfPoint(1);
                            cont.Push(polygon);

                            CvInvoke.DrawContours(display, cont, -1, Blue, 2);

                            for (int j = 0; j < cont.Size; j++)
                            {
                                cont[j].Dispose();
                            }
                            cont.Dispose();

                            polygon.Dispose();

                        }

                        for (int i = 0; i < contours.Size; i++)
                        {
                            contours[i].Dispose();
                        }
                        contours.Clear();

                        for (int i = 0; i < convexHulls.Size; i++)
                        {
                            convexHulls[i].Dispose();
                        }
                        convexHulls.Dispose();

                        CvInvoke.Resize(display, rawDisplayImage, new Size(), 2.0, 2.0);
                    }



                    CvInvoke.Imshow("RawDisplay", rawDisplayImage);
                    CvInvoke.Imshow("HSV", hsvConvert);
                    CvInvoke.Imshow("Filtered", inRangeMat);
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

        private static int GetOppositePoint(int index1, int index2)
        {
            if (index1 == 0 && index2 == 1) return 2;
            else if (index1 == 0 && index2 == 2) return 1;
            else if (index1 == 1 && index2 == 2) return 0;
            else if (index2 == 0 && index1 == 1) return 2;
            else if (index2 == 0 && index1 == 2) return 1;
            else return 0;
        }

        static Point TextPoint = new Point(0, 20);
        static Point TextPoint2 = new Point(0, 50);
        static Point TextPoint3 = new Point(0, 80);

        private static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
