using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankTracker.Base.Structs;

namespace TankTracker.Base.Interfaces
{
    public interface ITank
    {
        event EventHandler<Tank> OnTankLocationChanged;

        Tank Tank { get; }
    }
}
