using System;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void WelcomePacketReply()
    {
        Packet packet = new Packet((int)ClientPackets.welcomeReceived);
        packet.Write(Client.Instance.ClientID);
        packet.Write(PlayerRegistration.GetUsername());
        SendTCPPacket(packet);
    }
    public static void UDPTestPacketReply()
    {
        Packet packet = new Packet((int)ClientPackets.udpTestReceived);
        packet.Write(Client.Instance.ClientID);
        packet.Write("Received UDP Test Packet");
        SendUDPPacket(packet);
    }

    public static void PlayerMovement(bool isFacingRight, Vector2 position, Vector2 velocity, Quaternion rotation)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(isFacingRight);
            packet.Write(position);
            packet.Write(velocity);
            packet.Write(rotation);

            SendTCPPacket(packet);
        }        
    }
    public static void PlayerMovementStats(float runSpeed, float sprintSpeed)
    {
        Packet packet = new Packet((int)ClientPackets.playerMovementStats);
        packet.Write(runSpeed);
        packet.Write(sprintSpeed);
        SendTCPPacket(packet); // Only sending once, so want to make sure it gets there
    }

    internal static void ArmPositionAndRotation(Vector2 localPosition, Quaternion localRotation)
    {
        using (Packet packet = new Packet((int)ClientPackets.armPositionRotation))
        {
            packet.Write(localPosition);
            packet.Write(localRotation);
            SendTCPPacket(packet);
        }
    }

    public static void ShotBullet(Vector2 position, Quaternion rotation)
    {
        Packet packet = new Packet((int)ClientPackets.bulletShot);
        packet.Write(position);
        packet.Write(rotation);

        SendTCPPacket(packet);
    }
    public static void PlayerDied(int bulletOwnerID, TypeOfDeath typeOfDeath)
    {
        Packet packet = new Packet((int)ClientPackets.playerDied);
        packet.Write(bulletOwnerID);
        packet.Write((int)typeOfDeath);

        SendTCPPacket(packet);
    }
    public static void PlayerRespawned(Vector2 respawnPosition)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerRespawned))
        {
            packet.Write(respawnPosition);

            SendTCPPacket(packet);
        }        
    }
    public static void TookDamage(int damage, int currentHealth)
    {
        Packet packet = new Packet((int)ClientPackets.tookDamage);
        packet.Write(damage);
        packet.Write(currentHealth);

        SendTCPPacket(packet);
    }

    internal static void PausedGame(bool paused)
    {
        using (Packet packet = new Packet((int)ClientPackets.pausedGame))
        {
            packet.Write(paused);

            SendTCPPacket(packet);
        }
    }
    internal static void PlayerConnectedPacket()
    {
        using (Packet packet = new Packet((int)ClientPackets.stillConnnected))
        {
            SendTCPPacket(packet);
        }
    }


    private static void SendTCPPacket(Packet packet)
    {
        try
        {
            packet.WriteLength();
            Client.Instance.tCP.SendPacket(packet);
        }
        catch (Exception exception)
        {
            Debug.Log($"Error, sending TCP Packet...\n{exception}");
        }
    }
    private static void SendUDPPacket(Packet packet)
    {
        try
        {
            packet.WriteLength();
            Client.Instance.uDP.SendPacket(packet);
        }
        catch (Exception exception)
        {
            Debug.Log($"Error, sending UDP Packet...\n{exception}");
        }
    }

    
}
