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

        private static List<DiscoveryTCPClient> DiscoveryClients { get; set; }

        public delegate void PacketHandler(int client, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary;
        private static Dictionary<int, DiscoveryTCPClient> ClientDictionary = new Dictionary<int, DiscoveryTCPClient>();

        public static void StartServer(int port)
        {
            PortNum = port;
            DiscoveryClients = new List<DiscoveryTCPClient>();

            Console.WriteLine("\nTrying to start the Discovery Server...");
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

                    Console.WriteLine("\nSuccessfully started the Discovery Server.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in StartServer of DiscoveryServer:\n{exception}");
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
                    Console.WriteLine($"Player has sent out a Discovery call from: {UDPRemoteEndPoint.ToString()}");
                    int size = UDPBroadCastSocket.EndReceiveFrom(result, ref UDPRemoteEndPoint);
                    
                    //string address = RemoteEndPoint.ToString().Split(':')[0];
                    //AddNewDiscoveryClient(address, PortNum);

                    //TODO: Send serverName, playerCount, mapName, ping
                    //string serverName = "Random Please Work";
                    //int playerCount = 21;
                    //string mapName = "Please";
                    //int ping = 10;
                    //byte[] serverDiscover = Encoding.ASCII.GetBytes(serverName).Concat(BitConverter.GetBytes(playerCount)).Concat(Encoding.ASCII.GetBytes(mapName)).Concat(BitConverter.GetBytes(ping)).ToArray();
                    //
                    //client.SendTo(serverDiscover, RemoteEndPoint);

                    UDPBeginReceiveDiscoveryCalls();
                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in AsyncCallback of Discovery Server: \n{exception}");
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
            Console.WriteLine($"\nUser {client.Client.RemoteEndPoint} is trying to connect to the discovery server...");
            TCPBeginReceiveDiscoveryClients();

            //TODO: Actually have values for these
            string serverName = "Random Please Work";
            int playerCount = 21;
            string mapName = "Please";
            int ping = 10;

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
                    ClientDictionary.Add(discoveryClientCount, new DiscoveryTCPClient(discoveryClientCount));

                break;
            }

            ClientDictionary[discoveryClientCount].Connect(client);
            ClientDictionary[discoveryClientCount].SendServerData(serverName, playerCount, mapName, ping);
            Console.WriteLine($"Sent Server Data packet to: {client.Client.RemoteEndPoint}");
        }

        private static void InitPacketHandlerDictionary()
        {
            PacketHandlerDictionary = new Dictionary<int, PacketHandler>();
            //PacketHandlerDictionary.Add((int)DiscoveryClientPackets.welcomeReceived, hello);
        }

        
    }
}
