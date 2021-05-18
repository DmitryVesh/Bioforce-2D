using System;
using System.Collections.Generic;
using System.Diagnostics;
using MainServer;
using Shared;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServerRead
    {
        internal static void ReadFirstAskForServers(byte client, Packet packet)
        {
            //Send existing servers saved on server to client
            foreach (Server server in InternetDiscoveryTCPServer.ServersAvailable)
                InternetDiscoveryTCPServerSend.SendServerData(client, server);
            InternetDiscoveryTCPServer.ClientDictionary[client].ServersAlreadyGiven = new List<Server>(InternetDiscoveryTCPServer.ServersAvailable);
            Output.WriteLine($"Read FirstAskForServers from client:{client}");
        }
        internal static void ReadAskForServerChanges(byte client, Packet packet)
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
                Output.WriteLine($"Read AskForServerChanges from client:{client}");
            }
            catch (Exception exception)
            {
                Output.WriteLine($"Error ReadAskForServerChanges from client: {client}...\n{exception}");
            }

        }
        internal static void ReadAddServer(byte client, Packet packet)
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
            
            Output.WriteLine($"Read AddServer from client:{client} at ip {InternetDiscoveryTCPServer.ClientDictionary[client].TCPClient.Client.RemoteEndPoint}");

            int gameServerPort = 
                InternetDiscoveryTCPServer.MakeGameServer(serverName, maxNumPlayers, mapName, currentNumPlayers, ping, timeOutSeconds: 30, isServerPermanent: false);
            if (gameServerPort == -1)
            {
                Output.WriteLine(
                    $"\n==========================" +
                    $"\nNo more servers available!" +
                    $"\n==========================\n");
                InternetDiscoveryTCPServerSend.SendNoMoreServersAvailable(client);
                return;
            }
            InternetDiscoveryTCPServerSend.SendJoinServer(client, gameServerPort);
        }
        internal static void ReadDeleteServer(byte client, Packet packet)
        {
            string serverName = packet.ReadString();            

            InternetDiscoveryTCPServer.GameServerDict[serverName].Kill();
            Output.WriteLine($"Read DeleteServer from client:{client}");
        }
        internal static void ReadJoinServerNamed(byte client, Packet packet)
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

        internal static void ReadModifyServer(byte client, Packet packet)
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
            Output.WriteLine($"Read ModifyServer from client:{client}");
        }

        private static bool ServerMatch(Server server, string serverName) =>
            server.ServerName == serverName;
    }
}
