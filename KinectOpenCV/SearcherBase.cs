using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using NetworkTables;
using NetworkTables.Tables;

namespace KinectOpenCV
{
    struct TrackedRobot
    {
        public double Angle { get; }
        public Point Location { get; }
    }
    
    abstract class SearcherBase
    {
        protected ITable table;

        protected SearcherBase(string tablePrefix)
        {
            table = NetworkTable.GetTable(tablePrefix);
        }

        public abstract TrackedRobot FindTarget(Mat rawImage);
    }
}
