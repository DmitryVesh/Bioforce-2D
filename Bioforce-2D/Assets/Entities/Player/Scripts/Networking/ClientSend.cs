using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void WelcomePacketReply()
    {
        Packet packet = new Packet((int)ClientPackets.welcomeReceived);
        packet.Write(Client.Instance.ClientID);
        packet.Write(SimpleNetworkingUI.Instance.GetUsername());
        SendTCPPacket(packet);
    }
    public static void UDPTestPacketReply()
    {
        Packet packet = new Packet((int)ClientPackets.udpTestReceived);
        packet.Write(Client.Instance.ClientID);
        packet.Write("Received UDP Test Packet");
        SendUDPPacket(packet);
    }
    public static void PlayerMovement(Quaternion rotation, Vector3 position, Vector2 velocity)
    {
        Packet packet = new Packet((int)ClientPackets.playerMovement);
        packet.Write(rotation);
        packet.Write(position);
        packet.Write(velocity);
        SendUDPPacket(packet);
    }
    public static void PlayerMovementStats(float runSpeed, float sprintSpeed)
    {
        Packet packet = new Packet((int)ClientPackets.playerMovementStats);
        packet.Write(runSpeed);
        packet.Write(sprintSpeed);
        SendTCPPacket(packet); // Only sending once, so want to make sure it gets there
    }

    public static void PlayerAnimation(float runSpeed, float speedX, bool grounded, bool jumped)
    {
        //throw new NotImplementedException("Player Animation in Client Send Not set up");
    }

    private static void SendTCPPacket(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.tCP.SendPacket(packet);
    }
    private static void SendUDPPacket(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.uDP.SendPacket(packet);
    }

    
}
