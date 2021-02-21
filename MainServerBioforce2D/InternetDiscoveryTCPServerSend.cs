using System;
using System.Collections.Generic;
using System.Text;
using Shared;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServerSend
    {
        public static void SendWelcome(int client)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.welcome);
                packet.Write($"Welcome to the MainServer client: {client}!");

                SendPacket(client, packet);
            }
        }

        public static void SendServerData(int client, Server server)
        {
            if (server == null)
                return;

            try
            {
                using (Packet packet = new Packet())
                {
                    packet.Write((int)InternetDiscoveryServerPackets.serverData);
                    packet.Write(server.ServerName);
                    packet.Write(server.CurrentNumPlayers);
                    packet.Write(server.MaxNumPlayers);
                    packet.Write(server.MapName);
                    packet.Write(server.Ping); //TODO: Actually get ping value for

                    SendPacket(client, packet);
                }
                Console.WriteLine($"Sent ServerData of: {server.ServerName} to client:{client}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error sending server data to client: {client}\n{exception}");
            }
        }

        public static void SendServerDeleted(int client, Server server)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)InternetDiscoveryServerPackets.serverDeleted);
                packet.Write(server.ServerName);
                SendPacket(client, packet);
            }
            Console.WriteLine($"Sent ServerDeleted of: {server.ServerName} to client:{client}");
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

                SendPacket(client, packet);
            }
            Console.WriteLine($"Sent ModifiedServer of: {server.ServerName} to client:{client}");
        }

        internal static void SendCantJoinServerDeleted(int client, string serverName)
        {
            using (Packet packet = new Packet((int)InternetDiscoveryServerPackets.cantJoinServerDeleted))
            {
                packet.Write(serverName);

                SendPacket(client, packet);
            }
            Console.WriteLine($"Sent CantJoinServerDeleted packet server: {serverName} to: {client}");
        }

        internal static void SendNoMoreServersAvailable(int client)
        {
            using (Packet packet = new Packet((int)InternetDiscoveryServerPackets.noMoreServersAvailable))
            {
                SendPacket(client, packet);
            }
            Console.WriteLine($"Sent NoMoreServersAvailable packet to: {client}");
        }

        internal static void SendJoinServer(int client, int serverPort)
        {
            using (Packet packet = new Packet((int)InternetDiscoveryServerPackets.joinServer))
            {
                packet.Write(InternetDiscoveryTCPServer.MyIP);
                packet.Write(serverPort);

                SendPacket(client, packet);
            }
        }

        private static void SendPacket(int client, Packet packet) =>
            InternetDiscoveryTCPServer.ClientDictionary[client].SendPacket(packet);
    }
}
