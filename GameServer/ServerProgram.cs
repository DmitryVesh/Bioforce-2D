using System;
using System.Threading;
using Shared;

namespace GameServer
{
    public class ServerProgram
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        public static bool IsRunning { get; private set; } = true;

        static void Main() { }

        public static void StartServerProgram(string[] args)
        {
            (string serverName, int maxNumPlayers, string mapName, int port) = (args[0], int.Parse(args[1]), args[2], int.Parse(args[3]));
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            try
            {
                //int portNumLANDiscover = 28021;
                //DiscoveryServer.StartServer(portNumLANDiscover);
                Server.StartServer(serverName, maxNumPlayers, mapName, port);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError starting server:\n{exception}");
            }
        }

        private static void MainThread()
        {
            Console.WriteLine($"\n\tStarted main thread. Tick/second {Ticks}");
            DateTime TickTimer = DateTime.Now;

            while (IsRunning)
            {
                while (TickTimer < DateTime.Now)
                {
                    GameLogic.Update();
                    ThreadManager.UpdateMain();

                    TickTimer = TickTimer.AddMilliseconds(MillisecondsInTick);

                    if (TickTimer > DateTime.Now)
                    {
                        Thread.Sleep(TickTimer - DateTime.Now);
                    }
                }
            }
            Console.WriteLine("\tEnding server");
        }
    }
}
