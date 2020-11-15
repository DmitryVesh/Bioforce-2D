
using System;
using System.Numerics;

namespace GameServer
{
    class ServerSend
    {
        public static void Welcome(int recipientClient, string message)
        {
            Packet packet = new Packet((int)ServerPackets.welcome);

            packet.Write(message);
            packet.Write(recipientClient);
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


        public static void SpawnPlayer(int recipientClient, Player player)
        {
            Packet packet = new Packet((int)ServerPackets.spawnPlayer);
            packet.Write(player.ID);
            packet.Write(player.Username);
            packet.Write(player.Position);
            packet.Write(player.Rotation);

            SendTCPPacket(recipientClient, packet);
        }
        public static void PlayerPosition(int playerID, Vector3 position)
        {
            Packet packet = new Packet((int)ServerPackets.playerPosition);
            packet.Write(playerID);
            packet.Write(position);

            SendUDPPacketToAll(packet);
        }
        public static void PlayerPositionButLocal(int playerID, Vector3 position)
        {
            Packet packet = new Packet((int)ServerPackets.playerPosition);
            packet.Write(playerID);
            packet.Write(position);

            SendUDPPacketToAllButIncluded(playerID, packet);
        }
        public static void PlayerVelocity(int playerID, Vector3 velocity)
        {
            Packet packet = new Packet((int)ServerPackets.playerVelocity);
            packet.Write(playerID);
            packet.Write(velocity);

            SendUDPPacketToAllButIncluded(playerID, packet);
        }
        public static void PlayerRotation(int playerID, Quaternion rotation)
        {
            Packet packet = new Packet((int)ServerPackets.playerRotation);
            packet.Write(playerID);
            packet.Write(rotation);

            //SendUDPPacketToAllButIncluded(playerID, packet);
            SendTCPPacketToAllButIncluded(playerID, packet);
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
