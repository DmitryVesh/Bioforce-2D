using System;
using System.Numerics;

namespace GameServer
{
    public class ServerRead
    {
        public static void WelcomeRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.ClientDictionary[clientID].tCP.Socket.Client.RemoteEndPoint} connected as player: {clientID}");
            if (clientID == checkClientID)
            {
                Server.ClientDictionary[clientID].SendIntoGame(username);
                Console.WriteLine($"Player: {clientID} was sent into game.");
                return;
            }
            Console.WriteLine($"Error, player {username} is connected as wrong player number");
        }
        public static void UDPTestRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string message = packet.ReadString();

            Console.WriteLine($"UDP Test received from: {checkClientID}, Message: {message}\n");
            if (clientID == checkClientID)
            {
                return;
            }
            Console.WriteLine($"Error, player {clientID} is connected as wrong player number {checkClientID}");
        }
        public static void PlayerMovementRead(int clientID, Packet packet)
        {
            try
            {
                bool isFacingRight = packet.ReadBool();
                //TODO: Change position to be vector2
                Vector2 position = packet.ReadVector2();
                Vector2 velocity = packet.ReadVector2();
                //Sometimes System.NullReferenceException when a player disconnects
                Server.ClientDictionary[clientID].player.PlayerMoves(isFacingRight, position, velocity);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read player movement, when a player: {clientID} has disconnected...\n{exception}");
            }
        }
        public static void PlayerMovementStatsRead(int clientID, Packet packet)
        {
            try
            {
                float runSpeed = packet.ReadFloat();
                float sprintSpeed = packet.ReadFloat();

                Server.ClientDictionary[clientID].player.SetPlayerMovementStats(runSpeed, sprintSpeed);
                ServerSend.PlayerMovementStats(clientID, runSpeed, sprintSpeed);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read player movement stats, from player: {clientID}\n{exception}");
            }
            
        }
        public static void ShotBulletRead(int clientID, Packet packet)
        {
            try
            {
                Vector2 position = packet.ReadVector2();
                Quaternion rotation = packet.ReadQuaternion();
                ServerSend.ShotBullet(clientID, position, rotation);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read shot bullet, from player: {clientID}\n{exception}");
            }
        }

        
        public static void PlayerDiedRead(int clientID, Packet packet)
        {
            try
            {
                int bulletOwnerID = packet.ReadInt();
                int typeOfDeath = packet.ReadInt();
                ServerSend.PlayerDied(clientID, bulletOwnerID, typeOfDeath);
                if (clientID != bulletOwnerID)
                    Server.ClientDictionary[bulletOwnerID].player.AddKill();
                Server.ClientDictionary[clientID].player.Died();
                //TODO: UPDATE GAMELOGIC kill score
                //GameLogic.
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read player died, from player: {clientID}\n{exception}");
            }
        }
        public static void PlayerRespawnedRead(int clientID, Packet packet)
        {
            try
            {
                ServerSend.PlayerRespawned(clientID);
                Server.ClientDictionary[clientID].player.Respawned();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read player respawned, from player: {clientID}\n{exception}");
            }
        }

        public static void TookDamageRead(int clientID, Packet packet)
        {
            try
            {
                int damage = packet.ReadInt();
                int currentHealth = packet.ReadInt();

                ServerSend.TookDamage(clientID, damage, currentHealth);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error, trying to read player took damage, from player: {clientID}\n{exception}");
            }
        }

    }
}
