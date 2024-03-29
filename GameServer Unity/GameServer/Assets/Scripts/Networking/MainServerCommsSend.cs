﻿using Shared;
using System;
using UnityEngine.Output;

namespace GameServer
{
    internal class MainServerCommsSend
    {
        internal static void WelcomeReceived(string serverName)
        {
            try
            {
                using (Packet packet = new Packet((byte)ServerToMainServer.welcomeReceived))
                {
                    packet.Write(serverName);
                    SendTCPPacket(packet);
                    Output.WriteLine($"\t\tGameServer: {serverName} sent Received Welcome packet to MainServer.");
                }
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\n\t\tGameServer: {serverName} had error sending WelcomeReceivedPacket\n{exception}");
            }
        }

        internal static void ServerData(string serverName, GameState serverState, int currentNumPlayers, int maxNumPlayers, string mapName)
        {
            using (Packet packet = new Packet((byte)ServerToMainServer.serverData))
            {
                packet.Write(serverName);
                packet.Write((byte)serverState);
                packet.Write(currentNumPlayers);
                packet.Write(maxNumPlayers);
                packet.Write(mapName);

                SendTCPPacket(packet);
            }
        }

        internal static void ShuttingDown(string serverName)
        {
            using (Packet packet = new Packet((byte)ServerToMainServer.shuttingDown))
            {
                packet.Write(serverName);
                SendTCPPacket(packet);
            }
            Output.WriteLine($"\n\t\tGameServer: {serverName} sent ShuttingDown packet to MainServer.");
        }

        private static void SendTCPPacket(Packet packet)
        {
            packet.WriteLength();
            MainServerComms.tCP.SendPacket(packet);
        }
    }
}