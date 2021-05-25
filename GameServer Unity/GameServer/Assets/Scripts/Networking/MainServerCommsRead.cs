using Shared;
using System.Threading;
using UnityEngine.Output;

namespace GameServer
{
    class MainServerCommsRead
    {
        public static void Welcome(Packet _)
        {
            Output.WriteLine("\t\tGameServer reading welcome packet from MainServer");
            MainServerComms.EstablishedConnection = true;

            //Thread updateThread = new Thread(new ThreadStart(MainServerComms.Update));
            //updateThread.Start();
            MainServerComms.StartSendingServerData();
            
            MainServerCommsSend.WelcomeReceived(MainServerComms.ServerName);
            Output.WriteLine("\t\tGameServer recieved welcome packet from MainServer");
        }
    }
}
