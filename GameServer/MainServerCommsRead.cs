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
            MainServerComms.EstablishedConnection = true;
            Thread updateThread = new Thread(new ThreadStart(MainServerComms.Update));
            updateThread.Start();
            MainServerCommsSend.WelcomeReceived(MainServerComms.ServerName);
            Console.WriteLine("\n\t\tGameServer recieved welcome packet from MainServer");
        }
    }
}
