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

        static int PortRelease = 28020;
        static int PortTesting = PortRelease + InternetDiscoveryTCPServer.NumMaxServers + 1;

        static int PortInUse;

        static bool IsRunning = true;

        public static string GameServerFileName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">GameServer filename (string), is test build (bool), version num (string), main ip (string)</param>
        /// <exception cref="Exception"></exception>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            string arg0;
            string arg1;
            //string arg2;
            string message;
            switch (args.Length)
            {
                case 1:
                    arg0 = args[0].ToLower();
                    if (arg0 == "help")
                    {
                        message = "" +
                            "List of available commands:" +
                            "\n" +
                            "\nhelp         - gets all commands available" +
                            "\nhelp start   - gives example args to start MainServer" +
                            "\nip           - gets server's Public IP address";
                        Console.WriteLine(message);
                        return;
                    }
                    else if (arg0 == "ip")
                    {
                        message = "" +
                            $"{InternetDiscoveryTCPServer.GetServerIP()}";
                        Console.WriteLine(message);
                        return;
                    }

                    break;


                case 2:
                    arg0 = args[0].ToLower();
                    arg1 = args[1].ToLower();
                    if (args[0] != "help" || args[1] != "start")
                        break;

                    // help start
                    message = "" +
                        $"To start Main Server:" +
                        $"\n\tGameServer filename (string), is test build (bool), version num (string)" +//, main ip (string)" +                        
                        $"\n\tGameServer.{OS.GetAppExtension()} true 1.0.2" +// {InternetDiscoveryTCPServer.GetServerIP()}" +
                        $"\n" +
                        $"\nPorts used:" +
                        $"\n\tRelease {PortRelease} - {InternetDiscoveryTCPServer.GetMaxPortInclusive(PortRelease)}" +
                        $"\n\tTesting {PortTesting} - {InternetDiscoveryTCPServer.GetMaxPortInclusive(PortTesting)}";
                    Console.WriteLine(message);
                    return;


                case 3:
                    GameServerFileName = args[0];
                    bool isTestBuild = bool.Parse(args[1]);
                    string version = args[2];
                    //string mainIP = args[3];

                    Output.Init(version);

                    PortInUse = isTestBuild ? PortTesting : PortRelease;
                    InternetDiscoveryTCPServer.StartServer(PortInUse, version);

                    Thread mainThread = new Thread(new ThreadStart(MainThread));
                    mainThread.Start();
                    return;

                default:
                    break;
            }

            throw new Exception("" +
              "Not entered the Proper args..." +
              "\nRun program with \"help\" as argument to find list of commands");
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
