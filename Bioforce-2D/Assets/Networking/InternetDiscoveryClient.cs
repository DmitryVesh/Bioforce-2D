
public class InternetDiscoveryClient : DiscoveryTCPClient
{
    protected override void InitPacketHandlerDictionary()
    {
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.welcome, ServerMenu.ReadWelcomePacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverDeleted, ServerMenu.ReadServerDeletedPacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.serverModified, ServerMenu.ReadServerModifiedPacket);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.cantJoinServerDeleted, ServerMenu.ReadCantJoinServerDeleted);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.noMoreServersAvailable, ServerMenu.ReadNoMoreServersAvailable);
        PacketHandlerDictionary.Add((int)InternetDiscoveryServerPackets.joinServer, ServerMenu.ReadJoinServer);
    }
}
