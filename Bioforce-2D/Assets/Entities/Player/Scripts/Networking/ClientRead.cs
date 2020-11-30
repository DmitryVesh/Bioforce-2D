using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientRead : MonoBehaviour
{
    public static void WelcomeRead(Packet packet)
    {
        //PacketSender.Welcome(count, $"Welcome to the server client {count}");
        string message = packet.ReadString();
        int id = packet.ReadInt();

        Debug.Log($"Message from server:\n{message}");
        Client.Instance.SuccessfullyConnected(id);

        ClientSend.WelcomePacketReply();

        Client.Instance.uDP.Connect(((IPEndPoint)Client.Instance.tCP.Socket.Client.LocalEndPoint).Port);
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
    }

    public static void SpawnPlayer(Packet packet)
    {
        int iD = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        float runSpeed = packet.ReadFloat();
        float sprintSpeed = packet.ReadFloat();

        bool isDead = packet.ReadBool();

        GameManager.Instance.SpawnPlayer(iD, username, position, rotation, isDead);
        GameManager.PlayerDictionary[iD].SetPlayerMovementStats(runSpeed, sprintSpeed);
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
        Vector3 position = packet.ReadVector3();

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
        Quaternion rotation = packet.ReadQuaternion();
        Vector2 velocity = packet.ReadVector2();

        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].SetRotation(rotation);
            GameManager.PlayerDictionary[iD].SetVelocity(velocity);
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
            GameManager.PlayerDictionary[iD].ShotBullet(position, rotation);
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
        KillFeedUI.Instance.AddKillFeedEntry(playerKilledID, bulletOwnerID);
        GameManager.PlayerDictionary[playerKilledID].PlayerDied();
    }
    public static void PlayerRespawned(Packet packet)
    {
        int iD = packet.ReadInt();
        GameManager.PlayerDictionary[iD].PlayerRespawned();
    }
}
