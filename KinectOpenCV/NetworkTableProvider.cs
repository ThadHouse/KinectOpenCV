using System;
using NetworkTables.Independent;
using NetworkTables.Tables;
using TankTracker.Networking;

namespace KinectOpenCV
{
    public class NetworkTableProvider : INetworkTableProvider
    {
        public ITable GetRootTable => independentNetworkTable;
        public IndependentNtCore GetRootNtCore => independentNtCore;


        private IndependentNetworkTable independentNetworkTable;
        private IndependentRemoteProcedureCall independentRemoteProcedureCall;
        private IndependentNtCore independentNtCore;

        public NetworkTableProvider()
        {
            independentNtCore = new IndependentNtCore();
            independentNtCore.StartServer("KinectOpenCv.txt", "", IndependentNtCore.DefaultPort);
            independentNtCore.UpdateRate = 50;

            independentNetworkTable = new IndependentNetworkTable(independentNtCore, "KinectOpenCv");
        }


    }
}
