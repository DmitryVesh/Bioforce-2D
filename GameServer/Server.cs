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
        private static UdpClient UDPClient { get; set; }

        public delegate void PacketHandler(int clientID, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary;

        public static void StartServer(int maxNumPlayers, int portNum)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnEndingConsoleApplication);
            (MaxNumPlayers, PortNum) = (maxNumPlayers, portNum);

            Console.WriteLine("Trying to start the server...");
            InitServerData();

            TCPListener = new TcpListener(IPAddress.Any, portNum);
            TCPListener.Start();
            TCPBeginAcceptClient();

            
            UDPClient = new UdpClient(PortNum);
            UDPBeginReceive();

            Console.WriteLine($"\nServer started... " +
                $"\n\tPort number:  {PortNum}" +
                $"\n\tMax Players:  {MaxNumPlayers}");
        }
        public static void SendUDPPacket(IPEndPoint clientIPEndPoint, Packet packet)
        {
            try
            {
                if (clientIPEndPoint != null)
                {
                    UDPClient.BeginSend(packet.ToArray(), packet.Length(), clientIPEndPoint, null, null);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, sending data to Client from Server: {clientIPEndPoint} via UDP.\nException {exception}");
            }
        }
        
        private static void InitServerData()
        {
            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                ClientDictionary.Add(count, new Client(count));
            }

            PacketHandlerDictionary = new Dictionary<int, PacketHandler>();
            PacketHandlerDictionary.Add((int)ClientPackets.welcomeReceived, ServerRead.WelcomeRead);
            PacketHandlerDictionary.Add((int)ClientPackets.udpTestReceived, ServerRead.UDPTestRead);
            PacketHandlerDictionary.Add((int)ClientPackets.playerMovement, ServerRead.PlayerMovementRead);
            PacketHandlerDictionary.Add((int)ClientPackets.playerMovementStats, ServerRead.PlayerMovementStatsRead);
            PacketHandlerDictionary.Add((int)ClientPackets.bulletShot, ServerRead.ShotBulletRead);
            PacketHandlerDictionary.Add((int)ClientPackets.playerDied, ServerRead.PlayerDiedRead);
            PacketHandlerDictionary.Add((int)ClientPackets.playerRespawned, ServerRead.PlayerRespawnedRead);
            PacketHandlerDictionary.Add((int)ClientPackets.tookDamage, ServerRead.TookDamageRead);

            Console.WriteLine("Initialised server packets.");
        }

        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPListener.EndAcceptTcpClient(asyncResult);
            TCPBeginAcceptClient();
            Console.WriteLine($"\nUser {client.Client.RemoteEndPoint} is trying to connect...");

            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (ClientDictionary[count].tCP.Socket == null)
                {
                    ClientDictionary[count].tCP.Connect(client);
                    Console.WriteLine($"Sent welcome packet to: {count}");
                    ServerSend.Welcome(count, $"Welcome to the server client: {count}");
                    return;
                }
            }
            Console.WriteLine($"\nThe server is full... {client.Client.RemoteEndPoint} couldn't connect...");
        }
        private static void TCPBeginAcceptClient()
        {
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        }

        private static void UDPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                IPEndPoint clientIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = UDPClient.EndReceive(asyncResult, ref clientIPEndPoint);
                UDPBeginReceive();

                if (data.Length < 4)
                {
                    return;
                }

                Packet packet = new Packet(data);
                int clientID = packet.ReadInt();
                if (clientID == 0)
                {
                    return;
                }
                if (ClientDictionary[clientID].uDP.ipEndPoint == null)
                {
                    ClientDictionary[clientID].uDP.Connect(clientIPEndPoint);
                    return;
                }

                if (ClientDictionary[clientID].uDP.ipEndPoint.ToString() == clientIPEndPoint.ToString())
                {
                    ClientDictionary[clientID].uDP.HandlePacket(packet);
                }
            }
            catch (Exception exception)
            {
                //TODO: maybe Disconnect client, error caused when 2 or more clients disconnect at same time.
                Console.WriteLine($"Error, in UDP data:\n{exception}");
            }
        }
        private static void UDPBeginReceive()
        {
            UDPClient.BeginReceive(UDPConnectAsyncCallback, null);
        }

        private static void OnEndingConsoleApplication(object sender, EventArgs e)
        {
            TCPListener.Stop();
            UDPClient.Close();
        }
        
    }
}
