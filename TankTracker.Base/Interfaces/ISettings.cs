using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankTracker.Base.Structs;

namespace TankTracker.Base.Interfaces
{
    public interface ISettings
    {
        event EventHandler<double> OnExposureTimeChanged;

        event EventHandler<double> OnContrastChanged;

        event EventHandler<int> OnWhiteBalanceChanged;

        int WhiteBalance { get; set; }

        double Contrast { get; set; }

        double ExposureTime { get; set; }

        void SetupWhiteBalance(NetworkedValue<int> whiteBalanceValues);

        void SetupContrast(NetworkedValue<double> contrastValue);

        void SetupExposureTime(NetworkedValue<double> exposureTimeValue);

        NetworkedValue<int> GetWhiteBalanceConfiguration();

        NetworkedValue<double> GetConstrastConfiguration();

        NetworkedValue<double> GetExposureTimeConfiguration();
    }
}
