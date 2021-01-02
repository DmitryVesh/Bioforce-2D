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
            int portNum = 28020; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
            //Server.StartServer(maxNumPlayers, portNum);

            LanManager lanManager = new LanManager();
            lanManager.StartServer(portNum);

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
    

    public class LanManager
    {

        private Socket ServerSocket { get; set; }
        private EndPoint RemoteEndPoint;

        public void StartServer(int port)
        {
            if (ServerSocket == null)
            {
                try
                {
                    ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    if (ServerSocket == null)
                    {
                        Console.WriteLine("Server socket is null");
                        return;
                    }

                    ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    ServerSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackServer), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public void CloseServer()
        {
            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket = null;
            }
        }

        private void AsyncCallbackServer(IAsyncResult result)
        {
            if (ServerSocket != null)
            {
                try
                {
                    int size = ServerSocket.EndReceiveFrom(result, ref RemoteEndPoint);
                    byte[] pongBytes = Encoding.ASCII.GetBytes("pong");

                    ServerSocket.SendTo(pongBytes, RemoteEndPoint);

                    ServerSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackServer), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
