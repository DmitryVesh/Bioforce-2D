using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientRead : MonoBehaviour
{
    public static void WelcomeRead(Packet packet)
    {
        //PacketSender.Welcome(count, $"Welcome to the server client {count}");
        string message = packet.ReadString();
        int id = packet.ReadInt();
        string mapName = packet.ReadString();
        Debug.Log($"Message from server:\n{message}");
        Client.Instance.SuccessfullyConnected(id);
        Client.Instance.uDP.Connect(((IPEndPoint)Client.Instance.tCP.Socket.Client.LocalEndPoint).Port);

        GameManager.Instance.SwitchScene(mapName);

        ClientSend.WelcomePacketReply();

        GameManager.Instance.InGame = true;
    }
    public static void UDPTestRead(Packet packet)
    {
        Debug.Log($"Received packet via UDP: {packet.ReadString()}");
        ClientSend.UDPTestPacketReply();
    }
    public static void PlayerDisconnect(Packet packet)
    {
        int disconnectedPlayer = packet.ReadInt();
        GameManager.Instance.DisconnectPlayer(disconnectedPlayer);
        ScoreboardManager.Instance.DeleteEntry(disconnectedPlayer);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int iD = packet.ReadInt();
        string username = packet.ReadString();
        Vector2 position = packet.ReadVector2();
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

        int R = packet.ReadInt();
        int G = packet.ReadInt();
        int B = packet.ReadInt();
        Color color = new Color((float)R / (float)255, (float)G / (float)255, (float)B / (float)255);
        bool paused = packet.ReadBool();

        GameManager.Instance.SpawnPlayer(iD, username, position, isFacingRight, isDead, justJoined, maxHealth, currentHealth, color);
        GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
        ScoreboardManager.Instance.AddEntry(iD, username, kills, deaths, score);

        GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);
    }
    public static void PlayerMovementStats(Packet packet)
    {
        int iD = packet.ReadInt();
        float runSpeed = packet.ReadFloat();
        float sprintSpeed = packet.ReadFloat();

        GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
    }

    public static void PlayerPosition(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector2 position = packet.ReadVector2();
        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].SetPosition(position);
        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Player iD PlayerPosition: {iD}\n {exception}");
        }
    }
    public static void PlayerRotationAndVelocity(Packet packet)
    {
        int iD = packet.ReadInt();
        bool isFacingRight = packet.ReadBool();
        Vector2 velocity = packet.ReadVector2();
        Quaternion rotation = packet.ReadQuaternion();

        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].SetVelocity(velocity);
            //GameManager.PlayerDictionary[iD].SetRotation(rotation);
        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Player iD PlayerRotation: {iD}\n {exception}");
        }
    }
    public static void BulletShot(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector2 position = packet.ReadVector2();
        Quaternion rotation = packet.ReadQuaternion();

        try
        {
            GameManager.PlayerDictionary[iD].CallOnBulletShotEvent(position, rotation);
        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Error, in bullet shot in player iD: {iD}\n{exception}");
        }
    }
    public static void PlayerDied(Packet packet)
    {
        int playerKilledID = packet.ReadInt();
        int bulletOwnerID = packet.ReadInt();
        TypeOfDeath typeOfDeath = (TypeOfDeath)packet.ReadInt();
        KillFeedUI.Instance.AddKillFeedEntry(playerKilledID, bulletOwnerID);
        if (playerKilledID != bulletOwnerID)
            ScoreboardManager.Instance.AddKill(bulletOwnerID);
        ScoreboardManager.Instance.AddDeath(playerKilledID);
        GameManager.PlayerDictionary[playerKilledID].PlayerDied(typeOfDeath);
    }
    public static void PlayerRespawned(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector2 respawnPoint = packet.ReadVector2();
        GameManager.PlayerDictionary[iD].PlayerRespawned();
        GameManager.PlayerDictionary[iD].SetRespawnPosition(respawnPoint);
    }

    internal static void TookDamage(Packet packet)
    {
        int iD = packet.ReadInt();
        int damage = packet.ReadInt();
        int currentHealth = packet.ReadInt();

        GameManager.PlayerDictionary[iD].TookDamage(damage, currentHealth);
    }

    internal static void ServerIsFull(Packet packet)
    {
        ServerMenu.ServerConnectionFull();
    }

    internal static void ArmPositionRotation(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector2 position = packet.ReadVector2();
        Quaternion rotation = packet.ReadQuaternion();
        try
        {
            GameManager.PlayerDictionary[iD].SetArmPositionRotation(position, rotation);
        }
        catch (Exception exception)
        {
            Debug.Log($"Player's: {iD} ArmPositionRotation caused an error.\n{exception}");
        }
    }

    internal static void PlayerPausedGame(Packet packet)
    {
        int iD = packet.ReadInt();
        bool paused = packet.ReadBool();
        try
        {
            GameManager.PlayerDictionary[iD].SetPlayerPaused(paused);
        }
        catch (Exception exception)
        {
            Debug.Log($"Player's: {iD} PlayerPausedGame caused an error.\n{exception}");
        }
    }

    internal static void PlayerStillConnected(Packet packet)
    {
        Client.Instance.PlayerConnectedAckn(DateTime.Now.TimeOfDay);
    }

    internal static void SetHostClient(Packet packet)
    {
        bool shouldHost = packet.ReadBool();
        HostClient.Instance.Host(shouldHost);
    }
}
