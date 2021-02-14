

using Shared;
using System;
using System.Numerics;

namespace GameServer
{
    class ServerSend
    {
        public static void Welcome(int recipientClient, string message, string mapName)
        {
            Packet packet = new Packet((int)ServerPackets.welcome);

            packet.Write(message);
            packet.Write(recipientClient);
            packet.Write(mapName);
            SendTCPPacket(recipientClient, packet);
        }
        public static void UDPTest(int recipientClient)
        {
            Packet packet = new Packet((int)ServerPackets.udpTest);
            packet.Write("Testing UDP");
            SendUDPPacket(recipientClient, packet);
        }
        public static void DisconnectPlayer(int disconnectedPlayer)
        {
            Packet packet = new Packet((int)ServerPackets.playerDisconnect);
            packet.Write(disconnectedPlayer);
            SendTCPPacketToAll(packet); // Packet has to arrive, so sending via TCP to make sure
        }


        public static void SpawnPlayer(int recipientClient, PlayerServer player, bool justJoined)
        {
            Packet packet = new Packet((int)ServerPackets.spawnPlayer);
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

            SendTCPPacket(recipientClient, packet);
        }
        public static void PlayerPosition(int playerID, Vector2 position)
        {
            Packet packet = new Packet((int)ServerPackets.playerPosition);
            packet.Write(playerID);
            packet.Write(position);

            SendUDPPacketToAll(packet);
        }
        public static void PlayerPositionButLocal(int playerID, Vector2 position)
        {
            Packet packet = new Packet((int)ServerPackets.playerPosition);
            packet.Write(playerID);
            packet.Write(position);

            SendUDPPacketToAllButIncluded(playerID, packet);
        }
        public static void PlayerRotationAndVelocity(int playerID, bool isFacingRight, Vector2 velocity, Quaternion rotation)
        {
            Packet packet = new Packet((int)ServerPackets.playerRotationAndVelocity);
            packet.Write(playerID);
            packet.Write(isFacingRight);
            packet.Write(velocity);
            packet.Write(rotation);

            SendUDPPacketToAllButIncluded(playerID, packet);
        }
        public static void PlayerMovementStats(int playerID, float runSpeed, float sprintSpeed)
        {
            Packet packet = new Packet((int)ServerPackets.playerMovementStats);
            packet.Write(playerID);
            packet.Write(runSpeed);
            packet.Write(sprintSpeed);

            SendTCPPacketToAll(packet);
        }
        public static void ShotBullet(int playerID, Vector2 position, Quaternion rotation)
        {
            Packet packet = new Packet((int)ServerPackets.bulleShot);
            packet.Write(playerID);
            packet.Write(position);
            packet.Write(rotation);

            SendTCPPacketToAllButIncluded(playerID, packet);
        }
        public static void PlayerDied(int playerKilledID, int bulletOwnerID, int typeOfDeath)
        {
            Packet packet = new Packet((int)ServerPackets.playerDied);
            packet.Write(playerKilledID);
            packet.Write(bulletOwnerID);
            packet.Write(typeOfDeath);

            SendTCPPacketToAllButIncluded(playerKilledID, packet);
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
            Packet packet = new Packet((int)ServerPackets.tookDamage);
            packet.Write(clientID);
            packet.Write(damage);
            packet.Write(currentHealth);

            SendTCPPacketToAllButIncluded(clientID, packet);
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
