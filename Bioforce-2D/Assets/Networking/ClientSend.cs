using System;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Output;

public enum SendConstantPacketsState
{
    UDP,
    UDPandTCP,
    TCP
}
public class ClientSend : MonoBehaviour
{
    public static SendConstantPacketsState SendConstantPacketsState { get; set; } = SendConstantPacketsState.UDPandTCP;

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



    // Constantly sent
    // TCP 7B (byte 1B packetLen + byte 1B packetID + Vector2 4B position + PlayerMovingState 1B movingState)
    // UDP 8B (byte 1B clientID + byte 1B packetLen + byte 1B packetID + Vector2 4B position + PlayerMovingState 1B movingState)
    public static void PlayerMovement(Vector2 position, PlayerMovingState movingState)
    {
        using (Packet packet = new Packet((byte)ClientPackets.playerMovement))
        {
            packet.WriteWorldUVector2(position);
            packet.Write((byte)movingState);

            SendConstantlySentPacket(packet);
        }
    }

    // Constantly sent
    // TCP 7B (byte 1B PacketLen + byte 1B packetID + Vector2 2B position + 3B Quaternion rotation)
    // UDP 8B (byte 1B clientID + byte 1B PacketLen + byte 1B packetID + Vector2 2B position + 3B Quaternion rotation)
    internal static void ArmPositionAndRotation(Vector2 localPosition, Quaternion localRotation)
    {
        using (Packet packet = new Packet((byte)ClientPackets.armPositionRotation))
        {
            packet.WriteLocalPosition(localPosition);
            packet.Write(localRotation);

            SendConstantlySentPacket(packet);
        }
    }


    // Constantly sent
    // 3B (byte 1B packetLen + byte 1B packetID + byte 1B latencyID)
    internal static void PlayerConnectedTCPPacket(byte latencyID)
    {
        using (Packet packet = new Packet((byte)ClientPackets.stillConnectedTCP))
        {
            packet.Write(latencyID);

            SendTCPPacket(packet);
        }
    }
    // Constantly sent
    // 4B (byte 1B clientID + byte 1B packetLen + byte 1B packetID + byte 1B latencyID)
    internal static void PlayerConnectedUDPPacket(byte latencyID)
    {
        using (Packet packet = new Packet((byte)ClientPackets.stillConnectedUDP))
        {
            packet.Write(latencyID);

            SendUDPPacket(packet);
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
    
    // 9B (byte 1B packetLen + byte 1B packetID + Vector2 4B position + 3B Quaternion rotation)
    public static void ShotBullet(Vector2 position, Quaternion rotation)
    {
        using (Packet packet = new Packet((byte)ClientPackets.bulletShot))
        {
            packet.WriteWorldUVector2(position);
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
            packet.WriteWorldUVector2(respawnPosition);

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

    internal static void ChatMessage(string text)
    {
        using (Packet packet = new Packet((byte)ClientPackets.chatMessage))
        {
            packet.Write(text);

            SendTCPPacket(packet);
        }
    }

    private static void SendConstantlySentPacket(Packet packet)
    {
        switch (SendConstantPacketsState)
        {
            case SendConstantPacketsState.UDP:
                SendUDPPacket(packet);
                break;
            case SendConstantPacketsState.UDPandTCP:
                using (Packet samePacket = new Packet(packet.ToArray()))
                {
                    SendTCPPacket(packet);
                    SendUDPPacket(samePacket);
                }
                break;
            case SendConstantPacketsState.TCP:
                SendTCPPacket(packet);
                break;
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
            Output.WriteLine($"Error, sending TCP Packet...\n{exception}");
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
            Output.WriteLine($"Error, sending UDP Packet...\n{exception}");
        }
    }
}
