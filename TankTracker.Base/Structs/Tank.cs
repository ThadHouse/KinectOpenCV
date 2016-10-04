using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankTracker.Base.Structs
{
    public struct Tank
    {
        public double Angle { get; }
        public int X { get; }
        public int Y { get; }

        public Tank(double angle, int x, int y)
        {
            Angle = angle;
            X = x;
            Y = y;
        }
    }
}
