using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LANDiscoveryClient : DiscoveryTCPClient
{
    protected override void InitPacketHandlerDictionary()
    {        
        PacketHandlerDictionary.Add((int)LANDiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);        
    }
}
