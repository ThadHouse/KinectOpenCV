using System;
using NetworkTables;
using NetworkTables.Independent;
using NetworkTables.Tables;
using TankTracker.Base.Interfaces;
using TankTracker.Base.Structs;

namespace TankTracker.Networking
{
    public class Settings : ISettings
    {
        public event EventHandler<double> OnExposureTimeChanged;
        public event EventHandler<double> OnContrastChanged;
        public event EventHandler<int> OnWhiteBalanceChanged;

        public int WhiteBalance
        {
            get { return (int)settingsTable.GetNumber(WhiteBalanceKey, 2700); }
            set
            {
                var config = GetWhiteBalanceConfiguration();
                double setVal;
                InRange(value, config.MinimumValue, config.MaximumValue, out setVal);
                settingsTable.PutNumber(WhiteBalanceKey, setVal);
                OnWhiteBalanceChanged?.Invoke(this, (int)setVal);
            }
        }

        public double Contrast
        {
            get { return settingsTable.GetNumber(ContrastKey, 1.0); }
            set
            {
                var config = GetConstrastConfiguration();
                double setVal;
                InRange(value, config.MinimumValue, config.MaximumValue, out setVal);
                settingsTable.PutNumber(ContrastKey, setVal);
                OnContrastChanged?.Invoke(this, setVal);
            }
        }

        public double ExposureTime
        {
            get { return settingsTable.GetNumber(ExposureTimeKey, 22); }
            set
            {
                var config = GetExposureTimeConfiguration();
                double setVal;
                InRange(value, config.MinimumValue, config.MaximumValue, out setVal);
                settingsTable.PutNumber(ExposureTimeKey, setVal);
                OnExposureTimeChanged?.Invoke(this, setVal);
            }
        }


        public void SetupWhiteBalance(NetworkedValue<int> whiteBalanceValues)
        {
            settingsTable.PutNumber(WhiteBalanceMinimumKey, whiteBalanceValues.MinimumValue);
            settingsTable.PutNumber(WhiteBalanceMaximumKey, whiteBalanceValues.MaximumValue);
            settingsTable.SetDefaultNumber(WhiteBalanceKey, whiteBalanceValues.DefaultValue);
        }

        public void SetupContrast(NetworkedValue<double> contrastValue)
        {
            settingsTable.PutNumber(ContrastMinimumKey, contrastValue.MinimumValue);
            settingsTable.PutNumber(ContrastMaximumKey, contrastValue.MaximumValue);
            settingsTable.SetDefaultNumber(ContrastKey, contrastValue.DefaultValue);
        }

        public void SetupExposureTime(NetworkedValue<double> exposureTimeValue)
        {
            settingsTable.PutNumber(ExposureMinimumKey, exposureTimeValue.MinimumValue);
            settingsTable.PutNumber(ExposureMaximumKey, exposureTimeValue.MaximumValue);
            settingsTable.SetDefaultNumber(ExposureTimeKey, exposureTimeValue.DefaultValue);
        }

        public NetworkedValue<int> GetWhiteBalanceConfiguration()
        {
            double min = settingsTable.GetNumber(WhiteBalanceMinimumKey, 2700);
            double max = settingsTable.GetNumber(WhiteBalanceMaximumKey, 6500);

            return new NetworkedValue<int>((int)min, (int)max, 2700);
        }

        public NetworkedValue<double> GetConstrastConfiguration()
        {
            double min = settingsTable.GetNumber(ContrastMinimumKey, 0.5);
            double max = settingsTable.GetNumber(ContrastMaximumKey, 2.0);

            return new NetworkedValue<double>(min, max, 1.0);
        }

        public NetworkedValue<double> GetExposureTimeConfiguration()
        {
            double min = settingsTable.GetNumber(ExposureMinimumKey, 0.0);
            double max = settingsTable.GetNumber(ExposureMaximumKey, 40000);

            return new NetworkedValue<double>(min, max, 22);
        }

        private string ExposureTimeKey => "ExposureTime";
        private string ContrastKey => "Contrast";
        private string WhiteBalanceKey => "WhiteBalance";

        private string ExposureMinimumKey => ExposureTimeKey + "Minimum";
        private string ExposureMaximumKey => ExposureTimeKey + "Maximum";

        private string ContrastMinimumKey => ContrastKey + "Minimum";
        private string ContrastMaximumKey => ContrastKey + "Maximum";

        private string WhiteBalanceMinimumKey => WhiteBalanceKey + "Minimum";
        private string WhiteBalanceMaximumKey => WhiteBalanceKey + "Maximum";

        private INetworkTableProvider tableProvider;
        private ITable settingsTable;

        public Settings(INetworkTableProvider networkTableProvider)
        {
            tableProvider = networkTableProvider;

            settingsTable = new IndependentNetworkTable(tableProvider.GetRootNtCore, nameof(Settings));

            // Hook up our limits

            settingsTable.AddTableListener(ExposureTimeKey, (table1, key, value, flags) =>
            {
                ExposureTime = value.GetDouble();
            }, true); 

            settingsTable.AddTableListener(ContrastKey, (table1, key, value, flags) =>
            {
                Contrast = value.GetDouble();
            }, true);

            settingsTable.AddTableListener(WhiteBalanceKey, (table1, key, value, flags) =>
            {
                WhiteBalance = (int)value.GetDouble();
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
