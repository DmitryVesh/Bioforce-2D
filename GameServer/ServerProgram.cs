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
        private static int PortGame { get; set; }

        static void Main() { }

        public static void StartServerProgram(string[] args)
        {
            (string serverName, int maxNumPlayers, string mapName, int portGame, int portMainServer, int timeOut) 
                = (args[0], int.Parse(args[1]), args[2], int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]));

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            PortGame = portGame;

            try
            {
                MainServerComms.Connect(portMainServer, serverName, timeOut);
                Server.StartServer(serverName, maxNumPlayers, mapName, portGame);
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
                        Thread.Sleep(TickTimer - DateTime.Now);
                }
            }
            Console.WriteLine($"\tEnding server: {PortGame}");
        }
    }
}
