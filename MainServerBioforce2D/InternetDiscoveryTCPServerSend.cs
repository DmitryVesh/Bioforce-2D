using System;
using System.Collections.Generic;
using System.Text;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServerSend
    {
        public static void SendWelcome(int discoveryClientCount)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.welcome);
                packet.Write($"Welcome to the MainServer client: {discoveryClientCount}!");
                InternetDiscoveryTCPServer.ClientDictionary[discoveryClientCount].SendPacket(packet);
            }
        }

        public static void SendServerData(int client, Server server)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.serverData);
                packet.Write(server.ServerName);
                packet.Write(server.CurrentNumPlayers);
                packet.Write(server.MaxNumPlayers);
                packet.Write(server.MapName);
                packet.Write(server.Ping); //TODO: Actually get ping value for
                packet.Write(server.IP);
                InternetDiscoveryTCPServer.ClientDictionary[client].SendPacket(packet);
            }
        }

        public static void SendServerDeleted(int client, Server server)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.serverDeleted);
                packet.Write(server.ServerName);
                packet.Write(server.IP);
                InternetDiscoveryTCPServer.ClientDictionary[client].SendPacket(packet);
            }
        }

        public static void SendModifedServer(int client, Server server)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.serverModified);
                packet.Write(server.ServerName);
                packet.Write(server.CurrentNumPlayers);
                packet.Write(server.MaxNumPlayers);
                packet.Write(server.MapName);
                packet.Write(server.Ping); //TODO: Actually get ping value for
                packet.Write(server.IP);
                InternetDiscoveryTCPServer.ClientDictionary[client].SendPacket(packet);
            }
        }

        
    }
}
