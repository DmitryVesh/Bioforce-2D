using MainServerBioforce2D;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MainServer
{
    class GameServerComms
    {
        public delegate void PacketHandler(string serverName, Packet packet);
        public static Dictionary<byte, PacketHandler> PacketHandlerDictionary { get; set; }
        public static Dictionary<string, GameServerComms> GameServerConnection = new Dictionary<string, GameServerComms>();
        private TcpListener InitialSocketListener { get; set; }

        public string ServerName { get; private set; }
        public int Port { get; private set; }
        public GameServerComms(string serverName, int port)
        {
            ServerName = serverName;
            Port = port;

            //Listen to traffic comming from 127.0.0.1 on Port
            InitialSocketListener = new TcpListener(IPAddress.Loopback, port);
            InitialSocketListener.Start();
            TCPBeginReceiveGameServer();

            InitPacketHandlerDictionary();
        }

        private void InitPacketHandlerDictionary()
        {
            PacketHandlerDictionary = new Dictionary<byte, PacketHandler>();
            PacketHandlerDictionary.Add((byte)ServerToMainServer.welcomeReceived, ReadWelcomeReceived);
            PacketHandlerDictionary.Add((byte)ServerToMainServer.serverData, ReadServerData);
            PacketHandlerDictionary.Add((byte)ServerToMainServer.shuttingDown, ReadShuttingDown);
        }

        private void TCPBeginReceiveGameServer() =>
            InitialSocketListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        private void TCPConnectAsyncCallback(IAsyncResult ar)
        {
            TcpClient client = InitialSocketListener.EndAcceptTcpClient(ar);
            Output.WriteLine($"\nGameServer: {client.Client.RemoteEndPoint} is trying to connect to GameServerComms...");
            Connect(client);
            SendWelcome();
        }

        #region GameServerCommsRead

        private void ReadWelcomeReceived(string serverName, Packet packet) 
        {
            Output.WriteLine($"\nMainServer has Read WelcomeReceived packet from GameServer: {serverName}");
        }
        private void ReadServerData(string serverName, Packet packet)
        {
            byte serverState = packet.ReadByte();
            int currentNumPlayers = packet.ReadInt();
            int maxNumPlayers = packet.ReadInt();
            string mapName = packet.ReadString();

            try
            {
                List<Server> servers = InternetDiscoveryTCPServer.ServersAvailable;
                int serversNum = servers.Count;
                for (int serverCount = 0; serverCount < serversNum; serverCount++)
                {
                    if (servers[serverCount].MatchesName(serverName))
                        servers[serverCount] = new Server(serverName, serverState, maxNumPlayers, mapName, currentNumPlayers, 21);
                }
                InternetDiscoveryTCPServer.ServersAvailable = new List<Server>(servers.ToList());
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\nError ReadServerData of GameServer: {serverName}\n{exception}");
            }
            //Output.WriteLine($"\nRead ServerData from GameServer: {serverName}");
        }
        private void ReadShuttingDown(string serverName, Packet packet)
        {
            if (GameServerConnection.ContainsKey(serverName))
            {
                InternetDiscoveryTCPServer.GameServerDict[serverName].Kill();
                InternetDiscoveryTCPServer.GameServerDict.Remove(serverName);
                GameServerConnection.Remove(serverName);
                Output.WriteLine($"\nMainServer Killed GameServer: {serverName}");
            }
        }


        #endregion

        #region GameServerCommsSend

        private void SendWelcome()
        {
            using (Packet packet = new Packet((byte)MainServerToServer.welcome)) 
            {
                SendPacket(packet);
            }
            Output.WriteLine($"\nMainServer sent Welcome packet to GameServer: {ServerName}");
        }

        #endregion

        #region TcpClientHandling

        public TcpClient TCPClient { get; private set; }

        private static int DataBufferSize { get; set; } = 4096;
        private byte[] ReceiveBuffer;
        private NetworkStream Stream;
        private Packet ReceivePacket;

        public void Connect(TcpClient client)
        {
            TCPClient = client;

            TCPClient.ReceiveBufferSize = DataBufferSize;
            TCPClient.SendBufferSize = DataBufferSize;

            Stream = TCPClient.GetStream();
            ReceiveBuffer = new byte[DataBufferSize];
            ReceivePacket = new Packet();

            StreamBeginRead();
        }
        public void Disconnect()
        {
            TCPClient.Close();
            TCPClient = null;
            Stream = null;
            ReceiveBuffer = null;
            ReceivePacket = null;
        }

        private void SendPacket(Packet packet)
        {
            packet.WriteLength();

            try
            {
                if (TCPClient != null)
                    Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception exception)
            {
                //Disconnect();
                Output.WriteLine($"\n\t\tError, occured when sending TCP data from MainServer to GameServer: {ServerName}\n{exception}");
            }
        }

        private void StreamBeginRead()
        {
            Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
        }
        private void BeginReadReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                int byteLen = Stream.EndRead(asyncResult);
                if (byteLen <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLen];
                Array.Copy(ReceiveBuffer, data, byteLen);

                bool resetData = HandleData(data);
                ReceivePacket.Reset(resetData);
                StreamBeginRead();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\n\t\tError in BeginReadReceiveCallback of GameServerComms {ServerName}...\nError: {exception}");
                Disconnect();
            }
        }
        private bool HandleData(byte[] data)
        {
            byte packetLen = 0;
            ReceivePacket.SetBytes(data);

            if (ExitHandleData(ref packetLen))
                return true;

            while (packetLen > 0 && packetLen <= ReceivePacket.UnreadLength())
            {
                byte[] bytes = ReceivePacket.ReadBytes(packetLen);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(bytes))
                    {
                        byte packetId = packet.ReadByte();
                        string serverName = packet.ReadString();
                        PacketHandlerDictionary[packetId](serverName, packet);
                    }
                });
                packetLen = 0;

                if (ExitHandleData(ref packetLen))
                    return true;
            }
            if (packetLen < 2)
            {
                return true;
            }

            return false;
        }
        private bool ExitHandleData(ref byte packetLen)
        {
            if (ReceivePacket.UnreadLength() >= sizeof(byte))
            {
                packetLen = ReceivePacket.ReadPacketLen();
                if (packetLen < 1)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
