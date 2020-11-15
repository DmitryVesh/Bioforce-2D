using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int DataBufferSize = 4096;

    public string IPAddress = "127.0.0.1";
    public int PortNum = 28020; //Must be the same as GameServer Port
    public int ClientID = 0;
    public TCP tCP;
    public UDP uDP;

    //TODO: Make a class that ones with PacketHandlers can inheret from to don't have to rewrite
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> PacketHandlerDictionary;
    private bool Connected { get; set; } = false;

    public void ConnectToServer()
    {
        InitClientData();
        Connected = true;
        
        tCP = new TCP();
        uDP = new UDP();

        tCP.Connect();
    }

    public class TCP
    {
        public TcpClient Socket { get; private set; }
        private byte[] ReceiveBuffer;
        private NetworkStream Stream;
        private Packet ReceivePacket;


        public void Connect()
        {
            Socket = new TcpClient();
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;

            ReceiveBuffer = new byte[DataBufferSize];

            Socket.BeginConnect(Instance.IPAddress, Instance.PortNum, ConnectCallback, Socket);
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
                Debug.Log($"Error, sending data to server from Client: {Instance.ClientID} via TCP.\nException {exception}");
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Socket.EndConnect(asyncResult);
            if (!Socket.Connected) { return; } // No connected yet, then exit

            Stream = Socket.GetStream();
            ReceivePacket = new Packet();
            StreamBeginRead();
        }

        private void BeginReadReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                int byteLen = Stream.EndRead(asyncResult);
                if (byteLen <= 0)
                {
                    Instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLen];
                Array.Copy(ReceiveBuffer, data, byteLen);

                ReceivePacket.Reset(HandleData(data));
                StreamBeginRead();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\nError in BeginReadReceiveCallback...\nError: {exception}");
                Disconnect();
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
                    Packet packet = new Packet(bytes);
                    int packetID = packet.ReadInt();
                    PacketHandlerDictionary[packetID](packet);
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

        //TODO: Implement so don't copy and paste above
        private bool ExitHandleData()
        {
            if (ReceivePacket.UnreadLength() >= 4)
            {
                int packetLen = ReceivePacket.ReadInt();
                if (packetLen < 1)
                {
                    return true;
                }
            }
            return false;
        }
        private void StreamBeginRead()
        {
            Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
        }
        private void Disconnect()
        {
            Instance.Disconnect();

            Socket = null;
            Stream = null;
            ReceiveBuffer = null;
            ReceivePacket = null;
            
        }
    }
    public class UDP
    {
        public UdpClient Socket;
        public IPEndPoint ipEndPoint;

        public UDP()
        {
            ipEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(Instance.IPAddress), Instance.PortNum);
        }
        public void Connect(int localPort)
        {
            Socket = new UdpClient(localPort);
            Socket.Connect(ipEndPoint);
            SocketBeginReceive();

            Packet packet = new Packet();
            SendPacket(packet);
        }
        public void SendPacket(Packet packet)
        {
            try
            {
                packet.InsertInt(Instance.ClientID);
                if (Socket != null)
                {
                    Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception exception)
            {
                Debug.Log($"Error, sending data to server from Client: {Instance.ClientID} via UDP.\nException {exception}");
                Disconnect();
            }
        }
        private void BeginReadReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = Socket.EndReceive(result, ref ipEndPoint);
                SocketBeginReceive();

                if (data.Length < 4)
                {
                    Instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception exception)
            {
                Debug.Log($"Player {Instance.ClientID} Can't access the server via UDP.\nDisconnecting from server.\n{exception}");
                Disconnect();
            }
        }
        private void HandleData(byte[] data)
        {
            Packet packetInit = new Packet(data);
            int packetLen = packetInit.ReadInt();
            data = packetInit.ReadBytes(packetLen);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                Packet packet = new Packet(data);
                int packetID = packet.ReadInt();
                PacketHandlerDictionary[packetID](packet);
            });
        }
        private void SocketBeginReceive()
        {
            Socket.BeginReceive(BeginReadReceiveCallback, null);
        }
        private void Disconnect()
        {
            Instance.Disconnect();
            ipEndPoint = null;
            Socket = null;            
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"Client instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void InitClientData()
    {
        PacketHandlerDictionary = new Dictionary<int, PacketHandler>();
        PacketHandlerDictionary.Add((int)ServerPackets.welcome, ClientRead.WelcomeRead);
        PacketHandlerDictionary.Add((int)ServerPackets.udpTest, ClientRead.UDPTestRead);
        PacketHandlerDictionary.Add((int)ServerPackets.spawnPlayer, ClientRead.SpawnPlayer);
        PacketHandlerDictionary.Add((int)ServerPackets.playerPosition, ClientRead.PlayerPosition);
        PacketHandlerDictionary.Add((int)ServerPackets.playerVelocity, ClientRead.PlayerVelocity);
        PacketHandlerDictionary.Add((int)ServerPackets.playerRotation, ClientRead.PlayerRotation);
        PacketHandlerDictionary.Add((int)ServerPackets.playerDisconnect, ClientRead.PlayerDisconnect);
    }
    private void Disconnect()
    {
        if (Connected)
        {
            SimpleNetworkingUI.Instance.Disconnected();
            Connected = false;
            
            tCP.Socket.Close();
            uDP.Socket.Close();

            Debug.Log($"You, client: {ClientID} have been disconnected.");
            
            GameManager.Instance.DisconnectAllPlayers();
            
        }
    }    
}
