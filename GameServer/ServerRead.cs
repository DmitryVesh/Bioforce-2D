using System;
using System.Numerics;
using Shared;

namespace GameServer
{
    public class ServerRead
    {
        public static void WelcomeRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"\t{Server.ClientDictionary[clientID].tCP.Socket.Client.RemoteEndPoint} connected as player: {clientID}");
            if (clientID == checkClientID)
            {
                Server.ClientDictionary[clientID].SendIntoGame(username);
                Console.WriteLine($"\tPlayer: {clientID} was sent into game.");
                return;
            }
            Console.WriteLine($"\tError, player {username} is connected as wrong player number");
        }
        public static void UDPTestRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string message = packet.ReadString();

            Console.WriteLine($"\tUDP Test received from: {checkClientID}, Message: {message}\n");
            if (clientID == checkClientID)
            {
                return;
            }
            Console.WriteLine($"\tError, player {clientID} is connected as wrong player number {checkClientID}");
        }
        public static void PlayerMovementRead(int clientID, Packet packet)
        {
            try
            {
                bool isFacingRight = packet.ReadBool();
                Vector2 position = packet.ReadVector2();
                Vector2 velocity = packet.ReadVector2();
                Quaternion rotation = packet.ReadQuaternion();
                ServerSend.PlayerPositionButLocal(clientID, position);
                ServerSend.PlayerRotationAndVelocity(clientID, isFacingRight, velocity, rotation);
                //Sometimes System.NullReferenceException when a player disconnects
                //Server.ClientDictionary[clientID].Player.PlayerMoves(isFacingRight, position, velocity, rotation);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player movement, when a player: {clientID} has disconnected...\n{exception}");
            }
        }
        public static void PlayerMovementStatsRead(int clientID, Packet packet)
        {
            try
            {
                float runSpeed = packet.ReadFloat();
                float sprintSpeed = packet.ReadFloat();

                Server.ClientDictionary[clientID].Player.SetPlayerMovementStats(runSpeed, sprintSpeed);
                ServerSend.PlayerMovementStats(clientID, runSpeed, sprintSpeed);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player movement stats, from player: {clientID}\n{exception}");
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
                Console.WriteLine($"\tError, trying to read shot bullet, from player: {clientID}\n{exception}");
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
                    Server.ClientDictionary[bulletOwnerID].Player.AddKill();
                Server.ClientDictionary[clientID].Player.Died();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player died, from player: {clientID}\n{exception}");
            }
        }
        public static void PlayerRespawnedRead(int clientID, Packet packet)
        {
            try
            {
                Vector2 respawnPoint = packet.ReadVector2();
                ServerSend.PlayerRespawned(clientID, respawnPoint);
                Server.ClientDictionary[clientID].Player.Respawned();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player respawned, from player: {clientID}\n{exception}");
            }
        }

        public static void TookDamageRead(int clientID, Packet packet)
        {
            try
            {
                int damage = packet.ReadInt();
                int currentHealth = packet.ReadInt();
                Server.ClientDictionary[clientID].Player.CurrentHealth = currentHealth;
                ServerSend.TookDamage(clientID, damage, currentHealth);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player took damage, from player: {clientID}\n{exception}");
            }
        }

        internal static void PlayerPausedGame(int clientID, Packet packet)
        {
            try
            {
                bool paused = packet.ReadBool();
                Server.ClientDictionary[clientID].Player.SetPaused(paused);

                ServerSend.PlayerPausedGame(clientID, paused);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read PausedGame, from player: {clientID}\n{exception}");
            }
        }

        internal static void ArmPositionRotation(int clientID, Packet packet)
        {
            try
            {
                Vector2 position = packet.ReadVector2();
                Quaternion rotation = packet.ReadQuaternion();
                //TODO: make so all packets from player are sent in Update, apart from bullet
                //Server.ClientDictionary[clientID].player.SetArmPositionRotation(position, rotation);
                ServerSend.ArmPositionRotation(clientID, position, rotation);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\tError, trying to read player's arm position and rotation, from player: {clientID}\n{exception}");
            }
        }
    }
}
