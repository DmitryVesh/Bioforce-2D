using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Output;

public class ClientRead : MonoBehaviour
{
    public static void WelcomeRead(Packet packet)
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
    public static void UDPTestRead(Packet packet)
    {
        Output.WriteLine($"Received packet via UDP: {packet.ReadString()}");
        ClientSend.UDPTestPacketReply();
    }

    internal static void AskingForPlayerDetails(Packet packet)
    {
        Output.WriteLine("Have Received AskingForPlayerDetails packet");
        byte numPlayers = packet.ReadByte();

        List<int> playerColors = new List<int>();

        for (byte playerCount = 0; playerCount < numPlayers; playerCount++)
            playerColors.Add(packet.ReadInt());

        byte numPickups = packet.ReadByte();
        for (byte pickupCount = 0; pickupCount < numPickups; pickupCount++)
            GeneratedPickupItem(packet);

        PlayerChooseColor.Instance.SetActivate(true);
        PlayerChooseColor.Instance.SetTakenColors(playerColors);
        PlayerChooseColor.Instance.SetDefaultColor();
    }

    internal static void FreeColor(Packet packet)
    {
        int colorToFree = packet.ReadInt();

        PlayerChooseColor.Instance.FreeColor(colorToFree);
    }
    internal static void TakeColor(Packet packet)
    {
        int colorToTake = packet.ReadInt();

        PlayerChooseColor.Instance.TakeColor(colorToTake);
    }
    internal static void TriedTakingTakenColor(Packet packet)
    {
        int numColors = packet.ReadInt();
        List<int> takenColors = new List<int>(numColors);

        for (int playerCount = 0; playerCount < numColors; playerCount++)
            takenColors.Add(packet.ReadInt());

        PlayerChooseColor.Instance.SetTakenColors(takenColors);
        PlayerChooseColor.Instance.SetDefaultColor();
    }

    public static void PlayerDisconnect(Packet packet)
    {
        byte disconnectedPlayer = packet.ReadByte();
        GameManager.Instance.DisconnectPlayer(disconnectedPlayer);
        ScoreboardManager.Instance.DeleteEntry(disconnectedPlayer);
    }

    public static void SpawnPlayer(Packet packet)
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

        GameManager.Instance.SpawnPlayer(iD, username, position, isFacingRight, isDead, justJoined, maxHealth, currentHealth, playerColor);
        GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
        ScoreboardManager.Instance.AddEntry(iD, username, kills, deaths, score);

        GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);
    }
    public static void PlayerMovementStats(Packet packet)
    {
        byte iD = packet.ReadByte();
        float runSpeed = packet.ReadFloat();
        float sprintSpeed = packet.ReadFloat();

        GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
    }

    public static void PlayerPosition(Packet packet)
    {
        byte iD = packet.ReadByte();
        Vector2 position = packet.ReadUVector2WorldPosition();
        PlayerMovingState movingState = (PlayerMovingState)packet.ReadByte();

        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].SetVelocityState(movingState);
            GameManager.PlayerDictionary[iD].SetPosition(position);
        }
        catch (KeyNotFoundException exception)
        {
            Output.WriteLine($"Player iD PlayerPosition: {iD}\n {exception}");
        }
    }

    public static void BulletShot(Packet packet)
    {
        byte iD = packet.ReadByte();
        Vector2 position = packet.ReadUVector2WorldPosition();
        Quaternion rotation = packet.ReadQuaternion();

        try
        {
            GameManager.PlayerDictionary[iD].CallOnBulletShotEvent(position, rotation);
        }
        catch (KeyNotFoundException exception)
        {
            Output.WriteLine($"Error, in bullet shot in player iD: {iD}\n{exception}");
        }
    }
    public static void PlayerDied(Packet packet)
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
    public static void PlayerRespawned(Packet packet)
    {
        byte iD = packet.ReadByte();
        Vector2 respawnPoint = packet.ReadUVector2WorldPosition();
        GameManager.PlayerDictionary[iD].PlayerRespawned();
        GameManager.PlayerDictionary[iD].SetRespawnPosition(respawnPoint);
    }

    internal static void TookDamage(Packet packet)
    {
        byte iD = packet.ReadByte();
        int damage = packet.ReadInt();
        int currentHealth = packet.ReadInt();
        short bulletOwner = packet.ReadShort();

        GameManager.PlayerDictionary[iD].TookDamage(damage, currentHealth);
        if (Client.Instance.ClientID == bulletOwner)
            GameManager.PlayerDictionary[(byte)bulletOwner].CallLocalPlayerHitAnother();
    }

    internal static void ServerIsFull(Packet _)
    {
        ServerMenu.ServerConnectionFull();
    }

    internal static void ArmPositionRotation(Packet packet)
    {
        byte iD = packet.ReadByte();
        Vector2 localPosition = packet.ReadLocalVector2();
        Quaternion localRotation = packet.ReadQuaternion();
        try
        {
            GameManager.PlayerDictionary[iD].SetArmPositionRotation(localPosition, localRotation);
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Player's: {iD} ArmPositionRotation caused an error.\n{exception}");
        }
    }

    internal static void PlayerPausedGame(Packet packet)
    {
        byte iD = packet.ReadByte();
        bool paused = packet.ReadBool();
        try
        {
            GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Player's: {iD} PlayerPausedGame caused an error.\n{exception}");
        }
    }

    internal static void PlayerStillConnectedTCP(Packet packet)
    {
        try
        {
            byte latencyID = packet.ReadByte();
            Client.Instance.PlayerConnectedAcknTCP(DateTime.Now.TimeOfDay, latencyID);
        }
        catch (Exception e)
        {
            Output.WriteLine($"PlayerStillConnectedTCP caused an error.\n{e}");
        }
    }
    internal static void PlayerStillConnectedUDP(Packet packet)
    {
        try
        {
            byte latencyID = packet.ReadByte();
            Client.Instance.PlayerConnectedAcknUDP(DateTime.Now.TimeOfDay, latencyID);
        }
        catch (Exception e)
        {
            Output.WriteLine($"PlayerStillConnectedUDP caused an error.\n{e}");
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
        catch (Exception e)
        {
            Output.WriteLine($"Error in reading GeneratedPickupItem...\n{e}");
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
        catch (Exception e)
        {
            Output.WriteLine($"Error in reading PlayerPickedUpItem...\n{e}");
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
        catch (Exception e)
        {
            Output.WriteLine($"Error in reading ChatMessage...\n{e}");
        }
    }
}
