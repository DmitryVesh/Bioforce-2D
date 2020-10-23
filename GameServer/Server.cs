using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        public static int MaxNumPlayers { get; private set; }
        public static int PortNum { get; private set; }
        public static Dictionary<int, Client> ClientDictionary = new Dictionary<int, Client>();
        private static TcpListener TCPListener { get; set; }

        public static void StartServer(int maxNumPlayers, int portNum)
        {
            (MaxNumPlayers, PortNum) = (maxNumPlayers, portNum);

            Console.WriteLine("Trying to start the server...");
            InitServerData();

            TCPListener = new TcpListener(IPAddress.Any, portNum);
            TCPListener.Start();
            TCPBeginAcceptClient();

            Console.WriteLine($"\nServer started... " +
                $"\n\tPort number:  {PortNum}" +
                $"\n\tMax Players:  {MaxNumPlayers}");
        }

        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPListener.EndAcceptTcpClient(asyncResult);
            TCPBeginAcceptClient();
            Console.WriteLine($"User {client.Client.RemoteEndPoint} is trying to connect...");

            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (ClientDictionary[count].tCP.Socket == null)
                {
                    ClientDictionary[count].tCP.Connect(client);
                    PacketSender.Welcome(count, $"Welcome to the server client {count}");
                    return;
                }
            }
            Console.WriteLine($"The server is full... {client.Client.RemoteEndPoint} couldn't connect...");
        }
        private static void TCPBeginAcceptClient()
        {
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        }

        private static void InitServerData()
        {
            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                ClientDictionary.Add(count, new Client(count));
            }
        }
    }
}
