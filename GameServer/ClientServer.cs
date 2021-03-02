using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Shared;

namespace GameServer
{
    class ClientServer
    {
        public int ID { get; private set; }
        public TCPServer tCP { get; private set; }
        public UDPServer uDP { get; private set; }
        private static int DataBufferSize { get; set; } = 4096;

        public PlayerServer Player { get; private set; }

        public ClientServer(int iD)
        {
            ID = iD;
            tCP = new TCPServer(iD);
            uDP = new UDPServer(iD);
        }

        public class TCPServer
        {
            public TcpClient Socket { get; private set; }
            private int ID { get; }
            private byte[] ReceiveBuffer;
            private NetworkStream Stream;
            private Packet ReceivePacket;

            public TCPServer(int iD) => ID = iD;
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
                        Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
                catch (Exception exception)
                {
                    //Disconnect();
                    Console.WriteLine($"\n\tError, occured when sending TCP data from client {ID}\nError{exception}");
                }
            }
            public void Disconnect()
            {
                try
                {
                    Socket.Close();
                    Socket = null;
                    Stream = null;
                    ReceiveBuffer = null;
                    ReceivePacket = null;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\n\tError in {Server.ServerName} client: {ID}, error in Disconnect...{exception}");
                }
            }

            private void StreamBeginRead()
            {
                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
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

                    bool resetData = HandleData(data);
                    ReceivePacket.Reset(resetData);
                    StreamBeginRead();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\n\tError in BeginReadReceiveCallback of client {ID}...\nError: {exception}");
                    Server.ClientDictionary[ID].Disconnect();
                }
            }            
            private bool HandleData(byte[] data)
            {
                int packetLen = 0;
                ReceivePacket.SetBytes(data);

                if (ExitHandleData(ref packetLen))
                    return true;

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

                    if (ExitHandleData(ref packetLen))
                        return true;
                }
                if (packetLen < 2)
                    return true;

                return false;
            }
            private bool ExitHandleData(ref int packetLen)
            {
                if (ReceivePacket.UnreadLength() >= 4)
                {
                    packetLen = ReceivePacket.ReadInt();
                    if (packetLen < 1)
                        return true;
                }
                return false;
            }
            
            
        }
        public class UDPServer
        {
            public IPEndPoint ipEndPoint;
            private int ID { get; }

            public UDPServer(int iD) => ID = iD;

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
                    Packet newPacket = new Packet(data);
                    int packetID = newPacket.ReadInt();
                    Server.PacketHandlerDictionary[packetID](ID, newPacket);
                });
            }
            public void Disconnect()
            {
                ipEndPoint = null;
            }
        }
        
        public void SendIntoGame(string username)
        {
            Player = new PlayerServer(ID, username, new Vector2(0, 0), PlayerColor.GetRandomColor());

            //Spawning the player who just joined, for all connected users
            foreach (ClientServer client in Server.ClientDictionary.Values)
            {
                if (client.Player != null)
                    ServerSend.SpawnPlayer(client.ID, Player, true);
            }

            //Spawning rest of players for the connected user
            foreach (ClientServer client in Server.ClientDictionary.Values)
            {
                if (client.Player != null)
                {
                    if (client.ID != ID)
                    {
                        PlayerServer player = client.Player;
                        ServerSend.SpawnPlayer(ID, player, false);
                    }
                }
            }

            
        }
        public void Disconnect()
        {
            PlayerColor.GiveBackRandomColor(Player.PlayerColor);
            Console.WriteLine($"\tPlayer: {ID} has disconnected. {tCP.Socket.Client.RemoteEndPoint}");
            ServerSend.DisconnectPlayer(ID);
            
            tCP.Disconnect();
            uDP.Disconnect();

            Player = null;
        }
    }
}
