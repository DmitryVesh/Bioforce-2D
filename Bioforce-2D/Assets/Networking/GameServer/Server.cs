using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace GameServer
{
    static class Server
    {
        public static string ServerName { get; private set; }
        public static int MaxNumPlayers { get; private set; }
        public static string MapName { get; private set; }
        public static int PortNum { get; private set; }        

        public static Dictionary<int, ClientServer> ClientDictionary = new Dictionary<int, ClientServer>();
        private static TcpListener TCPListener { get; set; }
        private static UdpClient UDPClient { get; set; }

        public delegate void PacketHandler(int clientID, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary;

        public static int GetCurrentNumPlayers()
        {
            int playerCount = 0;
            foreach (ClientServer client in ClientDictionary.Values)
            {
                if (client.player != null)
                    playerCount++;
            }
            return playerCount;
        }

        public static void StartServer(string serverName, int maxNumPlayers, string mapName, int portNum)
        {
            ServerName = serverName;
            MapName = mapName;

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnEndingConsoleApplication);
            (MaxNumPlayers, PortNum) = (maxNumPlayers, portNum);

            Debug.Log("\nTrying to start the server...");
            InitServerData();

            TCPListener = new TcpListener(IPAddress.Any, portNum);
            TCPListener.Start();
            TCPBeginAcceptClient();

            
            UDPClient = new UdpClient(PortNum);
            UDPBeginReceive();

            Debug.Log($"" +
                $"\nServer: {ServerName}" +
                $"\n\tMap:          {MapName}" +
                $"\n\tMax Players:  {MaxNumPlayers}"+
                $"\n\tPort number:  {PortNum}");

            InternetServerScanner.ContactMainServerToAddOwnServer(Client.PortNumInternetDiscover);
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
                Debug.Log($"Error, sending data to Client from Server: {clientIPEndPoint} via UDP.\nException {exception}");
            }
        }
        
        private static void InitServerData()
        {
            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                ClientDictionary.Add(count, new ClientServer(count));
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

            Debug.Log("Initialised server packets.");
        }

        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPListener.EndAcceptTcpClient(asyncResult);
            TCPBeginAcceptClient();
            Debug.Log($"\nUser {client.Client.RemoteEndPoint} is trying to connect...");

            for (int count = 1; count < MaxNumPlayers + 1; count++)
            {
                if (ClientDictionary[count].tCP.Socket == null)
                {
                    ClientDictionary[count].tCP.Connect(client);
                    Debug.Log($"Sent welcome packet to: {count}");
                    ServerSend.Welcome(count, $"Welcome to {ServerName} server client: {count}", MapName);
                    return;
                }
            }
            Debug.Log($"\nThe server is full... {client.Client.RemoteEndPoint} couldn't connect...");
            SendServerIsFullPacket(client);
        }
        private static void TCPBeginAcceptClient()
        {
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        }
        private static void SendServerIsFullPacket(TcpClient client)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)ServerPackets.serverIsFull);
                packet.WriteLength();
                SendUDPPacket((IPEndPoint)client.Client.RemoteEndPoint, packet); //TODO: The (IPEndPoint) might cause issues
            }
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
                Debug.Log($"Error, in UDP data:\n{exception}");
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
