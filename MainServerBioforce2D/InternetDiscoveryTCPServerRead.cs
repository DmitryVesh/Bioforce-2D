using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
        }

        internal static void ReadAddServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();            
            int maxNumPlayers = packet.ReadInt();
            string mapName = packet.ReadString();
            int currentNumPlayers = packet.ReadInt();
            int ping = packet.ReadInt();
            string ip = packet.ReadString();

            Server server = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping, ip);
            InternetDiscoveryTCPServer.ServersAvailable.Add(server);
        }

        internal static void ReadDeleteServer(int client, Packet packet)
        {
            string serverName = packet.ReadString();
            string ip = packet.ReadString();

            Server[] servers = new Server[InternetDiscoveryTCPServer.ServersAvailable.Count - 1];
            int takeAwayOne = 0;
            for (int serverCount = 0; serverCount < InternetDiscoveryTCPServer.ServersAvailable.Count; serverCount++)
            {
                Server serverAvailable = InternetDiscoveryTCPServer.ServersAvailable[serverCount];
                if (serverAvailable.ServerName == serverName && serverAvailable.IP == ip) 
                {
                    takeAwayOne = 1;
                    continue;
                }
                servers[serverCount - takeAwayOne] = serverAvailable;
            }
            InternetDiscoveryTCPServer.ServersAvailable = ImmutableList.Create(servers.ToArray());
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
                if (ServerMatch(serverAvailable, serverName, ip))
                    serverAvailable = new Server(serverName, maxNumPlayers, mapName, currentNumPlayers, ping, ip);
                servers[serverCount] = serverAvailable;
            }
            InternetDiscoveryTCPServer.ServersAvailable = ImmutableList.Create(servers.ToArray());
        }

        private static bool ServerMatch(Server server, string serverName, string ip) =>
            server.ServerName == serverName && server.IP == ip;
    }
}
