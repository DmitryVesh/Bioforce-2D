using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Shared;
using UnityEngine;
using UnityEngine.Output;

namespace GameServer
{
    static class Server
    {
        public static string ServerName { get; private set; }
        public static int MaxNumPlayers { get; private set; }
        public static string MapName { get; private set; }

        public static int PortNum { get; private set; }

        

        public static Dictionary<byte, ClientServer> ClientDictionary = new Dictionary<byte, ClientServer>();

        public static Dictionary<byte, ClientServer> NotConnectedClients = new Dictionary<byte, ClientServer>();

        private static TcpListener TCPListener { get; set; }
        private static UdpClient UDPClient { get; set; }

        public delegate void PacketHandler(byte clientID, Packet packet);
        public static Dictionary<byte, PacketHandler> PacketHandlerDictionary;


        internal static List<int> GetAllPlayerColors()
        {
            List<int> PlayerColors = new List<int>();
            foreach (ClientServer client in ClientDictionary.Values)
            {
                if ((!(client.Player is null)) && client.Player.PlayerColorIndex != -1)
                {
                    PlayerColors.Add(client.Player.PlayerColorIndex);
                }
            }
            return PlayerColors;
        }
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

                Application.quitting += OnApplicationQuiting;
                (MaxNumPlayers, PortNum) = (maxNumPlayers, portNum);

                Output.WriteLine($"\n\tTrying to start server: {ServerName}, port: {PortNum}...");
                InitServerData();

                TCPListener = new TcpListener(IPAddress.Any, PortNum);
                TCPListener.Start();
                TCPBeginAcceptClient();


                UDPClient = new UdpClient(PortNum);
                UDPBeginReceive();

                Output.WriteLine($"\n\tSuccess Starting Server:" +
                    $"\n\t\tServer: {ServerName}" +
                    $"\n\t\tMap:          {MapName}" +
                    $"\n\t\tMax Players:  {MaxNumPlayers}" +
                    $"\n\t\tPort number:  {PortNum}");

                //PlayerColor.GetRandomColor();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError in making the Server: {ServerName}...\n{exception}");
                Application.Quit(1);
                //CloseServer(); TODO: May cause damage commenting it, but I think there is already an OnApplicationQuiting event that calls CloseServer
            }
        }

        private static void OnApplicationQuiting()
        {
            Output.WriteLine($"\tEnding Server: {ServerName} Console");
            CloseServer();
        }

        public static void CloseServer()
        {
            try
            {
                TCPListener.Stop();
                UDPClient.Close();
                Output.WriteLine($"\tClosed Server: {ServerName}");
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError closing Server: {ServerName}...\n{exception}");
            }                
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
                Output.WriteLine($"\tError, sending data to Client from Server: {clientIPEndPoint} via UDP.\nException {exception}");
            }
        }
        
        private static void InitServerData()
        {
            ClientDictionary = new Dictionary<byte, ClientServer>();
            for (byte count = 1; count < MaxNumPlayers + 1; count++)
            {
                ClientDictionary.Add(count, new ClientServer(count));
                NotConnectedClients.Add(count, new ClientServer(count));
            }

            PacketHandlerDictionary = new Dictionary<byte, PacketHandler>();
            PacketHandlerDictionary.Add((byte)ClientPackets.welcomeReceived, ServerRead.WelcomeRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.udpTestReceived, ServerRead.UDPTestRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.playerMovement, ServerRead.PlayerMovementRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.playerMovementStats, ServerRead.PlayerMovementStatsRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.bulletShot, ServerRead.ShotBulletRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.playerDied, ServerRead.PlayerDiedRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.playerRespawned, ServerRead.PlayerRespawnedRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.tookDamage, ServerRead.TookDamageRead);
            PacketHandlerDictionary.Add((byte)ClientPackets.armPositionRotation, ServerRead.ArmPositionRotation);
            PacketHandlerDictionary.Add((byte)ClientPackets.pausedGame, ServerRead.PlayerPausedGame);
            PacketHandlerDictionary.Add((byte)ClientPackets.stillConnectedTCP, ServerRead.PlayerStillConnectedTCP);
            PacketHandlerDictionary.Add((byte)ClientPackets.stillConnectedUDP, ServerRead.PlayerStillConnectedUDP);
            PacketHandlerDictionary.Add((byte)ClientPackets.colorToFreeAndTake, ServerRead.ColorToFreeAndToTake);
            PacketHandlerDictionary.Add((byte)ClientPackets.readyToJoin, ServerRead.ReadyToJoin);
            PacketHandlerDictionary.Add((byte)ClientPackets.chatMessage, ServerRead.ChatMessage);

            Output.WriteLine("\tInitialised server packets.");
        }

        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPListener.EndAcceptTcpClient(asyncResult);
            TCPBeginAcceptClient();
            Output.WriteLine($"\n\tServer: {ServerName}, user {client.Client.RemoteEndPoint} is trying to connect...");

            for (byte count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (ClientDictionary[count].tCP.Socket == null)
                {
                    ClientDictionary[count].tCP.Connect(client);
                    Output.WriteLine($"\tServer: {ServerName}, sent welcome packet to: {count}");
                    ServerSend.Welcome(count, $"Welcome to {ServerName} server client: {count}", MapName);
                    return;
                }
            }
            for (byte count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (NotConnectedClients[count].tCP.Socket == null)
                {
                    NotConnectedClients[count].tCP.Connect(client);
                    ServerSend.ServerIsFullPacket(count);
                    Output.WriteLine($"\n\tThe server is full... {client.Client.RemoteEndPoint} couldn't connect...");
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

                if (data.Length < sizeof(byte)) //Problem occurs, due to clientID now being a byte, not an int, so length is 1
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    byte clientID = packet.ReadByte();

                    if (clientID == 0)
                    {
                        return;
                    }
                    if (ClientDictionary[clientID].uDP.ipEndPoint == null)
                    {
                        Output.WriteLine($"\n\tUser: {clientID} trying to connect via UDP from: {clientIPEndPoint}");
                        ClientDictionary[clientID].uDP.Connect(clientIPEndPoint);
                        return;
                    }

                    if (ClientDictionary[clientID].uDP.ipEndPoint.ToString() == clientIPEndPoint.ToString())
                    {
                        ClientDictionary[clientID].uDP.HandlePacket(packet);
                    }
                }
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, in UDP data:\n{exception}");
            }
        }
        private static void UDPBeginReceive()
        {
            UDPClient.BeginReceive(UDPConnectAsyncCallback, null);
        }
        
    }
}
