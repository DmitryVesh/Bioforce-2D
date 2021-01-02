using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace GameServer
{
    static class Program
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        private static bool isRunning = true;

        static void Main()
        {
            string programTitleName = "GameServer";
            Console.Title = programTitleName;

            int maxNumPlayers = 10;
            int portNumGame = 28020; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
            int portNumDiscover = portNumGame + 1;

            DiscoveryServer discoveryServer = new DiscoveryServer();
            discoveryServer.StartServer(portNumDiscover);

            Server.StartServer(maxNumPlayers, portNumGame);

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

        }
        private static void MainThread()
        {
            Console.WriteLine($"\nStarted main thread. Tick/second {Ticks}");
            DateTime TickTimer = DateTime.Now;

            while (isRunning)
            {
                while (TickTimer < DateTime.Now)
                {
                    GameLogic.Update();
                    TickTimer = TickTimer.AddMilliseconds(MillisecondsInTick);
                    
                    if (TickTimer > DateTime.Now)
                    {
                        Thread.Sleep(TickTimer - DateTime.Now);
                    }
                }
            }
        }
    }
}
