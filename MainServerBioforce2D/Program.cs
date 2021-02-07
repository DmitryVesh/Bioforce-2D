using System;
using System.Threading;

namespace MainServerBioforce2D
{
    class Program
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        const int Port = 28022;
        static bool IsRunning = true;

        static void Main()
        {
            Server server1 = new Server("Hello", 10, "Jesus", 2, 10, "127.1");
            Server server2 = new Server("Jimbo", 24, "GCPD", 2, 10, "127.1");

            InternetDiscoveryTCPServer.StartServer(Port);

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
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
