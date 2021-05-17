using Shared;

public class LANDiscoveryClient : DiscoveryTCPClient
{
    protected override void InitPacketHandlerDictionary()
    {        
        PacketHandlerDictionary.Add((int)LANDiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);        
    }
}
