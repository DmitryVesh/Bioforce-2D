using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        public int ID { get; private set; }
        public TCP tCP { get; private set; }
        public UDP uDP { get; private set; }
        private static int DataBufferSize { get; set; } = 4096;

        public Player player { get; private set; }

        public Client(int iD)
        {
            ID = iD;
            tCP = new TCP(iD);
            uDP = new UDP(iD);
        }

        public class TCP
        {
            public TcpClient Socket { get; private set; }
            private int ID { get; }
            private byte[] ReceiveBuffer;
            private NetworkStream Stream;
            private Packet ReceivePacket;

            public TCP(int iD) => ID = iD;
            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                Stream = Socket.GetStream();
                ReceiveBuffer = new byte[DataBufferSize];
                ReceivePacket = new Packet();

                StreamBeginRead();
            }
            public void SendPacket(Packet packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception exception)
                {
                    //Disconnect();
                    Console.WriteLine($"\nError, occured when sending TCP data from client {ID}\nError{exception}");
                }
            }
            public void Disconnect()
            {
                Socket.Close();
                Socket = null;
                Stream = null;
                ReceiveBuffer = null;
                ReceivePacket = null;
            }

            private void BeginReadReceiveCallback(IAsyncResult asyncResult)
            {
                try
                {
                    int byteLen = Stream.EndRead(asyncResult);
                    if (byteLen <= 0)
                    {
                        Server.ClientDictionary[ID].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLen];
                    Array.Copy(ReceiveBuffer, data, byteLen);

                    ReceivePacket.Reset(HandleData(data));
                    StreamBeginRead();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\nError in BeginReadReceiveCallback of client {ID}...\nError: {exception}");
                    Server.ClientDictionary[ID].Disconnect();
                }
            }
            //TODO: Change so not copying and pasting same thing inheret from same class 
            private bool HandleData(byte[] data)
            {
                int packetLen = 0;
                ReceivePacket.SetBytes(data);

                if (ReceivePacket.UnreadLength() >= 4)
                {
                    packetLen = ReceivePacket.ReadInt();
                    if (packetLen < 1)
                    {
                        return true;
                    }
                }

                while (packetLen > 0 && packetLen <= ReceivePacket.UnreadLength())
                {
                    byte[] bytes = ReceivePacket.ReadBytes(packetLen);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(bytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.PacketHandlerDictionary[packetId](ID, packet);
                        }
                    });
                    packetLen = 0;

                    if (ReceivePacket.UnreadLength() >= 4)
                    {
                        packetLen = ReceivePacket.ReadInt();
                        if (packetLen < 1)
                        {
                            return true;
                        }
                    }
                }
                if (packetLen < 2)
                {
                    return true;
                }

                return false;
            }
            private void StreamBeginRead()
            {
                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
            }
            
        }
        public class UDP
        {
            public IPEndPoint ipEndPoint;
            private int ID { get; }

            public UDP(int iD) => ID = iD;

            public void Connect(IPEndPoint ipEndPoint)
            {
                this.ipEndPoint = ipEndPoint;
                ServerSend.UDPTest(ID);
            }

            public void SendPacket(Packet packet)
            {
                Server.SendUDPPacket(ipEndPoint, packet);
            }

            public void HandlePacket(Packet packet)
            {
                int packetLen = packet.ReadInt();
                byte[] data = packet.ReadBytes(packetLen);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    Packet packet = new Packet(data);
                    int packetID = packet.ReadInt();
                    Server.PacketHandlerDictionary[packetID](ID, packet);
                });
            }
            public void Disconnect()
            {
                ipEndPoint = null;
            }
        }
        
        public void SendIntoGame(string username)
        {
            player = new Player(ID, username, new Vector3(0, 0, 0));
            
            //Spawning rest of players for the connected user
            foreach (Client client in Server.ClientDictionary.Values)
            {
                if (client.player != null)
                {
                    if (client.ID != ID)
                    {
                        Player player = client.player;
                        ServerSend.SpawnPlayer(ID, player);
                    }
                }
            }

            //Spawning the player who just joined, for all connected users
            foreach (Client client in Server.ClientDictionary.Values)
            {
                if (client.player != null)
                {
                    ServerSend.SpawnPlayer(client.ID, player);
                }
            }
        }
        private void Disconnect()
        {
            //TODO: crashes when 2 players disconnect simultaneously, or stops receiving packets from players
            Console.WriteLine($"Player: {ID} has disconnected. {tCP.Socket.Client.RemoteEndPoint}");
            ServerSend.DisconnectPlayer(ID);
            
            tCP.Disconnect();
            uDP.Disconnect();

            player = null;
        }
    }
}
