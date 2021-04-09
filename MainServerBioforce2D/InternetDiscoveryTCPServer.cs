using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MainServer;
using Shared;
using DmitryNamespace;
using System.IO;
using System.Diagnostics;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServer
    {
        public const string MainServerIP = "18.134.197.3";
        //public const string MainServerIP = "127.0.0.1";

        public const string GameVersionLatest = "1.0.2";

        private static TcpListener TCPBroadCastTcpListener { get; set; }

        private static int PortNum { get; set; }

        public delegate void PacketHandler(byte client, Packet packet);
        public static Dictionary<byte, PacketHandler> PacketHandlerDictionary { get; set; }
        public static Dictionary<byte, InternetDiscoveryTCPClientOnServer> ClientDictionary = new Dictionary<byte, InternetDiscoveryTCPClientOnServer>();
        public static List<Server> ServersAvailable { get; set; } = new List<Server>();
        public static Dictionary<string, GameServerProcess> GameServerDict { get; set; } = new Dictionary<string, GameServerProcess>();

        private static Queue<int> PortQueue { get; set; }
        private static int NumServers { get; set; } = 100;
        
        private static BinaryTree<string> IPsConnected = new BinaryTree<string>(true);

        private static List<int> PortsAvailable()
        {
            int minPort = PortNum + 10, maxPort = minPort + NumServers - 1;
            List<int> ports = new List<int>();
            for (int port = minPort; port < maxPort; port++)
                ports.Add(port);
            return ports;
        }

        public static int MakeGameServer(string serverName, int maxNumPlayers, string mapName, int currentNumPlayers, int ping, int timeOut)
        {
            (int gameServerPort, int gameMainPort) = GetAvailablePort();
            if (gameServerPort == -1) //No more servers available
            {
                return gameServerPort;
            }

            Server server = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping);
            ServersAvailable.Add(server);

            GameServerProcess gameServerProcess = new GameServerProcess(serverName, gameServerPort);
            gameServerProcess.StartInfo = new ProcessStartInfo
            {
                FileName = Program.GameServerFileName,
                ArgumentList = { "GameServer", serverName, maxNumPlayers.ToString(), mapName, gameServerPort.ToString(), gameMainPort.ToString(), timeOut.ToString() }
            };
            gameServerProcess.Start();
            gameServerProcess.EnableRaisingEvents = true;
            gameServerProcess.OnGameServerExited += OnGameServerExited;

            GameServerDict.Add(serverName, gameServerProcess);

            GameServerComms gameServerConnection = new GameServerComms(serverName, gameMainPort);
            GameServerComms.GameServerConnection.Add(serverName, gameServerConnection);
            
            return gameServerPort;
        }
        private static void MakePersistentServer() =>
            MakeGameServer(serverName: "Welcome", maxNumPlayers: 16, mapName: "Level 1", currentNumPlayers: 0, ping: 0, timeOut: -1);

        public static void StartServer(int port)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CloseServer);
            PortNum = port;

            PortQueue = new Queue<int>(PortsAvailable());

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

            MakePersistentServer();
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
        private static (int, int) GetAvailablePort()
        {
            if (PortQueue.Count != 0)
            {
                int gamePort = PortQueue.Dequeue();
                return (gamePort, gamePort + NumServers);
            }
            else
                return (-1, -1);
        }

        private static void TCPBeginReceiveDiscoveryClients() =>
            TCPBroadCastTcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectAsyncCallback), null);
        private static void TCPConnectAsyncCallback(IAsyncResult asyncResult)
        {
            TcpClient client = TCPBroadCastTcpListener.EndAcceptTcpClient(asyncResult);
            Console.WriteLine($"\nUser {client.Client.RemoteEndPoint} is trying to connect to the discovery server...");
            TCPBeginReceiveDiscoveryClients();

            byte discoveryClientCount = SearchForDictSpace(ref ClientDictionary);
            ClientDictionary[discoveryClientCount].Connect(client);
            InternetDiscoveryTCPServerSend.SendWelcome(discoveryClientCount);
            string ip = client.Client.RemoteEndPoint.ToString().Split(':')[0].ToString();

            
            Console.WriteLine($"Connected and sent welcome to new DiscoveryTCPClient: {client.Client.RemoteEndPoint}");

            try
            {
                if (IPsConnected.NumNodes == 0)
                    IPsConnected.Add(ip);

                if (!IPsConnected.Contains(ip))
                    IPsConnected.Add(ip);

                using (StreamWriter sw = new StreamWriter("IPs.txt", false))
                {
                    sw.WriteLine(IPsConnected.NumNodes);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static byte SearchForDictSpace(ref Dictionary<byte, InternetDiscoveryTCPClientOnServer> dict)
        {
            byte discoveryClientCount = 0;
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
            PacketHandlerDictionary = new Dictionary<byte, PacketHandler>();
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.firstAskForServers, InternetDiscoveryTCPServerRead.ReadFirstAskForServers);
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.askForServerChanges, InternetDiscoveryTCPServerRead.ReadAskForServerChanges);
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.addServer, InternetDiscoveryTCPServerRead.ReadAddServer);
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.deletedServer, InternetDiscoveryTCPServerRead.ReadDeleteServer);
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.modifiedServer, InternetDiscoveryTCPServerRead.ReadModifyServer);
            PacketHandlerDictionary.Add((byte)InternetDiscoveryClientPackets.joinServerNamed, InternetDiscoveryTCPServerRead.ReadJoinServerNamed);
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

            if (serverName == "Welcome")
                MakePersistentServer();
        }
    }
}
