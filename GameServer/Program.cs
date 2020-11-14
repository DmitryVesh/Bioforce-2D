using System;
using System.Threading;

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
            int portNum = 28020; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
            Server.StartServer(maxNumPlayers, portNum);

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
