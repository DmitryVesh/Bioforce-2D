using System;
using System.Threading;
using Shared;
using System.IO;

namespace MainServerBioforce2D
{
    class Program
    {
        public const int Ticks = 30;
        public const double MillisecondsInTick = (double)1000 / Ticks;

        const int PortRelease = 28020;
        const int PortTesting = 28420;

        static int PortInUse;

        static bool IsRunning = true;

        public static string GameServerFileName { get; private set; }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args.Length == 3)
            {
                GameServerFileName = args[0];
                bool isTestBuild = bool.Parse(args[1]);
                string version = args[2];

                Output.Init(version);

                PortInUse = isTestBuild ? PortTesting : PortRelease;
                InternetDiscoveryTCPServer.StartServer(PortInUse, version);

                Thread mainThread = new Thread(new ThreadStart(MainThread));
                mainThread.Start();
            }
            else
            {
                throw new Exception("Not entered the Proper args... Need to Enter \"GameServer file name\", isTesting either \"true\" or \"false\", \"version number\" to start in MainServer args...");
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
                $"\nData ---" +
                $"\n{exc.Data}" +
                $"\n--- End Data" +
                $"\n--- End Unhandled Exception: {now}" +
                $"\n===========================================================";

            using (StreamWriter sw = new StreamWriter("excLog.txt", true)) 
                sw.Write(logMessage);            
        }

        private static void MainThread()
        {
            Output.WriteLine($"\nStarted main thread. Tick/second {Ticks}");
            //DateTime TickTimer = DateTime.Now;

            TimeSpan tickTime = TimeSpan.FromMilliseconds(MillisecondsInTick);
            while (IsRunning)
            {
                ThreadManager.UpdateMain();
                //Thread.Sleep(MillisecondsInTick); // This error occurs sometimes?? Why
                                                    // Number must be either non-negative and less than or equal to Int32.MaxValue or -1. (Parameter 'timeout')
                                                    // Switched to timeSpan to try and solve
                try
                {
                    Thread.Sleep(tickTime);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Output.WriteLine(   $"\nIt happened again!!!! The weird thread.sleep error..." +
                                        $"\nError: {e}" +
                                        $"\nData: {e.Data}" +
                                        $"\nActual Value: {e.ActualValue}");
                }

                //while (TickTimer < DateTime.Now)
                //{
                //    ThreadManager.UpdateMain();
                //    TickTimer = TickTimer.AddMilliseconds(MillisecondsInTick);

                //    if (TickTimer > DateTime.Now)
                //    {
                //        //TimeSpan timespanToWait = TickTimer - DateTime.Now;
                //        //int ms = (int)timespanToWait.TotalMilliseconds;
                //        //if (ms)
                //        Thread.Sleep(TickTimer - DateTime.Now);
                //    }
                //}
            }
        }
    }
}
