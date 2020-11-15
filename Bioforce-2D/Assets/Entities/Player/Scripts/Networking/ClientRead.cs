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

        Debug.Log($"Message from server: {message}");
        Client.Instance.ClientID = id;

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

        GameManager.Instance.SpawnPlayer(iD, username, position, rotation);
    }
    public static void PlayerPosition(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        //Debug.Log($"Received player position packet from player: {iD}\nPosition sent {position}");
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
    public static void PlayerVelocity(Packet packet)
    {
        int iD = packet.ReadInt();
        Vector3 velocity = packet.ReadVector3();
        
        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].AddVelocity(velocity);
        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Player iD PlayerVelocity: {iD}\n {exception}");
        }
    }
    public static void PlayerRotation(Packet packet)
    {
        int iD = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        // Prevents crash when a UDP packet connects before the TCP spawn player call from server
        try
        {
            GameManager.PlayerDictionary[iD].transform.rotation = rotation;
        }
        catch (KeyNotFoundException exception)
        {
            Debug.Log($"Player iD PlayerRotation: {iD}\n {exception}");
        }
    }
}
