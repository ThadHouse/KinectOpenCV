using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NetworkTables;
using NetworkTables.Tables;

namespace KinectOpenCV
{
    class TriangleColorSearch
    {
        private ITable table;

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


        public TriangleColorSearch(string tablePrefix)
        {
            table = NetworkTable.GetTable(tablePrefix);

            table.SetDefaultNumberArray(HSVLow, defaultLow);
            table.SetDefaultNumberArray(HSVHigh, defaultHigh);

            table.SetPersistent(HSVLow);
            table.SetPersistent(HSVHigh);

            table.SetDefaultNumber(minSize, defaultMinSize);
            table.SetDefaultNumber(maxSize, defaultMaxSize);

            table.SetPersistent(minSize);
            table.SetPersistent(maxSize);
        }

        public void TrackImage(Mat rawImage)
        {
            if (rawImage == null) return;

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

            using (var toDisplay = new Mat())
            
            {
                CvInvoke.Flip(rawImage, toDisplay, FlipType.Horizontal);


            }
        }
    }
}
