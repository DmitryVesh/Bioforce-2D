using System;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    public static void WelcomePacketReply()
    {
        using (Packet packet = new Packet((byte)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.Instance.ClientID);
            packet.Write(PlayerRegistration.GetUsername());

            SendTCPPacket(packet);
        }        
    }
    public static void UDPTestPacketReply()
    {
        using (Packet packet = new Packet((byte)ClientPackets.udpTestReceived))
        {
            packet.Write(Client.Instance.ClientID);
            packet.Write("Received UDP Test Packet");

            SendUDPPacket(packet);
        }        
    }

    internal static void ColorToFreeAndToTaken(int colorToFree, int colorToTake)
    {
        using (Packet packet = new Packet((byte)ClientPackets.colorToFreeAndTake))
        {
            packet.Write(colorToFree);
            packet.Write(colorToTake);

            SendTCPPacket(packet);
        }
    }
    internal static void PlayerReadyToJoin(int chosenColorIndex)
    {
        using (Packet packet = new Packet((byte)ClientPackets.readyToJoin))
        {
            packet.Write(chosenColorIndex);

            SendTCPPacket(packet);
        }
    }

    public static void PlayerMovement(Vector2 position, Vector2 velocity)
    {
        using (Packet packet = new Packet((byte)ClientPackets.playerMovement))
        {
            packet.Write(position);
            packet.Write(velocity);

            SendTCPPacket(packet);
        }        
    }

    public static void PlayerMovementStats(float runSpeed, float sprintSpeed)
    {
        using (Packet packet = new Packet((byte)ClientPackets.playerMovementStats))
        {
            packet.Write(runSpeed);
            packet.Write(sprintSpeed);

            SendTCPPacket(packet); // Only sending once, so want to make sure it gets there
        }        
    }

    

    internal static void ArmPositionAndRotation(Vector2 localPosition, Quaternion localRotation)
    {
        using (Packet packet = new Packet((byte)ClientPackets.armPositionRotation))
        {
            packet.Write(localPosition);
            packet.Write(localRotation);

            SendTCPPacket(packet);
        }
    }

    public static void ShotBullet(Vector2 position, Quaternion rotation)
    {
        using (Packet packet = new Packet((byte)ClientPackets.bulletShot))
        {
            packet.Write(position);
            packet.Write(rotation);

            SendTCPPacket(packet);
        }
    }
    public static void PlayerDied(byte bulletOwnerID, TypeOfDeath typeOfDeath)
    {
        using (Packet packet = new Packet((byte)ClientPackets.playerDied))
        {
            packet.Write(bulletOwnerID);
            packet.Write((byte)typeOfDeath);

            SendTCPPacket(packet);
        }       
    }
    public static void PlayerRespawned(Vector2 respawnPosition)
    {
        using (Packet packet = new Packet((byte)ClientPackets.playerRespawned))
        {
            packet.Write(respawnPosition);

            SendTCPPacket(packet);
        }        
    }
    public static void TookDamage(int damage, int currentHealth, short bulletOwner)
    {
        using (Packet packet = new Packet((byte)ClientPackets.tookDamage))
        {
            packet.Write(damage);
            packet.Write(currentHealth);
            packet.Write(bulletOwner);

            SendTCPPacket(packet);
        }        
    }

    internal static void PausedGame(bool paused)
    {
        using (Packet packet = new Packet((byte)ClientPackets.pausedGame))
        {
            packet.Write(paused);
            SendTCPPacket(packet);
        }
    }
    internal static void PlayerConnectedPacket()
    {
        using (Packet packet = new Packet((byte)ClientPackets.stillConnected))
        {
            SendTCPPacket(packet);
        }
    }

    internal static void LocalPlayerPickedUpItem(int pickupID)
    {
        using (Packet packet = new Packet((byte)ClientPackets.pickedUpItem))
        {
            packet.Write(pickupID);

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
