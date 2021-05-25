using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Output;
using Shared;

public class ClientRead : MonoBehaviour
{
    #region Important Packets That must arrive
    public static void WelcomeRead(Packet packet)
    {
        try
        {
            string message = packet.ReadString();
            byte id = packet.ReadByte();
            string mapName = packet.ReadString();
            Output.WriteLine($"Message from server:\n{message}");
            Client.Instance.SuccessfullyConnected(id);
            Client.Instance.uDP.Connect(((IPEndPoint)Client.Instance.tCP.Socket.Client.LocalEndPoint).Port);

            ClientSend.WelcomePacketReply();

            GameManager.Instance.SwitchScene(mapName);

            GameManager.Instance.InGame = true;

            InternetServerScanner.Instance.Disconnect();
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    public static void UDPTestRead(Packet packet)
    {
        try 
        { 
            Output.WriteLine($"Received packet via UDP: {packet.ReadString()}");
            ClientSend.UDPTestPacketReply();
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void AskingForPlayerDetails(Packet packet)
    {
        try 
        { 
            Output.WriteLine("Have Received AskingForPlayerDetails packet");

            //Colors
            byte numPlayers = packet.ReadByte();
            List<int> playerColors = new List<int>();
            for (byte playerCount = 0; playerCount < numPlayers; playerCount++)
                playerColors.Add(packet.ReadInt());

            //Pickups
            byte numPickups = packet.ReadByte();
            for (byte pickupCount = 0; pickupCount < numPickups; pickupCount++)
                GeneratedPickupItem(packet);


            PlayerChooseColor.Instance.Activate();
            PlayerChooseColor.Instance.SetTakenColors(playerColors);
            PlayerChooseColor.Instance.SetDefaultColor();
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    internal static void ReadGameState(Packet packet)
    {
        try 
        {
            GameState currentGameState = (GameState)packet.ReadByte();
            float remainingGameTime = packet.ReadFloat();
            remainingGameTime -= Client.Instance.Latency1WaySecondsTCP;

            GameStateManager.ReadGameState(currentGameState, remainingGameTime);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void FreeColor(Packet packet)
    {
        try 
        { 
            int colorToFree = packet.ReadInt();

            PlayerChooseColor.Instance.FreeColor(colorToFree);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    internal static void TakeColor(Packet packet)
    {
        try
        {
            int colorToTake = packet.ReadInt();

            PlayerChooseColor.Instance.TakeColor(colorToTake);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    internal static void TriedTakingTakenColor(Packet packet)
    {
        try 
        { 
            int numColors = packet.ReadInt();
            List<int> takenColors = new List<int>(numColors);

            for (int playerCount = 0; playerCount < numColors; playerCount++)
                takenColors.Add(packet.ReadInt());

            PlayerChooseColor.Instance.SetTakenColors(takenColors);
            PlayerChooseColor.Instance.SetDefaultColor();
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    public static void PlayerDisconnect(Packet packet)
    {
        try 
        { 
            byte disconnectedPlayer = packet.ReadByte();
            GameManager.Instance.DisconnectPlayer(disconnectedPlayer);
            ScoreboardManager.Instance.DeleteEntry(disconnectedPlayer);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    public static void SpawnPlayer(Packet packet)
    {
        try 
        { 
            byte iD = packet.ReadByte();
            string username = packet.ReadString();
            Vector2 position = packet.ReadUVector2WorldPosition();
            bool isFacingRight = packet.ReadBool();

            float runSpeed = packet.ReadFloat();
            float sprintSpeed = packet.ReadFloat();

            bool isDead = packet.ReadBool();
            bool justJoined = packet.ReadBool();

            int kills = packet.ReadInt();
            int deaths = packet.ReadInt();
            int score = packet.ReadInt();

            int maxHealth = packet.ReadInt();
            int currentHealth = packet.ReadInt();

            int playerColor = packet.ReadInt();
            
            bool paused = packet.ReadBool();

            float invincibilityTime = packet.ReadFloat();

            ushort pingMS = packet.ReadUShort();

            GameManager.Instance.SpawnPlayer(iD, username, position, isFacingRight, isDead, justJoined, maxHealth, currentHealth, playerColor);
            GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
            ScoreboardManager.Instance.AddEntry(iD, username, kills, deaths, score, pingMS);

            GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);

            GameManager.PlayerDictionary[iD].AdrenalinePickup(invincibilityTime);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    public static void PlayerMovementStats(Packet packet)
    {
        try
        {
            byte iD = packet.ReadByte();
            float runSpeed = packet.ReadFloat();
            float sprintSpeed = packet.ReadFloat();

            GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    public static void BulletShot(Packet packet)
    {
        try
        {
            byte iD = packet.ReadByte();
            Vector2 position = packet.ReadUVector2WorldPosition();
            Quaternion rotation = packet.ReadQuaternion();
            GameManager.PlayerDictionary[iD].CallOnBulletShotEvent(position, rotation);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    public static void PlayerDied(Packet packet)
    {
        try 
        { 
            byte playerKilledID = packet.ReadByte();
            byte bulletOwnerID = packet.ReadByte();
            TypeOfDeath typeOfDeath = (TypeOfDeath)packet.ReadByte();
            KillFeedUI.Instance.AddKillFeedEntry(playerKilledID, bulletOwnerID);
            if (playerKilledID != bulletOwnerID)
                ScoreboardManager.Instance.AddKill(bulletOwnerID);
            ScoreboardManager.Instance.AddDeath(playerKilledID);
            GameManager.PlayerDictionary[playerKilledID].PlayerDied(typeOfDeath);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    public static void PlayerRespawned(Packet packet)
    {
        try 
        { 
            byte iD = packet.ReadByte();
            Vector2 respawnPoint = packet.ReadUVector2WorldPosition();
            GameManager.PlayerDictionary[iD].PlayerRespawned();
            GameManager.PlayerDictionary[iD].SetRespawnPosition(respawnPoint);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void TookDamage(Packet packet)
    {
        try
        {
            byte iD = packet.ReadByte();
            int damage = packet.ReadInt();
            int currentHealth = packet.ReadInt();
            short bulletOwner = packet.ReadShort();

            GameManager.PlayerDictionary[iD].TookDamage(damage, currentHealth);
            if (Client.Instance.ClientID == bulletOwner)
                GameManager.PlayerDictionary[(byte)bulletOwner].CallLocalPlayerHitAnother();
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void ServerIsFull(Packet _)
    {
        ServerMenu.ServerConnectionFull();
    }

    internal static void PlayerPausedGame(Packet packet)
    {
        try
        {
            byte iD = packet.ReadByte();
            bool paused = packet.ReadBool();
            GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void GeneratedPickupItem(Packet packet)
    {
        try
        {
            PickupType pickupType = (PickupType)packet.ReadByte();
            ushort pickupID = packet.ReadUShort();
            Vector2 position = packet.ReadUVector2WorldPosition();

            PickupItemsManager.Instance.SpawnGeneratedPickup(pickupType, pickupID, position);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    internal static void PlayerPickedUpItem(Packet packet)
    {
        try
        {
            ushort pickupID = packet.ReadUShort();
            byte clientWhoPickedUp = packet.ReadByte();

            PickupItemsManager.Instance.PlayerPickedUpItem(pickupID, clientWhoPickedUp, packet);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void ChatMessage(Packet packet)
    {
        try
        {
            string text = packet.ReadString();
            byte playerID = packet.ReadByte();

            InGameChat.Instance.AddInGameChatEntry(text, GameManager.PlayerDictionary[playerID].PlayerColor);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    #endregion

    #region ConstantlySentPackets

    internal static void PlayerStillConnectedTCP(Packet packet)
    {
        try
        {
            byte latencyIDTCP = packet.ReadByte();
            ushort latency2WayMSTCP = packet.ReadUShort();

            Client.Instance.PlayerConnectedAcknAndPingTCP(DateTime.Now.TimeOfDay, latency2WayMSTCP);
            ClientSend.PingPacketAckTCP(latencyIDTCP);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }
    internal static void PlayerStillConnectedUDP(Packet packet)
    {
        try
        {
            byte latencyIDUDP = packet.ReadByte();
            ushort latency2WayMSUDP = packet.ReadUShort();

            Client.Instance.PlayerConnectedAcknAndPingUDP(DateTime.Now.TimeOfDay, latency2WayMSUDP);
            ClientSend.PingPacketAckUDP(latencyIDUDP);
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    internal static void ConstantPlayerData(Packet packet)
    {
        try
        {
            byte numPlayers = packet.ReadByte();

            for (int playerCount = 0; playerCount < numPlayers; playerCount++)
            {
                byte playerID = packet.ReadByte();

                bool[] bits = packet.Read1ByteAs8Bools();

                PlayerManager player = GameManager.PlayerDictionary[playerID];

                if (bits[0])
                {
                    Quaternion armRotation = packet.ReadQuaternion();
                    if (player.ID != Client.Instance.ClientID)
                        player.SetArmRotation(armRotation);
                }

                if (bits[1])
                {
                    Vector2 armPosition = packet.ReadLocalVector2();
                    if (player.ID != Client.Instance.ClientID)
                        player.SetArmPosition(armPosition);
                }

                if (bits[2])
                {
                    Vector2 playerPosition = packet.ReadUVector2WorldPosition();
                    if (player.ID != Client.Instance.ClientID)
                        player.SetPosition(playerPosition);
                }

                if (bits[3])
                {
                    byte moveState = packet.ReadByte();
                    if (player.ID != Client.Instance.ClientID)
                        player.SetVelocityState((PlayerMovingState)moveState);
                }

                if (bits[4])
                {
                    ushort pingMS = packet.ReadUShort();
                    ScoreboardManager.Instance.SetPing(playerID, pingMS);
                    //if (player.ID != Client.Instance.ClientID)
                    //
                }
            }
        }
        catch (Exception e) {
            OutputPacketError(e);
        }
    }

    #endregion

    private static void OutputPacketError(Exception e) =>
        Output.WriteLine($"\tError, reading packet...\n{e}");
}
