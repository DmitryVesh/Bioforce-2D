using Shared;

public class InternetDiscoveryClient : DiscoveryTCPClient
{
    protected override void InitPacketHandlerDictionary()
    {
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.welcome, ServerMenu.ReadWelcomePacket);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.serverDeleted, ServerMenu.ReadServerDeletedPacket);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.serverModified, ServerMenu.ReadServerModifiedPacket);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.cantJoinServerDeleted, ServerMenu.ReadCantJoinServerDeleted);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.noMoreServersAvailable, ServerMenu.ReadNoMoreServersAvailable);
        PacketHandlerDictionary.Add((byte)InternetDiscoveryServerPackets.joinServer, ServerMenu.ReadJoinServer);
    }
}
