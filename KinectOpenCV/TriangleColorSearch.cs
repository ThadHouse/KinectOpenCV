using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NetworkTables;
using NetworkTables.Tables;
using OpenTK;

namespace KinectOpenCV
{
    class TriangleColorSearch : SearcherBase
    {

        static readonly MCvScalar Red = new MCvScalar(0, 0, 255);
        static readonly MCvScalar Green = new MCvScalar(0, 255, 0);
        static readonly MCvScalar Blue = new MCvScalar(255, 0, 0);

        const string HSVLow = "HSVLow";
        const string HSVHigh = "HSVHigh";
        const string minSize = "minSize";
        const string maxSize = "maxSize";

        VectorOfDouble arrayLow = new VectorOfDouble(3);
        VectorOfDouble arrayHigh = new VectorOfDouble(3);

        static readonly double[] defaultLow = new double[] { 50, 44, 193 };
        static readonly double[] defaultHigh = new double[] { 90, 255, 255 };

        private const double defaultMaxSize = 500;
        private const double defaultMinSize = 100;


        public TriangleColorSearch(string tablePrefix) : base(tablePrefix)
        {

            table.SetDefaultNumberArray(HSVLow, defaultLow);
            table.SetDefaultNumberArray(HSVHigh, defaultHigh);

            table.SetPersistent(HSVLow);
            table.SetPersistent(HSVHigh);

            table.SetDefaultNumber(minSize, defaultMinSize);
            table.SetDefaultNumber(maxSize, defaultMaxSize);

            table.SetPersistent(minSize);
            table.SetPersistent(maxSize);
        }

        private Mat HsvDisplay = new Mat();
        private Mat InRangeDisplay = new Mat();
        private VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

        // Will already be flipped
        public override TrackedRobot? FindTarget(Mat rawImage)
        {
            if (rawImage == null) return null;

            double[] ntLow = table.GetNumberArray(HSVLow, defaultLow);
            double[] ntHigh = table.GetNumberArray(HSVHigh, defaultHigh);

            double localMinSize = table.GetNumber(minSize, defaultMinSize);
            double localMaxSize = table.GetNumber(maxSize, defaultMaxSize);

            if (ntLow.Length != 3)
                ntLow = defaultLow;
            if (ntHigh.Length != 3)
                ntHigh = defaultHigh;

            arrayLow.Clear();
            arrayLow.Push(ntLow);
            arrayHigh.Clear();
            arrayHigh.Push(ntHigh);

            using (var temp = new Mat())

            {
                CvInvoke.CvtColor(rawImage, HsvDisplay, ColorConversion.Bgr2Hsv);

                CvInvoke.InRange(HsvDisplay, arrayLow, arrayHigh, InRangeDisplay);

                InRangeDisplay.ConvertTo(temp, DepthType.Cv8U);

                CvInvoke.FindContours(temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);

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

                    Line shortestLine = new Line(double.MaxValue, 0, 0);

                    for (int k = 0; k < 3; k++)
                    {
                        double dx = polygon[k].X - polygon[(k + 1)%3].X;
                        double dy = polygon[k].Y - polygon[(k + 1)%3].Y;

                        double lineLen = Math.Sqrt(dx*dx + dy*dy);

                        double slopeRad = Math.Atan2(dy, dx);

                        if (lineLen < shortestLine.Length)
                        {
                            // Find the opposite point
                            int oppositeIndex = GetOppositePoint(k, (k + 1)%3);

                            Vector2d pa = new Vector2d(polygon[oppositeIndex].X - polygon[k].X,
                                polygon[oppositeIndex].Y - polygon[k].Y);
                            ;
                            Vector2d d = new Vector2d(dx, dy);
                            d.Normalize();
                            double dot = Vector2d.Dot(pa, d);

                            Vector2d multiplied = Vector2d.Multiply(d, dot);

                            Point x = new Point((int) (polygon[k].X + multiplied.X), (int) (polygon[k].Y + multiplied.Y));


                            double newDx = polygon[oppositeIndex].X - x.X;
                            double newDy = polygon[oppositeIndex].Y - x.Y;


                            shortestLine = new Line(lineLen, ToDegrees(Math.Atan2(newDy, newDx)), ToDegrees(slopeRad));
                        }
                    }
                }
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
