using System;
using System.Collections.Generic;
using System.Diagnostics;
using MainServer;
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
            InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven = new List<Server>(InternetDiscoveryTCPServer.ServersAvailable);
            Console.WriteLine($"Read FirstAskForServers from client:{client}");
        }
        internal static void ReadAskForServerChanges(int client, Packet packet)
        {
            try
            {
                List<Server> serversGiven = InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven;
                List<Server> serversAvailable = InternetDiscoveryTCPServer.ServersAvailable;

                if (serversGiven.Count > serversAvailable.Count) //Must mean 1 or more servers deleted
                {
                    foreach (Server serverGiven in serversGiven)
                    {
                        bool givenServerFound = false;
                        foreach (Server serverAvailable in serversAvailable)
                        {
                            if (serverGiven.ServerName == serverAvailable.ServerName)
                            {
                                givenServerFound = true;
                                break;
                            }
                        }
                        if (!givenServerFound)
                            InternetDiscoveryTCPServerSend.SendServerDeleted(client, serverGiven);
                    }
                }

                foreach (Server serverAvailable in serversAvailable)
                {
                    bool foundServerMatch = false;
                    bool timeStampsMatch = false;

                    foreach (Server serverGiven in serversGiven)
                    {
                        if (serverAvailable.ServerName == serverGiven.ServerName)
                        {
                            foundServerMatch = true;
                            if (serverAvailable.TimeStamp == serverGiven.TimeStamp)
                                timeStampsMatch = true;
                            break;
                        }
                    }

                    if (foundServerMatch && !timeStampsMatch)
                        InternetDiscoveryTCPServerSend.SendModifedServer(client, serverAvailable);
                    else if (!foundServerMatch) 
                        InternetDiscoveryTCPServerSend.SendServerData(client, serverAvailable);
                }

                InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven = new List<Server>(serversAvailable);
                Console.WriteLine($"Read AskForServerChanges from client:{client}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error ReadAskForServerChanges from client: {client}...\n{exception}");
            }

        }
        internal static void ReadAddServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();            
            int maxNumPlayers = packet.ReadInt();
            string mapName = packet.ReadString();
            int currentNumPlayers = packet.ReadInt();
            int ping = packet.ReadInt();

            if (InternetDiscoveryTCPServer.GameServerDict.ContainsKey(serverName))
            {
                int port = int.Parse(InternetDiscoveryTCPServer.GameServerDict[serverName].StartInfo.ArgumentList[4]);
                InternetDiscoveryTCPServerSend.SendJoinServer(client, port);
                return;
            }
            
            Console.WriteLine($"Read AddServer from client:{client} at ip {InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.RemoteEndPoint}");

            (int gameServerPort, int gameMainPort) = InternetDiscoveryTCPServer.GetAvailablePort();
            if (gameServerPort == -1) //No more servers available
            {
                Console.WriteLine(
                    $"\n==========================" +
                    $"\nNo more servers available!" +
                    $"\n==========================\n");
                InternetDiscoveryTCPServerSend.SendNoMoreServersAvailable(client);
                return;
            }

            Server server = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping);
            InternetDiscoveryTCPServer.ServersAvailable.Add(server);

            GameServerProcess gameServerProcess = new GameServerProcess(serverName, gameServerPort);
            gameServerProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "MainServer",
                ArgumentList = { "GameServer", serverName, maxNumPlayers.ToString(), mapName, gameServerPort.ToString(), gameMainPort.ToString() }
            };
            gameServerProcess.Start();
            gameServerProcess.EnableRaisingEvents = true;
            gameServerProcess.OnGameServerExited += InternetDiscoveryTCPServer.OnGameServerExited;

            InternetDiscoveryTCPServer.GameServerDict.Add(serverName, gameServerProcess);

            GameServerComms gameServerConnection = new GameServerComms(serverName, gameMainPort);
            GameServerComms.GameServerConnection.Add(serverName, gameServerConnection);

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

            Server[] servers = new Server[InternetDiscoveryTCPServer.ServersAvailable.Count];
            for (int serverCount = 0; serverCount < InternetDiscoveryTCPServer.ServersAvailable.Count; serverCount++)
            {
                Server serverAvailable = InternetDiscoveryTCPServer.ServersAvailable[serverCount];
                if (ServerMatch(serverAvailable, serverName))
                    serverAvailable = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping);
                servers[serverCount] = serverAvailable;
            }
            InternetDiscoveryTCPServer.ServersAvailable = new List<Server>(servers);
            Console.WriteLine($"Read ModifyServer from client:{client}");
        }

        private static bool ServerMatch(Server server, string serverName) =>
            server.ServerName == serverName;
    }
}
