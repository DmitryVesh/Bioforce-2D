using Shared;
using System;
using System.Numerics;
using System.Threading;

namespace GameServer
{
    class ServerSend
    {
        public static void Welcome(int recipientClient, string message, string mapName)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(message);
                packet.Write(recipientClient);
                packet.Write(mapName);
                SendTCPPacket(recipientClient, packet);
            }
        }
        public static void UDPTest(int recipientClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.udpTest))
            {
                packet.Write("Testing UDP");
                SendUDPPacket(recipientClient, packet);
            }
        }
        public static void DisconnectPlayer(int disconnectedPlayer)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerDisconnect))
            {
                packet.Write(disconnectedPlayer);
                SendTCPPacketToAllButIncluded(disconnectedPlayer, packet); // Packet has to arrive, so sending via TCP to make sure
            }
        }


        public static void SpawnPlayer(int recipientClient, PlayerServer player, bool justJoined)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player.ID);
                packet.Write(player.Username);
                packet.Write(player.Position);
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

                packet.Write(player.PlayerColor.R);
                packet.Write(player.PlayerColor.G);
                packet.Write(player.PlayerColor.B);

                packet.Write(player.Paused);

                SendTCPPacket(recipientClient, packet);
            }
        }
        public static void PlayerPositionButLocal(int playerID, Vector2 position)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerPosition))
            {
                packet.Write(playerID);
                packet.Write(position);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }
        public static void PlayerRotationAndVelocity(int playerID, bool isFacingRight, Vector2 velocity, Quaternion rotation)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRotationAndVelocity))
            {
                packet.Write(playerID);
                packet.Write(isFacingRight);
                packet.Write(velocity);
                packet.Write(rotation);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }
        public static void PlayerMovementStats(int playerID, float runSpeed, float sprintSpeed)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerMovementStats))
            {
                packet.Write(playerID);
                packet.Write(runSpeed);
                packet.Write(sprintSpeed);

                SendTCPPacketToAll(packet);
            }
        }
        public static void ShotBullet(int playerID, Vector2 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((int)ServerPackets.bulleShot))
            {

                packet.Write(playerID);
                packet.Write(position);
                packet.Write(rotation);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }
        public static void PlayerDied(int playerKilledID, int bulletOwnerID, int typeOfDeath)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerDied))
            {
                packet.Write(playerKilledID);
                packet.Write(bulletOwnerID);
                packet.Write(typeOfDeath);

                SendTCPPacketToAllButIncluded(playerKilledID, packet);
            }
        }
        public static void PlayerRespawned(int playerID, Vector2 respawnPoint)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRespawned))
            {
                packet.Write(playerID);
                packet.Write(respawnPoint);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }            
        }
        internal static void TookDamage(int clientID, int damage, int currentHealth)
        {
            using (Packet packet = new Packet((int)ServerPackets.tookDamage))
            {
                packet.Write(clientID);
                packet.Write(damage);
                packet.Write(currentHealth);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }
        internal static void ArmPositionRotation(int playerID, Vector2 position, Quaternion rotation)
        {
            using (Packet packet = new Packet((int)ServerPackets.armPositionRotation))
            {
                packet.Write(playerID);
                packet.Write(position);
                packet.Write(rotation);

                SendTCPPacketToAllButIncluded(playerID, packet);
            }
        }

        internal static void PlayerPausedGame(int clientID, bool paused)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerPausedGame))
            {
                packet.Write(clientID);
                packet.Write(paused);

                SendTCPPacketToAllButIncluded(clientID, packet);
            }
        }

        private static void SendTCPPacket(int recipientClient, Packet packet)
        {
            packet.WriteLength();
            Server.ClientDictionary[recipientClient].tCP.SendPacket(packet);
        }
        private static void SendTCPPacketToAll(Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                //TODO: maybe check if the client is null or something to fix the simultaneous exit crash
                Server.ClientDictionary[count].tCP.SendPacket(packet);
            }
        }
        private static void SendTCPPacketToAllButIncluded(int NonRecipientClient, Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                if (count == NonRecipientClient)
                    continue;
                Server.ClientDictionary[count].tCP.SendPacket(packet);
            }
        }

        public static void ServerIsFullPacket(int notConnectedClient)
        {
            using (Packet packet = new Packet())
            {
                packet.Write((int)ServerPackets.serverIsFull);
                packet.WriteLength();
                Server.NotConnectedClients[notConnectedClient].tCP.SendPacket(packet);
                Console.WriteLine($"\n\tGameServer: {Server.ServerName} is full and sent a server is full packet");
            }
        }


        private static void DisconnectAfterTime(int notConnectedClient, int ms)
        {
            Thread.Sleep(ms);
            Server.NotConnectedClients[notConnectedClient].Disconnect();
            Console.WriteLine("\n\tDisconnected not connected client...");
        }

        private static void SendUDPPacket(int RecipientClient, Packet packet)
        {
            packet.WriteLength();
            Server.ClientDictionary[RecipientClient].uDP.SendPacket(packet);
        }
        private static void SendUDPPacketToAll(Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                Server.ClientDictionary[count].uDP.SendPacket(packet);
            }
        }
        private static void SendUDPPacketToAllButIncluded(int NonRecipientClient, Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                if (count == NonRecipientClient)
                    continue;
                Server.ClientDictionary[count].uDP.SendPacket(packet);
            }
        }
    }
}
