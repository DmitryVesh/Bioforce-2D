using Shared;
using System;
using UnityEngine;
using UnityEngine.Output;
using System.Collections.Generic;


namespace GameServer
{
    public class ServerRead
    {
        public static void WelcomeRead(byte clientID, Packet packet)
        {
            byte checkClientID = packet.ReadByte();
            string username = packet.ReadString();

            Output.WriteLine($"\t{Server.ClientDictionary[clientID].tCP.Socket.Client.RemoteEndPoint} connected as player: \"{username}\" : {clientID}");
            if (clientID == checkClientID)
            {
                ServerSend.AskPlayerDetails(clientID, PlayerColor.UnAvailablePlayerColors());
                ServerSend.SendGameState(GameStateManager.Instance.CurrentState, GameStateManager.Instance.RemainingGameTime, clientID);
                
                Server.ClientDictionary[clientID].SetPlayer(username);
                Server.ClientDictionary[clientID].SpawnOtherPlayersToConnectedUser();
                return;
            }
            Output.WriteLine($"\tError, player {username} is connected as wrong player number");
        }
        public static void UDPTestRead(byte clientID, Packet packet)
        {
            byte checkClientID = packet.ReadByte();
            string message = packet.ReadString();

            Output.WriteLine($"\tUDP Test received from: {checkClientID}, Message: {message}\n");
            if (clientID == checkClientID)
            {
                return;
            }
            Output.WriteLine($"\tError, player {clientID} is connected as wrong player number {checkClientID}");
        }

        internal static void ColorToFreeAndToTake(byte clientID, Packet packet)
        {
            try
            {
                int colorToFree = packet.ReadInt();
                int colorToTake = packet.ReadInt();

                PlayerColor.FreeColor(colorToFree, clientID);
                PlayerColor.TakeColor(colorToTake, clientID);
                Server.ClientDictionary[clientID].Player.PlayerColorIndex = colorToTake;
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read ColorToFreeAndToTAke, from player: {clientID}...\n{exception}");
            }
        }
        internal static void ReadyToJoin(byte clientID, Packet packet)
        {
            try
            {
                int colorIndex = packet.ReadInt();
                GameStateManager.Instance.PlayerJoinedServer();
                Server.ClientDictionary[clientID].SendIntoGame(colorIndex);
                
                Output.WriteLine($"\tPlayer: {clientID} was sent into game.");
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read ReadyToJoin, from player: {clientID}...\n{exception}");
            }
        }

        internal static void ConstantPlayerData(byte clientID, Packet packet)
        {
            try {
                bool[] bits = packet.Read1ByteAs8Bools();
                PlayerServer player = Server.ClientDictionary[clientID].Player;
                
                if (bits[0])
                {
                    UnityEngine.Quaternion armRotation = packet.ReadQuaternion();
                    player.PlayerArmRotation(armRotation);
                }

                if (bits[1])
                {
                    UnityEngine.Vector2 armPosition = packet.ReadLocalVector2();
                    player.PlayerArmPosition(armPosition);
                }

                if (bits[2])
                {
                    UnityEngine.Vector2 playerPosition = packet.ReadUVector2WorldPosition();
                    player.PlayerPosition(playerPosition);
                }

                if (bits[3])
                {
                    byte moveState = packet.ReadByte();
                    player.PlayerMoveState(moveState);
                }
            }
            catch (Exception e) {
                OutputPacketError(clientID, e);
            }
        }

        private static void OutputPacketError(byte clientID, Exception e) =>
            Output.WriteLine($"\tError, reading player:{clientID} packet...\n{e}");

