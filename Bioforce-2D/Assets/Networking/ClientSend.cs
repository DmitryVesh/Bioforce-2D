using System;
using UnityEngine;
using UnityEngine.Output;
using Shared;

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

    // TCP extra 40B = TCP Header 20B + IPv4 Header 20B
    // UDP extra 28B = UDP Header 8B + IPv4 Header 20B
    //TODO: Make all the constantly sent packets to be compressed into 1
    //      Add a byte which contains 1s for which states changed or not, e.g. 
    //      position, movingState, localPosition, localRotation
    //      0b_0000_xyzw       where x=1 if movingState is sent, y=1 if position is sent...
    //TODO: perhaps set the Nagle Algorithm to false on TCP, Socket.NoDelay = true;
    //


    // TCP max 13B + 40B = 53B  (byte 1B packetLen + byte 1B packetID (+ UDP byte 1B clientID)
    // UDP max 14B + 28B = 42B  + byte 1B whatShouldSend/Read
    //                          + byte 1B MoveState + worldVector2 4B playerPosition
    //                          + localVector2 2B armPosition + localRotation 3B armRotation
    internal static void ConstantPlayerData(
        bool shouldSendMoveState, PlayerMovingState moveState,
        bool shouldSendWorldPosition, Vector2 playerPosition,
        bool shouldSendArmPosition, Vector2 armPosition,
        bool shouldSendArmRotation, Quaternion armRotation)
    {
        using (Packet packet = new Packet((byte)ClientPackets.constantPlayerData))
        {
            packet.Write8BoolsAs1Byte(false, false, false, false,
                shouldSendMoveState, shouldSendWorldPosition, shouldSendArmPosition, shouldSendArmRotation);

            if (shouldSendArmRotation)
                packet.Write(armRotation);

            if (shouldSendArmPosition)
                packet.WriteLocalPosition(armPosition);

            if (shouldSendWorldPosition)
                packet.WriteWorldUVector2(playerPosition);

            if (shouldSendMoveState)
                packet.Write((byte)moveState);

            SendConstantlySentPacket(packet);
        }
    }

    // Constantly sent
    // TCP 2B + 40B = 42B (byte 1B packetLen + byte 1B packetID + byte 1B latencyID)
    internal static void PlayerConnectedTCPPacket()
    {
        using (Packet packet = new Packet((byte)ClientPackets.stillConnectedTCP))
        {
            SendTCPPacket(packet);
        }
    }
    // Constantly sent
    // UDP 3B + 28B = 31B (byte 1B clientID + byte 1B packetLen + byte 1B packetID)
    internal static void PlayerConnectedUDPPacket()
    {
        using (Packet packet = new Packet((byte)ClientPackets.stillConnectedUDP))
        {
            SendUDPPacket(packet);
        }
    }

    internal static void PingPacketAckTCP(byte latencyIDTCP)
    {
        using (Packet packet = new Packet((byte)ClientPackets.pingAckTCP))
        {
            packet.Write(latencyIDTCP);

            SendTCPPacket(packet);
        }
    }
    internal static void PingPacketAckUDP(byte latencyIDUDP)
    {
        using (Packet packet = new Packet((byte)ClientPackets.pingAckUDP))
        {
            packet.Write(latencyIDUDP);

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

    // maxB 102B = (byte 1B packetLen, byte 1B packetID, string numChars * B -> max 100B text)
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
