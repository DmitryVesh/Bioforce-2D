using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServer
{
    class MainServerCommsRead
    {
        public static void Welcome(Packet packet)
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
