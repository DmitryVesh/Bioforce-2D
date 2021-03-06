﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int DataBufferSize = 4096;

    private string IPAddressLocalHost = "127.0.0.1";
    private string IPAddressLAN = "192.168.1.65";
    private string IPAddressInternet = "148.252.129.44";

    private string IPAddressConnectTo;
    public int PortNum = 28020; //Must be the same as GameServer Port
    public int ClientID = 0;
    public TCP tCP { get; set; }
    public UDP uDP;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> PacketHandlerDictionary;
    public bool Connected { get; set; } = false;

    [SerializeField] private float TimerTimeOutTime = 15;
    private bool TimerRunning { get; set; } = false;
    private float Timer { get; set; }

    public static bool IsIPAddressValid(string text)
    {
        string[] splitIPAddress = text.Split('.');
        if (splitIPAddress.Length != 4)
            return false;
        try
        {
            foreach (string item in splitIPAddress)
            {
                int byteValue = int.Parse(item);
                if (byteValue < 0 || byteValue > 255)
                    return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
    public void ConnectToServer()
    {
        InitClientData();
        
        tCP = new TCP();
        uDP = new UDP();

        ResetTimeOutTimer();

        tCP.Connect();
    }

    public void SuccessfullyConnected(int assignedID)
    {
        Connected = true;
        ResetTimeOutTimer(false);
        ClientID = assignedID;
        //TODO: Display connected to server sign 
    }
    public void ChangeIPAddressConnectTo(int IPAddressIndex)
    {
        string[] IPs = new string[] { IPAddressLAN, IPAddressLocalHost, IPAddressInternet };
        IPAddressConnectTo = IPs[IPAddressIndex];
    }
    public void SetManualIPAddressConnectTo(string IPaddress) =>
        IPAddressConnectTo = IPaddress;

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

            Socket.BeginConnect(Instance.IPAddressConnectTo, Instance.PortNum, ConnectCallback, Socket);
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
                    Packet packet = new Packet(bytes);
                    int packetID = packet.ReadInt();
                    PacketHandlerDictionary[packetID](packet);
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
            ipEndPoint = new IPEndPoint(System.Net.IPAddress.Parse(Instance.IPAddressConnectTo), Instance.PortNum);
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
                    Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
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
            IPAddressConnectTo = IPAddressLAN; //Set default address
        }
        else if (Instance != this)
        {
            Debug.Log($"Client instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
    }
    private void FixedUpdate()
    {
        if (TimerRunning)
            Timer += Time.fixedDeltaTime;
        else
            return;

        if (Timer > TimerTimeOutTime)
        {
            ResetTimeOutTimer(false);
            ConnectionTimedOut();
        }
    }
    private void ResetTimeOutTimer(bool runTimer = true)
    {
        TimerRunning = runTimer;
        Timer = 0;
    }
    private void ConnectionTimedOut()
    {
        Disconnect();
        NetworkingUI.Instance.DisplayTimeOutMessage();
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
        PacketHandlerDictionary.Add((int)ServerPackets.playerRotationAndVelocity, ClientRead.PlayerRotationAndVelocity);
        PacketHandlerDictionary.Add((int)ServerPackets.playerMovementStats, ClientRead.PlayerMovementStats);
        PacketHandlerDictionary.Add((int)ServerPackets.playerDisconnect, ClientRead.PlayerDisconnect);
        PacketHandlerDictionary.Add((int)ServerPackets.bulleShot, ClientRead.BulletShot);
        PacketHandlerDictionary.Add((int)ServerPackets.playerDied, ClientRead.PlayerDied);
        PacketHandlerDictionary.Add((int)ServerPackets.playerRespawned, ClientRead.PlayerRespawned);
        PacketHandlerDictionary.Add((int)ServerPackets.tookDamage, ClientRead.TookDamage);
    }
    private void Disconnect()
    {
        if (Connected)
        {
            try
            {
                tCP.Socket.Close();
                uDP.Socket.Close();
            }
            catch (Exception exception)
            {
                Debug.Log($"Error, tried to close TCP and UDP sockets:\n{exception}");
            }
            NetworkingUI.Instance.Disconnected();
            Connected = false;

            Debug.Log($"You, client: {ClientID} have been disconnected.");
            
            GameManager.Instance.DisconnectAllPlayers();
            
        }
    }    
}
