using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankTracker.Base.Interfaces;
using Microsoft.Kinect;
using TankTracker.Base.Structs;
using TankTracker.Networking;

namespace KinectOpenCV
{
    public class KinectNetworkSettings
    {
        private ISettings networkSettings;

        private ColorCameraSettings cameraSettings;

        const double defaultExposure = 22;
        const double defaultContrast = 1.0;
        const int defaultWhiteBalance = 2700;

        const double minExposure = 0.0;
        const double maxExposure = 40000;

        const double minContrast = 0.5;
        const double maxContrast = 2.0;

        const int minWhiteBalance = 2700;
        const int maxWhiteBalance = 6500;

        public KinectNetworkSettings(INetworkTableProvider provider, ColorCameraSettings cameraSettings)
        {
            networkSettings = new Settings(provider);

            this.cameraSettings = cameraSettings;

            cameraSettings.AutoExposure = false;
            cameraSettings.AutoWhiteBalance = false;
            //cameraSettings.BacklightCompensationMode = BacklightCompensationMode.LowlightsPriority;

            networkSettings.SetupContrast(new NetworkedValue<double>(minContrast, maxContrast, defaultContrast));
            networkSettings.SetupExposureTime(new NetworkedValue<double>(minExposure, maxExposure, defaultExposure));
            networkSettings.SetupWhiteBalance(new NetworkedValue<int>(minWhiteBalance, maxWhiteBalance, defaultWhiteBalance));

            networkSettings.OnWhiteBalanceChanged += (sender, i) =>
            {
                cameraSettings.WhiteBalance = i;
            };

            networkSettings.OnContrastChanged += (sender, d) =>
            {
                cameraSettings.Contrast = d;
            };

            networkSettings.OnExposureTimeChanged += (sender, d) =>
            {
                cameraSettings.ExposureTime = d;
            };
        }
    }
}
