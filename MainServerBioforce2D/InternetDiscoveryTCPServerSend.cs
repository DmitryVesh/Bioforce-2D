using System;
using Shared;

namespace MainServerBioforce2D
{
    class InternetDiscoveryTCPServerSend
    {
        public static void SendWelcome(byte client)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.welcome))
            {
                packet.Write($"Welcome to the MainServer client: {client}!");
                packet.Write(InternetDiscoveryTCPServer.GameVersionLatest);

                SendPacket(client, packet);
            }
        }

        public static void SendServerData(byte client, Server server)
        {
            if (server == null)
                return;

            try
            {
                using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.serverData))
                {
                    packet.Write(server.ServerName);
                    packet.Write(server.CurrentNumPlayers);
                    packet.Write(server.MaxNumPlayers);
                    packet.Write(server.MapName);
                    packet.Write(server.Ping); //TODO: Actually get ping value for

                    SendPacket(client, packet);
                }
                Output.WriteLine($"Sent ServerData of: {server.ServerName} to client:{client}");
            }
            catch (Exception exception)
            {
                Output.WriteLine($"Error sending server data to client: {client}\n{exception}");
            }
        }

        public static void SendServerDeleted(byte client, Server server)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.serverDeleted))
            {
                packet.Write(server.ServerName);
                SendPacket(client, packet);
            }
            Output.WriteLine($"Sent ServerDeleted of: {server.ServerName} to client:{client}");
        }

        public static void SendModifedServer(byte client, Server server)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.serverModified))
            {
                packet.Write(server.ServerName);
                packet.Write(server.CurrentNumPlayers);
                packet.Write(server.MaxNumPlayers);
                packet.Write(server.MapName);
                packet.Write(server.Ping); //TODO: Actually get ping value for

                SendPacket(client, packet);
            }
            Output.WriteLine($"Sent ModifiedServer of: {server.ServerName} to client:{client}");
        }

        internal static void SendCantJoinServerDeleted(byte client, string serverName)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.cantJoinServerDeleted))
            {
                packet.Write(serverName);

                SendPacket(client, packet);
            }
            Output.WriteLine($"Sent CantJoinServerDeleted packet server: {serverName} to: {client}");
        }

        internal static void SendNoMoreServersAvailable(byte client)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.noMoreServersAvailable))
            {
                SendPacket(client, packet);
            }
            Output.WriteLine($"Sent NoMoreServersAvailable packet to: {client}");
        }

        internal static void SendJoinServer(byte client, int serverPort)
        {
            using (Packet packet = new Packet((byte)InternetDiscoveryServerPackets.joinServer))
            {
                packet.Write(InternetDiscoveryTCPServer.MainServerIP);
                packet.Write(serverPort);

                SendPacket(client, packet);
            }
        }

        private static void SendPacket(byte client, Packet packet) =>
            InternetDiscoveryTCPServer.ClientDictionary[client].SendPacket(packet);
    }
}
