using Shared;
using System;
using UnityEngine;
using UnityEngine.Output;


namespace GameServer
{
    public class ServerRead
    {
        public static void WelcomeRead(byte clientID, Packet packet)
        {
            try
            {
                byte checkClientID = packet.ReadByte();
                string username = packet.ReadString();

                Output.WriteLine($"\t{Server.ClientDictionary[clientID].tCP.Socket.Client.RemoteEndPoint} connected as player: \"{username}\" : {clientID}");
                if (clientID == checkClientID)
                {
                    ServerSend.AskPlayerDetails(clientID, PlayerColor.UnAvailablePlayerColors());
                    ServerSend.SendGameState(GameStateManager.Instance.CurrentState, GameStateManager.Instance.RemainingGameTime, clientID);

                    Server.ClientDictionary[clientID].SetPlayer(username);
                    if (GameStateManager.Instance.CurrentState == GameState.Playing ||
                        GameStateManager.Instance.CurrentState == GameState.Waiting)
                    {
                        Server.ClientDictionary[clientID].SpawnOtherPlayersToConnectedUser();
                        //TODO: Send existing bots
                        return;
                    }
                }
                Output.WriteLine($"\tError, player {username} is connected as wrong player number");
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }   
        }
        public static void UDPTestRead(byte clientID, Packet packet)
        {
            try
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }   
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
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
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }   
        }
        
        internal static void ChatMessage(byte clientID, Packet packet)
        {
            try
            {
                string text = packet.ReadString();

                Server.ClientDictionary[clientID].Player.MessageToSend(text);
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }   
        }



        //Stage 1 -> Get reminder from player, that they are still connected -> send a ping packet, with player's ping and latencyID
        internal static void PlayerStillConnectedTCP(byte clientID, Packet packet)
        {
            try
            {
                PlayerServer player = Server.ClientDictionary[clientID].Player;

                player.LastPacketReceivedTCP(DateTime.Now.TimeOfDay);
                ServerSend.PlayerConnectedAcknAndPingTCP(clientID, player.Latency2WayMSTCP, player.GetLatencyIDTCP());
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }              
        }            
        internal static void PlayerStillConnectedUDP(byte clientID, Packet packet)
        {
            try
            {
                PlayerServer player = Server.ClientDictionary[clientID].Player;

                player.LastPacketReceivedUDP(DateTime.Now.TimeOfDay);
                ServerSend.PlayerConnectedAcknAndPingUDP(clientID, player.Latency2WayMSUDP, player.GetLatencyIDUDP());
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }            
        }



        //Stage 2
        internal static void PlayerPingAckTCP(byte clientID, Packet packet)
        {
            try
            {
                byte latencyIDTCP = packet.ReadByte();

                PlayerServer player = Server.ClientDictionary[clientID].Player;
                
                player.LastPacketReceivedTCP(DateTime.Now.TimeOfDay);
                player.PingAckTCP(latencyIDTCP);
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }              
        }            
        internal static void PlayerPingAckUDP(byte clientID, Packet packet)
        {
            try
            {
                byte latencyIDUDP = packet.ReadByte();

                PlayerServer player = Server.ClientDictionary[clientID].Player;

                player.LastPacketReceivedUDP(DateTime.Now.TimeOfDay);
                player.PingAckUDP(latencyIDUDP);
            }
            catch (Exception e) { 
                OutputPacketError(clientID, e);
            }            
        }

        internal static void ConstantPlayerData(byte clientID, Packet packet)
        {
            try
            {
                bool[] bits = packet.Read1ByteAs8Bools();
                PlayerServer player = Server.ClientDictionary[clientID].Player;

                if (bits[0])
                {
                    Quaternion armRotation = packet.ReadQuaternion();
                    player.PlayerArmRotation(armRotation);
                }

                if (bits[1])
                {
                    Vector2 armPosition = packet.ReadLocalVector2();
                    player.PlayerArmPosition(armPosition);
                }

                if (bits[2])
                {
                    Vector2 playerPosition = packet.ReadUVector2WorldPosition();
                    player.SetPlayerPosition(playerPosition);
                }

                if (bits[3])
                {
                    byte moveState = packet.ReadByte();
                    player.PlayerMoveState(moveState);
                }
            }
            catch (Exception e)
            {
                OutputPacketError(clientID, e);
            }
        }


        private static void OutputPacketError(byte clientID, Exception e) =>
            Output.WriteLine($"\tError, reading player:{clientID} packet...\n{e}");
    }
}
