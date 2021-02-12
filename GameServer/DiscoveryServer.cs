using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace GameServer
{
    public static class DiscoveryServer
    {

        private static Socket UDPBroadCastSocket { get; set; }
        private static TcpListener TCPBroadCastTcpListener { get; set; }

        private static EndPoint UDPRemoteEndPoint;
        private static int PortNum { get; set; }

        public delegate void PacketHandler(int client, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary;
        private static Dictionary<int, DiscoveryTCPClientServer> ClientDictionary = new Dictionary<int, DiscoveryTCPClientServer>();

        public static void StartServer(int port)
        {
            PortNum = port;

            Console.WriteLine("\tTrying to start the Discovery Server...");
            if (UDPBroadCastSocket == null)
            {
                try
                {
                    UDPBroadCastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    UDPBroadCastSocket.Bind(new IPEndPoint(IPAddress.Any, PortNum));
                    UDPRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    UDPBeginReceiveDiscoveryCalls();

                    TCPBroadCastTcpListener = new TcpListener(IPAddress.Any, PortNum);
                    TCPBroadCastTcpListener.Start();
                    TCPBeginReceiveDiscoveryClients();

                    InitPacketHandlerDictionary();

                    Console.WriteLine("\tSuccessfully started the Discovery Server.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\tError in StartServer of DiscoveryServer:\n{exception}");
                }
            }
        }
        public static void CloseServer()
        {
            if (UDPBroadCastSocket != null)
            {
                UDPBroadCastSocket.Close();
                UDPBroadCastSocket = null;
            }
            if (TCPBroadCastTcpListener != null)
            {
                TCPBroadCastTcpListener.Stop();
                TCPBroadCastTcpListener = null;
            }
        }

        private static void UDPBeginReceiveDiscoveryCalls()
        {
            UDPBroadCastSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref UDPRemoteEndPoint, new AsyncCallback(UDPAsyncCallbackServer), null);
        }
        private static void UDPAsyncCallbackServer(IAsyncResult result)
        {
            if (UDPBroadCastSocket != null)
            {
                try
                {
                    Console.WriteLine($"\tPlayer has sent out a Discovery call from: {UDPRemoteEndPoint}");
                    int size = UDPBroadCastSocket.EndReceiveFrom(result, ref UDPRemoteEndPoint);
                    byte[] initialBytesToSend = new byte[10];
                    UDPBroadCastSocket.SendTo(initialBytesToSend, UDPRemoteEndPoint);
                    UDPBeginReceiveDiscoveryCalls();                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\tError in AsyncCallback of Discovery Server: \n{exception}");
                }
            }
        }
        

        private static void TCPBeginReceiveDiscoveryClients()
        {
            TCPBroadCastTcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        }
        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPBroadCastTcpListener.EndAcceptTcpClient(asyncResult);
            Console.WriteLine($"\n\tUser {client.Client.RemoteEndPoint} is trying to connect to the discovery server...");
            TCPBeginReceiveDiscoveryClients();

            int discoveryClientCount = -1;
            while (true)
            {
                discoveryClientCount++;

                if (ClientDictionary.ContainsKey(discoveryClientCount)) 
                {
                    if (ClientDictionary[discoveryClientCount].TCPClient != null)
                        continue;                    
                }
                else
                    ClientDictionary.Add(discoveryClientCount, new DiscoveryTCPClientServer(discoveryClientCount));

                break;
            }

            ClientDictionary[discoveryClientCount].Connect(client);

            //TODO: 9001 Make a seperate packet for this, that a server must hear for - like the InternetDiscoveryClientPackets listened for
            string serverName = Server.ServerName;
            int currentPlayerCount = Server.GetCurrentNumPlayers();
            int maxPlayerCount = Server.MaxNumPlayers;
            string mapName = Server.MapName;

            //TODO: Actually get ping value for
            int ping = 10;
            ClientDictionary[discoveryClientCount].SendServerData(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
            Console.WriteLine($"\tSent Server Data packet to: {client.Client.RemoteEndPoint}");
        }

        private static void InitPacketHandlerDictionary()
        {
            PacketHandlerDictionary = new Dictionary<int, PacketHandler>();
            //PacketHandlerDictionary.Add((int)DiscoveryClientPackets.welcomeReceived, hello);
        }

        
    }
}
