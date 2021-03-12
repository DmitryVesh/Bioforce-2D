using System;
using System.Threading;
using Shared;
using GameServer;
using System.Linq;
using System.IO;

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exc = (Exception)args.ExceptionObject;
            DateTime now = DateTime.Now;
            string logMessage =
                $"\n\n" +
                $"\nUnhandled Exception ---" +
                $"\n===========================================================" +
                $"\n{now}" +
                $"\n" +
                $"\nMessage ---" +
                $"\n{exc.Message}" +
                $"\n--- End Message" +
                $"\n" +
                $"\nSource ---" +
                $"\n{exc.Source}" +
                $"\n--- End Source" +
                $"\n" +
                $"\nStackTrace ---" +
                $"\n{exc.StackTrace}" +
                $"\n--- End StackTrace" +
                $"\n" +
                $"\nTargetSite ---" +
                $"\n{exc.TargetSite}" +
                $"\n--- End TargetSite" +
                $"\n--- End Unhandled Exception: {now}" +
                $"\n===========================================================";

            using (StreamWriter sw = new StreamWriter("excLog.txt", true)) 
                sw.Write(logMessage);            
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
