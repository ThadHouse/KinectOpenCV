using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using NetworkTables;
using NetworkTables.Tables;

namespace KinectOpenCV
{
    public class NetworkSettings
    {
        private ColorCameraSettings settings;

        private ITable table;

        const string exposure = "Exposure";
        const string contrast = "Contrast";
        const string whiteBalance = "WhiteBalance";

        const double defaultExposure = 22;
        const double defaultContrast = 1.0;
        const double defaultWhiteBalance = 2700;

        const double minExposure = 0.0;
        const double maxExposure = 40000;

        const double minContrast = 0.5;
        const double maxContrast = 2.0;

        const double minWhiteBalance = 2700;
        const double maxWhiteBalance = 6500;

        public NetworkSettings(ColorCameraSettings settings)
        {
            this.settings = settings;

            settings.AutoWhiteBalance = false;
            settings.AutoExposure = false;

            // Set our default keys
            table = NetworkTable.GetTable("Settings");

            table.PutNumber("MinimumExposure", minExposure);
            table.PutNumber("MaximumExposure", maxExposure);

            table.PutNumber("MinimumContrast", minContrast);
            table.PutNumber("MaximumContrast", maxContrast);

            table.PutNumber("MinimumWhiteBalance", minWhiteBalance);
            table.PutNumber("MaximumWhiteBalance", maxWhiteBalance);

            table.SetDefaultNumber(exposure, defaultExposure);
            table.SetPersistent(exposure);

            table.SetDefaultNumber(contrast, defaultContrast);
            table.SetPersistent(contrast);

            table.SetDefaultNumber(whiteBalance, defaultWhiteBalance);
            table.SetPersistent(whiteBalance);

            table.AddTableListener(exposure, (table1, key, value, flags) =>
            {
                double setVal;
                if (!InRange(value.GetDouble(), minExposure, maxExposure, out setVal))
                {
                    table1.PutNumber(key, setVal);
                }
                settings.ExposureTime = setVal;
            }, true);

            table.AddTableListener(contrast, (table1, key, value, flags) =>
            {
                double setVal;
                if (!InRange(value.GetDouble(), minContrast, maxContrast, out setVal))
                {
                    table1.PutNumber(key, setVal);
                }
                settings.Contrast = setVal;
            }, true);

            table.AddTableListener(whiteBalance, (table1, key, value, flags) =>
            {
                double setVal;
                if (!InRange(value.GetDouble(), minWhiteBalance, maxWhiteBalance, out setVal))
                {
                    table1.PutNumber(key, setVal);
                }
                settings.WhiteBalance = (int)setVal;
            }, true);
        }

        

        private bool InRange(double newVal, double min, double max, out double setVal)
        {
            if (newVal > max)
            {
                setVal = max;
                return false;
            }
            else if (newVal < min)
            {
                setVal = min;
                return false;
            }
            else
            {
                setVal = newVal;
                return true;
            }
        }


    }
}
