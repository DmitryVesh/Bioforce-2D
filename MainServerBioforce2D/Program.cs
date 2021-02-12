using System;
using System.Threading;
using Shared;
using GameServer;
using System.Linq;

namespace MainServerBioforce2D
{
    class Program
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        const int Port = 28020;
        static bool IsRunning = true;

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "MainServer")
            {
                InternetDiscoveryTCPServer.StartServer(Port);
                Thread mainThread = new Thread(new ThreadStart(MainThread));
                mainThread.Start();
            }
            else if (args[0] == "GameServer")
            {
                args = args.Skip(1).ToArray();
                ServerProgram.StartServerProgram(args);
            }
        }

        private static void MainThread()
        {
            Console.WriteLine($"\nStarted main thread. Tick/second {Ticks}");
            DateTime TickTimer = DateTime.Now;

            while (IsRunning)
            {
                while (TickTimer < DateTime.Now)
                {
                    ThreadManager.UpdateMain();
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
