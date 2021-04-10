using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Shared;
using UnityEngine;

namespace GameServer
{
    class ClientServer
    {
        public byte ID { get; private set; }
        public TCPServer tCP { get; private set; }
        public UDPServer uDP { get; private set; }
        private static int DataBufferSize { get; set; } = 4096;

        public PlayerServer Player { get; private set; }
        private bool Disconnected { get; set; } = false;
        public readonly Vector2 SpawningVector2 = new Vector2(31.08f, 5.65f);

        public ClientServer(byte iD)
        {
            ID = iD;
            tCP = new TCPServer(iD);
            uDP = new UDPServer(iD);
        }

        public class TCPServer
        {
            public TcpClient Socket { get; private set; }
            private byte ID { get; }

            private byte[] ReceiveBuffer;
            private NetworkStream Stream;
            private Packet ReceivePacket;

            public TCPServer(byte iD) => ID = iD;
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
                    Output.WriteLine($"\n\tError, occured when sending TCP data to client {ID}\nError{exception}");
                    Server.ClientDictionary[ID].Disconnect();
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
                    Output.WriteLine($"\n\tError in {Server.ServerName} client: {ID}, error in TCP Disconnect...\n{exception}");
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
                    Output.WriteLine($"\n\tError in BeginReadReceiveCallback of client {ID}...\nError: {exception}");
                    Server.ClientDictionary[ID].Disconnect();
                }
            }            
            private bool HandleData(byte[] data)
            {
                byte packetLen = 0;
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
                            try
                            {
                                byte packetId = packet.ReadByte();
                                Server.PacketHandlerDictionary[packetId](ID, packet);
                            }
                            catch (Exception exception)
                            {
                                Output.WriteLine($"\n\tError in TCP HandlePacket of GameServer Client: {ID}...\n{exception}");
                                //Server.ClientDictionary[ID].Disconnect();
                                //Instead of disconnecting straight away, make sure packets are being received from this player,
                                //  If still receiving packets, then don't disconnect
                                //  Else, disconnect player
                            }
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
            private bool ExitHandleData(ref byte packetLen)
            {
                if (ReceivePacket.UnreadLength() >= sizeof(byte))
                {
                    packetLen = ReceivePacket.ReadPacketLen();
                    if (packetLen < 1)
                        return true;
                }
                return false;
            }
            
            
        }


        public class UDPServer
        {
            public IPEndPoint ipEndPoint;
            private byte ID { get; }

            public UDPServer(byte iD) => ID = iD;

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
                byte packetLen = packet.ReadPacketLen();
                byte[] data = packet.ReadBytes(packetLen);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    try
                    {
                        using (Packet newPacket = new Packet(data))
                        {
                            byte packetID = newPacket.ReadByte();
                            Server.PacketHandlerDictionary[packetID](ID, newPacket);
                        }
                    }
                    catch (Exception exception)
                    {
                        Output.WriteLine($"\n\tError in UDP HandlePacket of GameServer Client: {ID}...{exception}");
                        Server.ClientDictionary[ID].Disconnect();
                    }
                });
            }
            public void Disconnect()
            {
                ipEndPoint = null;
            }
        }

        internal void SetPlayer(string username)
        {
            Player = NetworkManager.Instance.InstantiatePlayer();
            Player.Init(ID, username);
        }
        public void SendIntoGame(int playerColor)
        {
            Player.ReadyToPlay = true;
            Player.SetPlayerData(SpawningVector2, playerColor);

            //Spawning the player who just joined, for all connected users
            foreach (ClientServer client in Server.ClientDictionary.Values)
            {
                if (client.Player != null)
                    ServerSend.SpawnPlayer(client.ID, Player, true);
            }
        }

        public void SpawnOtherPlayersToConnectedUser()
        {
            //Spawning rest of players for the connected user
            foreach (ClientServer client in Server.ClientDictionary.Values)
            {
                PlayerServer player = client.Player;
                if (player != null && client.ID != ID && player.ReadyToPlay) //Player exists, not own player, and is ready to play
                {
                    ServerSend.SpawnPlayer(ID, player, false);
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                try
                {
                    if (Player is null)
                    {
                        Output.WriteLine($"\n\tTrying to Disconnect Player: {ID}, when they are already null...");
                        tCP.Disconnect();
                        uDP.Disconnect();
                        return;
                    }
                }
                catch (Exception e)
                {
                    Output.WriteLine($"\n\tError in GameServer Disconnecting Player when null: {ID}...\n{e}");
                }

                Output.WriteLine($"\tPlayer: {ID} has disconnected. {tCP.Socket.Client.RemoteEndPoint}");
                ServerSend.DisconnectPlayer(ID);

                PlayerColor.FreeColor(Player.PlayerColor, ID);

                UnityEngine.Object.Destroy(Player.gameObject);
                Player = null;

                
                tCP.Disconnect();
                uDP.Disconnect();

                Disconnected = true;
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\n\tError in GameServer Disconnecting Player: {ID}...\n{exception}");
            }
        }
    }

}
