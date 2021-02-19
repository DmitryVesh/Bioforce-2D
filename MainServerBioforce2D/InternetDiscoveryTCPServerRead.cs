using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Shared;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServerRead
    {
        internal static void ReadFirstAskForServers(int client, Packet packet)
        {
            //Send existing servers saved on server to client
            foreach (Server server in InternetDiscoveryTCPServer.ServersAvailable)
                InternetDiscoveryTCPServerSend.SendServerData(client, server);
            InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven = InternetDiscoveryTCPServer.ServersAvailable;
            Console.WriteLine($"Read FirstAskForServers from client:{client}");
        }
        internal static void ReadAskForServerChanges(int client, Packet packet)
        {
            ImmutableList<Server> serversGiven = InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven;
            foreach (Server serverGiven in serversGiven)
            {
                if (!InternetDiscoveryTCPServer.ServersAvailable.Contains(serverGiven)) //Must mean server was deleted
                    InternetDiscoveryTCPServerSend.SendServerDeleted(client, serverGiven);
            }

            foreach (Server serverAvailable in InternetDiscoveryTCPServer.ServersAvailable)
            {
                bool serversGivenHasServer = serversGiven.Contains(serverAvailable);
                if (!serversGivenHasServer)
                    InternetDiscoveryTCPServerSend.SendServerData(client, serverAvailable);
                else
                {
                    bool timeStampsMatch = serversGiven[serversGiven.IndexOf(serverAvailable)].TimeStamp == serverAvailable.TimeStamp;
                    if (!timeStampsMatch)
                        InternetDiscoveryTCPServerSend.SendModifedServer(client, serverAvailable);
                }
            }

            InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven = InternetDiscoveryTCPServer.ServersAvailable;
            Console.WriteLine($"Read AskForServerChanges from client:{client}");


        }
        internal static void ReadAddServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();            
            int maxNumPlayers = packet.ReadInt();
            string mapName = packet.ReadString();
            int currentNumPlayers = packet.ReadInt();
            int ping = packet.ReadInt();
            //string ip = InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.RemoteEndPoint.ToString().Split(':')[0];

            Server server = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping);
            InternetDiscoveryTCPServer.ServersAvailable = InternetDiscoveryTCPServer.ServersAvailable.Add(server);
            Console.WriteLine($"Read AddServer from client:{client} at ip {InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.RemoteEndPoint}");
            Console.WriteLine($"Client's RemoteEndPoint: {InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.RemoteEndPoint}");
            Console.WriteLine($"Client's LocalEndPoint: {InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.LocalEndPoint}");

            int gameServerPort = InternetDiscoveryTCPServer.GetAvailablePort();
            if (gameServerPort == -1) //No more servers available
            {
                InternetDiscoveryTCPServerSend.SendNoMoreServersAvailable(client);
                return;
            }

            GameServerProcess gameServerProcess = new GameServerProcess(serverName, gameServerPort);
            gameServerProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "MainServer",
                ArgumentList = { "GameServer", serverName, maxNumPlayers.ToString(), mapName, gameServerPort.ToString() }
            };
            gameServerProcess.Start();
            gameServerProcess.EnableRaisingEvents = true;
            gameServerProcess.OnGameServerExited += InternetDiscoveryTCPServer.OnGameServerExited;

            InternetDiscoveryTCPServer.GameServerDict.Add(serverName, gameServerProcess);
            //TODO: send TCP packet to GameServer
            InternetDiscoveryTCPServerSend.SendJoinServer(client, gameServerPort);
        }
        internal static void ReadDeleteServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();            

            InternetDiscoveryTCPServer.GameServerDict[serverName].Kill();
            Console.WriteLine($"Read DeleteServer from client:{client}");
        }
        internal static void ReadJoinServerNamed(int client, Packet packet)
        {
            string serverName = packet.ReadString();
            GameServerProcess gameServer;
            if (!InternetDiscoveryTCPServer.GameServerDict.TryGetValue(serverName, out gameServer)) //Check if server is still running
            {
                InternetDiscoveryTCPServerSend.SendCantJoinServerDeleted(client, serverName);
                return;
            }
            
            //Give the port and ip to the client of server to join
            int serverPort = gameServer.ServerPort;
            InternetDiscoveryTCPServerSend.SendJoinServer(client, serverPort);
        }

        internal static void ReadModifyServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();
            int maxNumPlayers = packet.ReadInt();
            string mapName = packet.ReadString();
            int currentNumPlayers = packet.ReadInt();
            int ping = packet.ReadInt();
            string ip = packet.ReadString();

            Server[] servers = new Server[InternetDiscoveryTCPServer.ServersAvailable.Count - 1];
            for (int serverCount = 0; serverCount < InternetDiscoveryTCPServer.ServersAvailable.Count; serverCount++)
            {
                Server serverAvailable = InternetDiscoveryTCPServer.ServersAvailable[serverCount];
                if (ServerMatch(serverAvailable, serverName))
                    serverAvailable = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping);
                servers[serverCount] = serverAvailable;
            }
            InternetDiscoveryTCPServer.ServersAvailable = ImmutableList.Create(servers.ToArray());
            Console.WriteLine($"Read ModifyServer from client:{client}");
        }

        private static bool ServerMatch(Server server, string serverName) =>
            server.ServerName == serverName;
    }
}
