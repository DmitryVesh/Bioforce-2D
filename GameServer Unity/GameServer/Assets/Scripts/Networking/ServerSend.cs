using Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Output;

namespace GameServer
{
    public enum SendConstantPacketsState
    {
        UDP,
        UDPandTCP,
        TCP
    }
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
                Output.WriteLine($"\n\tSending UDPTest packet to client: {recipientClient}");
                packet.Write("Testing UDP");

                SendUDPPacket(recipientClient, packet);
            }
        }

        // max 138B (byte 1B packetLen + byte 1B packetID + byte 1B numPlayers + xint xMax=16 16*4B = 64B playerColors 
        //          + byte 1B numPickups + ypickups yMax=10 10*7B = 70B pickups
        internal static void AskPlayerDetails(byte clientID, List<int> PlayerColors)
        {
            using (Packet packet = new Packet((byte)ServerPackets.askPlayerDetails))
            {
                //Colors
                byte numberOfPlayers = (byte)PlayerColors.Count;
                packet.Write(numberOfPlayers);
                for (byte playerCount = 0; playerCount < numberOfPlayers; playerCount++)
                    packet.Write(PlayerColors[playerCount]);

                //Pickups
                packet.Write((byte)PickupItemsManager.Instance.PickupsDictionary.Count);
                foreach (PickupItem pickup in PickupItemsManager.Instance.PickupsDictionary.Values)
                    WritePickupData(pickup, packet);

                SendTCPPacket(clientID, packet);
            }
        }

        // 7B (byte 1B packetLen + byte 1B packetID + byte 1B currentState, float 4B remainingGameTime)
        internal static void SendGameState(GameState currentState, float remainingGameTime)
        {
            using (Packet packet = new Packet((byte)ServerPackets.gameState))
            {
                packet.Write((byte)currentState);
                packet.Write(remainingGameTime);

                SendTCPPacketToAll(packet);
            }
        }
        // 7B (byte 1B packetLen + byte 1B packetID + byte 1B currentState, float 4B remainingGameTime)
        internal static void SendGameState(GameState currentState, float remainingGameTime, byte clientID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.gameState))
            {
                packet.Write((byte)currentState);
                packet.Write(remainingGameTime);

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

        // 63B (byte 1B packetLen + byte 1B packetID + byte 1B playerID + string 16B-max = 4B+12B 12Chars playerName + Vector2 4B playerPosition +
        //      bool 1B isFacingRight + float 4B runSpeed + float 4B sprintSpeed + bool 1B isDead + bool 1B justJoined + int 4B Kills +
        //      int 4B Deaths + int 4B Score + int 4B MaxHealth + int 4B CurrentHealth + int 4B PlayerColor + bool 1B Paused +
        //      float 4B CurrentInvincibilityTime
        public static void SpawnPlayer(byte recipientClient, PlayerServer player, bool justJoined)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnPlayer))
            {
                packet.Write(player.ID);
                packet.Write(player.Username);
                packet.WriteWorldUVector2((Vector2)player.transform.position);
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

                packet.Write(player.PlayerColorIndex);

                packet.Write(player.Paused);

                packet.Write(player.CurrentInvincibilityTime);

                SendTCPPacket(recipientClient, packet);
            }
        }

        


        // Constantly sent
        // TCP 8B (byte 1B packetLen + byte 1B packetID + byte 1B playerID + Vector2 4B position + PlayerMovingState 1B movingState)
        // UDP 8B (byte 1B packetLen + byte 1B packetID + byte 1B playerID + Vector2 4B position + PlayerMovingState 1B movingState)
        public static void PlayerPositionButLocal(byte playerID, Vector2 position, byte moveState)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerPosition))
            {
                packet.Write(playerID);
                packet.WriteWorldUVector2(position);
                packet.Write(moveState);

                //SendTCPPacketToAllButIncluded(playerID, packet);
                SendConstantlySentPacketToAllButIncluded(playerID, packet);
            }
        }


        // Constantly sent
        // TCP 8B (byte 1B PacketLen + byte 1B packetID + byte 1B playerID + Vector2 2B position + 3B Quaternion rotation)
        // UDP 8B (byte 1B PacketLen + byte 1B packetID + byte 1B playerID + Vector2 2B position + 3B Quaternion rotation)
        internal static void ArmPositionRotation(byte playerID, Vector2 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((byte)ServerPackets.armPositionRotation))
            {
                packet.Write(playerID);
                packet.WriteLocalPosition(position);
                packet.Write(rotation);

                //SendTCPPacketToAllButIncluded(playerID, packet);
                SendConstantlySentPacketToAllButIncluded(playerID, packet);
            }
        }

        // Constantly sent
        // 3B (byte 1B packetLen + byte 1B packetID + byte 1B latencyID)
        internal static void PlayerConnectedAcknTCP(byte clientID, byte latencyID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.stillConnectedTCP))
            {
                packet.Write(latencyID);

                SendTCPPacket(clientID, packet);
            }
        }
        // Constantly sent
        // 3B (byte 1B packetLen + byte 1B packetID + byte 1B latencyID)
        internal static void PlayerConnectedAcknUDP(byte clientID, byte latencyID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.stillConnectedUDP))
            {
                packet.Write(latencyID);

                SendUDPPacket(clientID, packet);
            }
        }

        private static void SendConstantlySentPacketToAllButIncluded(byte NonRecipientClient, Packet packet)
        {
            packet.WriteLength();

            for (byte count = 1; count < (byte)Server.MaxNumPlayers + 1; count++)
            {
                if (count == NonRecipientClient || Server.ClientDictionary[count].Player == null)
                    continue;

                SendConstantPacketsState sendVia = Server.ClientDictionary[count].Player.CurrentSendConstantPacketsState;
                switch (sendVia)
                {
                    case SendConstantPacketsState.UDP:
                        Server.ClientDictionary[count].uDP.SendPacket(packet);
                        break;
                    case SendConstantPacketsState.UDPandTCP:
                        using (Packet samePacket = new Packet(packet.ToArray()))
                        {
                            Server.ClientDictionary[count].tCP.SendPacket(packet);
                            Server.ClientDictionary[count].uDP.SendPacket(packet);
                        }
                        break;
                    case SendConstantPacketsState.TCP:
                        Server.ClientDictionary[count].tCP.SendPacket(packet);
                        break;
                }

            }
        }


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
                packet.WriteWorldUVector2(position);
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
                packet.WriteWorldUVector2(respawnPoint);

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

        internal static void PlayerPausedGame(byte clientID, bool paused)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerPausedGame))
            {
                packet.Write(clientID);
                packet.Write(paused);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }

        internal static void GeneratedPickupItem(PickupItem pickup)
        {
            using (Packet packet = new Packet((byte)ServerPackets.generatedPickup))
            {
                WritePickupData(pickup, packet);

                SendTCPPacketToAll(packet);
            }
        }
        private static void WritePickupData(PickupItem pickup, Packet packet)
        {
            packet.Write((byte)pickup.PickupType);
            packet.Write(pickup.PickupID);
            packet.WriteWorldUVector2(pickup.transform.position);
        }
        // Bandage/Medkit 9B (byte 1B packetLen + byte 1B packetID + ushort 2B pickupID + byte 1B clientID + int 4B restoreHealth)
        // Adrenaline 17B (byte 1B packetLen + byte 1B packetID + ushort 2B pickupID + byte 1B clientID + 
        //                  TimeSpan 8B timeOfInvincibilityStart + float 4B invincibilityTime)
        internal static void PlayerPickedUpItem(byte clientID, ushort pickupID, PickupItem pickup)
        {
            using (Packet packet = new Packet((byte)ServerPackets.playerPickedUpItem))
            {
                packet.Write(pickupID);
                packet.Write(clientID);

                switch (pickup.PickupType)
                {
                    case PickupType.bandage:
                    case PickupType.medkit:
                        packet.Write(((HealthPickup)pickup).Restore);
                        break;
                    case PickupType.adrenaline:
                        //Output.WriteLine($"TimeNow: {DateTime.UtcNow.TimeOfDay}, TimeNowTicks: {DateTime.UtcNow.TimeOfDay.Ticks}");
                        //packet.Write(DateTime.UtcNow.TimeOfDay); //Need so players' are synchronised with server
                        packet.Write(((AdrenalinePickup)pickup).InvincibilityTime);
                        break;
                }

                SendTCPPacketToAll(packet);
            }
        }

        internal static void ChatMessage(string chatEntryToSend, byte clientID)
        {
            using (Packet packet = new Packet((byte)ServerPackets.chatMessage))
            {
                packet.Write(chatEntryToSend);
                packet.Write(clientID);

                SendTCPPacketToAll(packet);
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
