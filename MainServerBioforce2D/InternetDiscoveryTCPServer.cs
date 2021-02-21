using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MainServer;
using Shared;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServer
    {
        public const string MyIP = "18.130.250.31";
        //public const string MyIP = "127.0.0.1";

        private static TcpListener TCPBroadCastTcpListener { get; set; }

        private static int PortNum { get; set; }

        public delegate void PacketHandler(int client, Packet packet);
        public static Dictionary<int, PacketHandler> PacketHandlerDictionary { get; set; }
        public static Dictionary<int, InternetDiscoveryTCPClientOnServer> ClientDictionary = new Dictionary<int, InternetDiscoveryTCPClientOnServer>();
        public static List<Server> ServersAvailable { get; set; } = new List<Server>();
        public static Dictionary<string, GameServerProcess> GameServerDict { get; set; } = new Dictionary<string, GameServerProcess>();

        private static Queue<int> PortQueue = new Queue<int>(PortsAvailable);
        private static List<int> PortsAvailable
        {
            get
            {
                int minPort = 28030, maxPort = 28129;
                List<int> ports = new List<int>();
                for (int port = minPort; port < maxPort; port++)
                    ports.Add(port);
                return ports;
            }
        }

        public static void StartServer(int port)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CloseServer);
            PortNum = port;

            Console.WriteLine("\nTrying to start the MainServer...");
            if (TCPBroadCastTcpListener == null)
            {
                try
                {
                    TCPBroadCastTcpListener = new TcpListener(IPAddress.Any, PortNum);
                    TCPBroadCastTcpListener.Start();
                    TCPBeginReceiveDiscoveryClients();

                    InitPacketHandlerDictionary();
                    Console.WriteLine("\nSuccessfully started the MainServer.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in StartServer of MainServer:\n{exception}");
                }
            }
        }
        public static void CloseServer(object sender, EventArgs e)
        {
            Console.WriteLine("Closing MainServer...");
            if (TCPBroadCastTcpListener != null)
            {
                TCPBroadCastTcpListener.Stop();
                TCPBroadCastTcpListener = null;
            }

            foreach (GameServerProcess gameServer in GameServerDict.Values)
                gameServer.Kill();
        }
        internal static (int, int) GetAvailablePort()
        {
            if (PortQueue.Count != 0)
            {
                int gamePort = PortQueue.Dequeue();
                return (gamePort, gamePort + 100);
            }
            else
                return (-1, -1);
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

            int discoveryClientCount = SearchForDictSpace(ref ClientDictionary);
            ClientDictionary[discoveryClientCount].Connect(client);
            InternetDiscoveryTCPServerSend.SendWelcome(discoveryClientCount);
            Console.WriteLine($"Connected and sent welcome to new DiscoveryTCPClient: {client.Client.RemoteEndPoint}");
        }

        private static int SearchForDictSpace(ref Dictionary<int, InternetDiscoveryTCPClientOnServer> dict)
        {
            int discoveryClientCount = 0;
            while (true)
            {
                discoveryClientCount++;

                if (dict.ContainsKey(discoveryClientCount))
                {
                    if (dict[discoveryClientCount].TCPClient != null)
                        continue;
                }
                else
                    dict.Add(discoveryClientCount, new InternetDiscoveryTCPClientOnServer(discoveryClientCount));

                break;
            }

            return discoveryClientCount;
        }


        private static void InitPacketHandlerDictionary()
        {
            PacketHandlerDictionary = new Dictionary<int, PacketHandler>();
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.firstAskForServers, InternetDiscoveryTCPServerRead.ReadFirstAskForServers);
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.askForServerChanges, InternetDiscoveryTCPServerRead.ReadAskForServerChanges);
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.addServer, InternetDiscoveryTCPServerRead.ReadAddServer);
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.deletedServer, InternetDiscoveryTCPServerRead.ReadDeleteServer);
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.modifiedServer, InternetDiscoveryTCPServerRead.ReadModifyServer);
            PacketHandlerDictionary.Add((int)InternetDiscoveryClientPackets.joinServerNamed, InternetDiscoveryTCPServerRead.ReadJoinServerNamed);
        }

        internal static void OnGameServerExited(object sender, GameServerArgs args)
        {
            string serverName = args.ServerName;
            int port = args.ServerPort;
            PortQueue.Enqueue(port);

            Console.WriteLine("" +
                $"\n-------------------------------------------" +
                $"\nGameServer: {serverName} exited..." +
                $"\n-------------------------------------------");

            lock (ServersAvailable)
            {
                int serverIndexToDelete = -1;
                for (int serverCount = 0; serverCount < ServersAvailable.Count; serverCount++)
                {
                    if (ServersAvailable[serverCount].ServerName == serverName)
                        serverIndexToDelete = serverCount;
                }
                if (serverIndexToDelete != -1)
                    ServersAvailable.RemoveAt(serverIndexToDelete);

                GameServerDict.Remove(serverName);
                GameServerComms.GameServerConnection.Remove(serverName);
            }
        }
    }
}