        public static void PlayerMovementRead(byte clientID, Packet packet)
        {
            try
            {
                UnityEngine.Vector2 position = packet.ReadUVector2WorldPosition();
                byte moveState = packet.ReadByte();
                PlayerServer player = Server.ClientDictionary[clientID].Player;
                player.PlayerPosition(position);
                player.PlayerMoveState(moveState);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read player movement, from player: {clientID}...\n{exception}");
            }
        }
        public static void PlayerMovementStatsRead(byte clientID, Packet packet)
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
                Output.WriteLine($"\tError, trying to read player movement stats, from player: {clientID}\n{exception}");
            }
            
        }
        public static void ShotBulletRead(byte clientID, Packet packet)
        {
            try
            {
                Vector2 position = packet.ReadUVector2WorldPosition();
                Quaternion rotation = packet.ReadQuaternion();
                ServerSend.ShotBullet(clientID, position, rotation);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read shot bullet, from player: {clientID}\n{exception}");
            }
        }

        
        public static void PlayerDiedRead(byte clientID, Packet packet)
        {
            try
            {
                byte bulletOwnerID = packet.ReadByte();
                byte typeOfDeath = packet.ReadByte();
                ServerSend.PlayerDied(clientID, bulletOwnerID, typeOfDeath);
                if (clientID != bulletOwnerID)
                    Server.ClientDictionary[bulletOwnerID].Player.AddKill();
                Server.ClientDictionary[clientID].Player.Died();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read player died, from player: {clientID}\n{exception}");
            }
        }
        public static void PlayerRespawnedRead(byte clientID, Packet packet)
        {
            try
            {
                Vector2 respawnPoint = packet.ReadUVector2WorldPosition();
                ServerSend.PlayerRespawned(clientID, respawnPoint);
                Server.ClientDictionary[clientID].Player.Respawned();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read player respawned, from player: {clientID}\n{exception}");
            }
        }

        public static void TookDamageRead(byte clientID, Packet packet)
        {
            int currentHealth = 0, damage = 0;
            short bulletOwner = -1;
            try
            {
                damage = packet.ReadInt();
                currentHealth = packet.ReadInt();
                bulletOwner = packet.ReadShort();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read player took damage, from player: {clientID}\n{exception}");
            }
            finally
            {
                Server.ClientDictionary[clientID].Player.CurrentHealth = currentHealth;
                ServerSend.TookDamage(clientID, damage, currentHealth, bulletOwner);
            }
        }

        

        internal static void PlayerPausedGame(byte clientID, Packet packet)
        {
            try
            {
                bool paused = packet.ReadBool();
                Server.ClientDictionary[clientID].Player.Paused = paused;
                ServerSend.PlayerPausedGame(clientID, paused);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read PausedGame, from player: {clientID}\n{exception}");
            }
        }
        
        internal static void ChatMessage(byte clientID, Packet packet)
        {
            try
            {
                string text = packet.ReadString();

                Server.ClientDictionary[clientID].Player.MessageToSend(text);
            }
            catch (Exception e)
            {
                Output.WriteLine($"\tError, trying to read ChatMessage, from player: {clientID}\n{e}");
            }
        }


        internal static void PlayerStillConnectedTCP(byte clientID, Packet packet)
        {
            try
            {
                byte latencyID = packet.ReadByte();

                Server.ClientDictionary[clientID].Player.LastPacketReceivedTCP(DateTime.Now.TimeOfDay);
                ServerSend.PlayerConnectedAcknTCP(clientID, latencyID);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read PlayerStillConnectedTCP, from player: {clientID}\n{exception}");
            }            
        }

        

        internal static void PlayerStillConnectedUDP(byte clientID, Packet packet)
        {
            try
            {
                byte latencyID = packet.ReadByte();

                Server.ClientDictionary[clientID].Player.LastPacketReceivedUDP(DateTime.Now.TimeOfDay);
                ServerSend.PlayerConnectedAcknUDP(clientID, latencyID);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read PlayerStillConnectedUDP, from player: {clientID}\n{exception}");
            }            
        }


        internal static void ArmPositionRotation(byte clientID, Packet packet)
        {
            try
            {
                Vector2 position = packet.ReadLocalVector2();
                Quaternion rotation = packet.ReadQuaternion();

                //TODO: make so all packets from player are sent in Update
                //Server.ClientDictionary[clientID].player.SetArmPositionRotation(position, rotation);
                PlayerServer player = Server.ClientDictionary[clientID].Player;
                player.PlayerArmRotation(rotation);
                player.PlayerArmPosition(position);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError, trying to read player's arm position and rotation, from player: {clientID}\n{exception}");
            }
        }
    }
}
