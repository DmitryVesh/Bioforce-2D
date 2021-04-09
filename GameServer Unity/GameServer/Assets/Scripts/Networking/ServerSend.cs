using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;

namespace GameServer
{
    class ServerSend
    {
        public static void Welcome(byte recipientClient, string message, string mapName)
        {
            using (Packet packet = new Packet((byte)ServerPackets.welcome))
            {
                packet.Write(message);
                packet.Write(recipientClient);
                packet.Write(mapName);

                SendTCPPacket(recipientClient, packet);
            }
        }

        public static void UDPTest(byte recipientClient)
        {
            using (Packet packet = new Packet((byte)ServerPackets.udpTest))
            {
                packet.Write("Testing UDP");

                SendUDPPacket(recipientClient, packet);
            }
        }

        internal static void AskPlayerDetails(byte clientID, List<int> PlayerColors)
        {
            using (Packet packet = new Packet((byte)ServerPackets.askPlayerDetails))
            {
                byte numberOfPlayers = (byte)PlayerColors.Count;
                packet.Write(numberOfPlayers);
                for (byte playerCount = 0; playerCount < numberOfPlayers; playerCount++)
                    packet.Write(PlayerColors[playerCount]);

                SendTCPPacket(clientID, packet);
            }
        }

        internal static void ColorIsAvailable(int colorToFree, byte clientID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.freeColor))
            {
                packet.Write(colorToFree);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }
        internal static void ColorIsTaken(int colorToTake, byte clientID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.takeColor))
            {
                packet.Write(colorToTake);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }
        internal static void PlayerTriedTakingAlreadyTakenColor(byte clientID, List<int> unavailableColors)
        {
            using (Packet packet = new Packet((byte)ServerPackets.triedTakingTakenColor))
            {
                int numColors = unavailableColors.Count;
                packet.Write(numColors);
                for (int colorCount = 0; colorCount < numColors; colorCount++)
                    packet.Write(unavailableColors[colorCount]);

                SendTCPPacket(clientID, packet);
            }
        }

        public static void DisconnectPlayer(byte disconnectedPlayer)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerDisconnect))
            {
                packet.Write(disconnectedPlayer);
                SendTCPPacketToAllButIncluded(disconnectedPlayer, packet); // Packet has to arrive, so sending via TCP to make sure
            }
        }


        public static void SpawnPlayer(byte recipientClient, PlayerServer player, bool justJoined)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnPlayer))
            {
                packet.Write(player.ID);
                packet.Write(player.Username);
                packet.Write((Vector2)player.transform.position);
                packet.Write(player.IsFacingRight);

                packet.Write(player.RunSpeed);
                packet.Write(player.SprintSpeed);

                packet.Write(player.IsDead);
                packet.Write(justJoined);

                packet.Write(player.Kills);
                packet.Write(player.Deaths);
                packet.Write(player.Score);

                packet.Write(player.MaxHealth);
                packet.Write(player.CurrentHealth);

                packet.Write(player.PlayerColor);

                packet.Write(player.Paused);

                SendTCPPacket(recipientClient, packet);
            }
        }
        public static void PlayerPositionButLocal(byte playerID, Vector2 position, Vector2 velocity)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerPosition))
            {
                packet.Write(playerID);
                packet.Write(position);
                packet.Write(velocity);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }
        //Dont need this packet anymore
        //public static void PlayerRotationAndVelocity(byte playerID, Vector2 velocity)
        //{
        //    using (Packet packet = new Packet((byte)ServerPackets.playerRotationAndVelocity))
        //    {
        //        packet.Write(playerID);
        //        packet.Write(velocity);

        //        SendTCPPacketToAllButIncluded(playerID, packet);
        //    }
        //}

        public static void PlayerMovementStats(byte playerID, float runSpeed, float sprintSpeed)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerMovementStats))
            {
                packet.Write(playerID);
                packet.Write(runSpeed);
                packet.Write(sprintSpeed);

                SendTCPPacketToAll(packet);
            }
        }
        public static void ShotBullet(byte playerID, Vector2 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((byte)ServerPackets.bulleShot))
            {

                packet.Write(playerID);
                packet.Write(position);
                packet.Write(rotation);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }
        public static void PlayerDied(byte playerKilledID, byte bulletOwnerID, byte typeOfDeath)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerDied))
            {
                packet.Write(playerKilledID);
                packet.Write(bulletOwnerID);
                packet.Write(typeOfDeath);

                SendTCPPacketToAllButIncluded(playerKilledID, packet);
            }
        }
        public static void PlayerRespawned(byte playerID, Vector2 respawnPoint)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerRespawned))
            {
                packet.Write(playerID);
                packet.Write(respawnPoint);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }            
        }
        internal static void TookDamage(byte clientID, int damage, int currentHealth, short bulletOwner)
        {
            using (Packet packet = new Packet((byte)ServerPackets.tookDamage))
            {
                packet.Write(clientID);
                packet.Write(damage);
                packet.Write(currentHealth);
                packet.Write(bulletOwner);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }
        internal static void ArmPositionRotation(byte playerID, Vector2 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((byte)ServerPackets.armPositionRotation))
            {
                packet.Write(playerID);
                packet.Write(position);
                packet.Write(rotation);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }

        

        internal static void PlayerPausedGame(byte clientID, bool paused)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerPausedGame))
            {
                packet.Write(clientID);
                packet.Write(paused);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }
        internal static void PlayerConnectedAckn(byte clientID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.stillConnected))
            {
                SendTCPPacket(clientID, packet);
            }
        }


        private static void SendTCPPacket(byte recipientClient, Packet packet)
        {
            packet.WriteLength();
            Server.ClientDictionary[recipientClient].tCP.SendPacket(packet);
        }
        private static void SendTCPPacketToAll(Packet packet)
        {
            packet.WriteLength();
            for (byte count = 1; count < (byte)Server.MaxNumPlayers + 1; count++)
            {
                Server.ClientDictionary[count].tCP.SendPacket(packet);
            }
        }
        private static void SendTCPPacketToAllButIncluded(byte NonRecipientClient, Packet packet)
        {
            packet.WriteLength();
            for (byte count = 1; count < (byte)Server.MaxNumPlayers + 1; count++)
            {
                if (count == NonRecipientClient)
                    continue;
                Server.ClientDictionary[count].tCP.SendPacket(packet);
            }
        }

        public static void ServerIsFullPacket(byte notConnectedClient)
        {
            using (Packet packet = new Packet((byte)ServerPackets.serverIsFull))
            {
                packet.WriteLength();
                Server.NotConnectedClients[notConnectedClient].tCP.SendPacket(packet);
                Output.WriteLine($"\n\tGameServer: {Server.ServerName} is full and sent a server is full packet");
            }
        }


        private static void DisconnectAfterTime(byte notConnectedClient, int ms)
        {
            Thread.Sleep(ms);
            Server.NotConnectedClients[notConnectedClient].Disconnect();
            Output.WriteLine("\n\tDisconnected not connected client...");
        }

        private static void SendUDPPacket(byte RecipientClient, Packet packet)
        {
            packet.WriteLength();
            Server.ClientDictionary[RecipientClient].uDP.SendPacket(packet);
        }
        private static void SendUDPPacketToAll(Packet packet)
        {
            packet.WriteLength();
            for (byte count = 1; count < (byte)Server.MaxNumPlayers + 1; count++)
            {
                Server.ClientDictionary[count].uDP.SendPacket(packet);
            }
        }
        private static void SendUDPPacketToAllButIncluded(byte NonRecipientClient, Packet packet)
        {
            packet.WriteLength();
            for (byte count = 1; count < (byte)Server.MaxNumPlayers + 1; count++)
            {
                if (count == NonRecipientClient)
                    continue;
                Server.ClientDictionary[count].uDP.SendPacket(packet);
            }
        }
    }
}
