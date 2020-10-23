
namespace GameServer
{
    class PacketSender
    {
        public static void Welcome(int recipientClient, string message)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(message);
                packet.Write(recipientClient);

                SendData(recipientClient, packet);
            }
        }

        private static void SendData(int RecipientClient, Packet packet)
        {
            packet.WriteLength();
            Server.ClientDictionary[RecipientClient].tCP.SendData(packet);
        }
        private static void SendDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                Server.ClientDictionary[count].tCP.SendData(packet);
            }
        }
        private static void SendDataToAllButIncluded(int[] NonRecipientClients, Packet packet)
        {
            packet.WriteLength();
            for (int count = 1; count < Server.MaxNumPlayers + 1; count++)
            {
                Server.ClientDictionary[count].tCP.SendData(packet);
            }
        }
    }
}
