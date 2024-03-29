﻿using Shared;
using System;

namespace GameServer
{
    internal class MainServerCommsSend
    {
        internal static void WelcomeReceived(string serverName)
        {
            try
            {
                using (Packet packet = new Packet((int)ServerToMainServer.welcomeReceived))
                {
                    packet.Write(serverName);
                    SendTCPPacket(packet);
                    Console.WriteLine($"\n\t\tGameServer: {serverName} sent Received Welcome packet to MainServer.");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n\t\tGameServer: {serverName} had error sending WelcomeReceivedPacket\n{exception}");
            }
        }

        internal static void ServerData(string serverName, int currentNumPlayers, int maxNumPlayers, string mapName)
        {
            using (Packet packet = new Packet((int)ServerToMainServer.serverData))
            {
                packet.Write(serverName);
                packet.Write(currentNumPlayers);
                packet.Write(maxNumPlayers);
                packet.Write(mapName);
                SendTCPPacket(packet);
            }
            //Console.WriteLine($"\n\t\tGameServer: {serverName} sent ServerData packet to MainServer.");
        }

        internal static void ShuttingDown(string serverName)
        {
            using (Packet packet = new Packet((int)ServerToMainServer.shuttingDown))
            {
                packet.Write(serverName);
                SendTCPPacket(packet);
            }
            Console.WriteLine($"\n\t\tGameServer: {serverName} sent ShuttingDown packet to MainServer.");
        }

        private static void SendTCPPacket(Packet packet)
        {
            packet.WriteLength();
            MainServerComms.tCP.SendPacket(packet);
        }
    }
}