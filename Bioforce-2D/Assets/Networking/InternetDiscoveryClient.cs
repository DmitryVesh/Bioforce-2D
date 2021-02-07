using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetDiscoveryClient : DiscoveryTCPClient
{
    protected override void InitPacketHandlerDictionary()
    {
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.welcome, ServerMenu.ReadWelcomePacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverDeleted, ServerMenu.ReadServerDeletedPacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverModified, ServerMenu.ReadServerModifiedPacket);
    }
}
