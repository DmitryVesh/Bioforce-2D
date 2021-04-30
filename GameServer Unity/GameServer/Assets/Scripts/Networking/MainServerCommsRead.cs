using Shared;
using System.Threading;
using UnityEngine.Output;

namespace GameServer
{
    class MainServerCommsRead
    {
        public static void Welcome(Packet _)
        {
            Output.WriteLine("\n\t\tGameServer reading welcome packet from MainServer");
            MainServerComms.EstablishedConnection = true;
            Thread updateThread = new Thread(new ThreadStart(MainServerComms.Update));
            updateThread.Start();
            MainServerCommsSend.WelcomeReceived(MainServerComms.ServerName);
            Output.WriteLine("\n\t\tGameServer recieved welcome packet from MainServer");
        }
    }
}
