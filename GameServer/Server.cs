using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace GameServer
{
    static class Server
    {
        public static string ServerName { get; private set; }
        public static int MaxNumPlayers { get; private set; }
        public static string MapName { get; private set; }
        public static int PortNum { get; private set; }        

        public static Dictionary<int, ClientServer> ClientDictionary = new Dictionary<int, ClientServer>();
        public static Dictionary<int, ClientServer> NotConnectedClients = new Dictionary<int, ClientServer>();

        private static TcpListener TCPListener { get; set; }
        private static UdpClient UDPClient { get; set; }

        public delegate void PacketHandler(int clientID, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary;

        public static int GetCurrentNumPlayers()
        {
            int playerCount = 0;
            foreach (ClientServer client in ClientDictionary.Values)
            {
                if (client.Player != null)
                    playerCount++;
            }
            return playerCount;
        }

        public static void StartServer(string serverName, int maxNumPlayers, string mapName, int portNum)
        {
            try
            {
                ServerName = serverName;
                MapName = mapName;

                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnEndingConsoleApplication);
                (MaxNumPlayers, PortNum) = (maxNumPlayers, portNum);

                Console.WriteLine($"\n\tTrying to start server: {ServerName}, port: {PortNum}...");
                InitServerData();

                TCPListener = new TcpListener(IPAddress.Any, PortNum);
                TCPListener.Start();
                TCPBeginAcceptClient();


                UDPClient = new UdpClient(PortNum);
                UDPBeginReceive();

                Console.WriteLine($"\n\tSuccess Starting Server:" +
                    $"\n\t\tServer: {ServerName}" +
                    $"\n\t\tMap:          {MapName}" +
                    $"\n\t\tMax Players:  {MaxNumPlayers}" +
                    $"\n\t\tPort number:  {PortNum}");
                
                PlayerColor.GetRandomColor();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError in making the Server: {ServerName}...\n{exception}");
                CloseServer();
            }
        }
        public static void CloseServer()
        {
            try
            {
                TCPListener.Stop();
                UDPClient.Close();
                Console.WriteLine($"\tClosed Server: {ServerName}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError closing Server: {ServerName}...\n{exception}");
            }

            if (MainServerComms.EstablishedConnection)
                Environment.Exit(1);
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
                Console.WriteLine($"\tError, sending data to Client from Server: {clientIPEndPoint} via UDP.\nException {exception}");
            }
        }
        
        private static void InitServerData()
        {
            ClientDictionary = new Dictionary<int, ClientServer>();
            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                ClientDictionary.Add(count, new ClientServer(count));
                NotConnectedClients.Add(count, new ClientServer(count));
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
            PacketHandlerDictionary.Add((int)ClientPackets.armPositionRotation, ServerRead.ArmPositionRotation);
            PacketHandlerDictionary.Add((int)ClientPackets.pausedGame, ServerRead.PlayerPausedGame);
            PacketHandlerDictionary.Add((int)ClientPackets.stillConnected, ServerRead.PlayerStillConnected);

            Console.WriteLine("\tInitialised server packets.");
        }

        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPListener.EndAcceptTcpClient(asyncResult);
            TCPBeginAcceptClient();
            Console.WriteLine($"\n\tServer: {ServerName}, user {client.Client.RemoteEndPoint} is trying to connect...");

            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (ClientDictionary[count].tCP.Socket == null)
                {
                    ClientDictionary[count].tCP.Connect(client);
                    Console.WriteLine($"\tServer: {ServerName}, sent welcome packet to: {count}");
                    ServerSend.Welcome(count, $"Welcome to {ServerName} server client: {count}", MapName);
                    return;
                }
            }
            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (NotConnectedClients[count].tCP.Socket == null)
                {
                    NotConnectedClients[count].tCP.Connect(client);
                    ServerSend.ServerIsFullPacket(count);
                    Console.WriteLine($"\n\tThe server is full... {client.Client.RemoteEndPoint} couldn't connect...");
                }
            }
            
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
                Console.WriteLine($"\tError, in UDP data:\n{exception}");
            }
        }
        private static void UDPBeginReceive()
        {
            UDPClient.BeginReceive(UDPConnectAsyncCallback, null);
        }

        private static void OnEndingConsoleApplication(object sender, EventArgs e)
        {
            Console.WriteLine($"\tEnding Server: {ServerName} Console");
            CloseServer();
        }
        
    }
}
