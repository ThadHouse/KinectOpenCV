using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetworkTables.Tables;
using NetworkTables.Wire;
using TankTracker.Base.Interfaces;
using TankTracker.Base.Structs;

namespace TankTracker.Networking
{
    public class EnemyTanks : ITank
    {
        private const byte structId = 42;

        public event EventHandler<Tank> OnTankLocationChanged;

        public Tank Tank
        {
            get
            {
                IList<byte> raw = tankTable.GetRaw(tankKey, null);
                if (raw == null)
                {
                    return new Tank();
                }
                Tank? tank = DecodeTankArray(raw);
                if (tank != null)
                {
                    return tank.Value;
                }
                else
                {
                    return new Tank();
                }
            }
            set
            {
                byte[] data = EncodeTankArray(value);
                tankTable.PutRaw(tankKey, data);
                OnTankLocationChanged?.Invoke(this, value);
            }
        }

        private string tankKey = "TankLocationArray";

        private INetworkTableProvider tableProvider;
        private ITable tankTable;

        public EnemyTanks(INetworkTableProvider networkTableProvider)
        {
            tableProvider = networkTableProvider;
            tankTable = tableProvider.GetRootTable.GetSubTable(nameof(FriendlyTanks));

            // Hook up our events

            tankTable.AddTableListener(tankKey, (table, key, value, flags) =>
            {
                Tank? tank = DecodeTankArray(value.GetRaw());
                if (tank != null)
                {
                    // Since tank just writes to NT, only fire our event
                    OnTankLocationChanged?.Invoke(this, tank.Value);
                }
            }, true);
        }

        private Tank? DecodeTankArray(IList<byte> array)
        {
            using (MemoryStream stream = new MemoryStream(array.ToArray()))
            {
                WireDecoder decoder = new WireDecoder(stream, 0x0300);

                byte id = 0;
                decoder.Read8(ref id);
                if (id != structId)
                {
                    return null;
                }

                double angle = 0;
                uint x = 0;
                uint y = 0;
                if (!decoder.ReadDouble(ref angle))
                {
                    return null;
                }
                if (!decoder.Read32(ref x))
                {
                    return null;
                }
                if (!decoder.Read32(ref y))
                {
                    return null;
                }
                return new Tank(angle, (int)x, (int)y);
            }
        }

        private byte[] EncodeTankArray(Tank tankPos)
        {
            WireEncoder encoder = new WireEncoder(0x0300);
            encoder.Write8(structId);
            encoder.WriteDouble(tankPos.Angle);
            encoder.Write32((uint)tankPos.X);
            encoder.Write32((uint)tankPos.Y);
            return encoder.Buffer;
        }
    }
}
