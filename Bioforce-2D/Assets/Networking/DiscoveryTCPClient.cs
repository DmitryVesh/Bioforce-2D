﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Output;
using Shared;

public abstract class DiscoveryTCPClient
{
    public TcpClient Socket { get; private set; }
    private byte[] ReceiveBuffer;
    private NetworkStream Stream;
    private Packet ReceivePacket;

    private int DataBufferSize = 4096;

    public Action OnDisconnectAction { get; set; }
    public Action OnTimedOutAction { get; set; }

    protected delegate void PacketHandler(string ip, Packet packet);
    protected Dictionary<byte, PacketHandler> PacketHandlerDictionary { get; set; } = new Dictionary<byte, PacketHandler>();

    public DiscoveryTCPClient()
    {
        InitPacketHandlerDictionary();
    }

    protected abstract void InitPacketHandlerDictionary();

    public void Connect(string ipAddressConnectTo, int portNum)
    {
        try
        {
            Socket = new TcpClient();
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;

            ReceiveBuffer = new byte[DataBufferSize];
            Socket.BeginConnect(ipAddressConnectTo, portNum, ConnectCallback, Socket);
            Output.WriteLine($"A DiscoveryTCPClient trying to connect to: {ipAddressConnectTo}:{portNum}...");
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Error Connecting DiscoveryTCPClient...{exception}");
            Disconnect(false, true);
        }
    }
    public void SendPacket(Packet packet)
    {
        packet.WriteLength();
        try
        {
            if (Socket != null)
                Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Error, sending data to server from Client via TCP.\nException {exception}");
            Disconnect(false, true);
        }
    }
    public void Disconnect(bool crashed, bool timedOut)
    {
        if (Socket != null)
            Socket.Close();
        if (Stream != null)
            Stream.Close();
        Socket = null;
        Stream = null;
        ReceiveBuffer = null;
        ReceivePacket = null;

        if (crashed)
            OnDisconnectAction?.Invoke();
        else if (timedOut)
            OnTimedOutAction?.Invoke();
    }
    public bool IsConnected() =>
        Socket.Connected;

    private void ConnectCallback(IAsyncResult asyncResult)
    {
        try
        {
            //Error occurs here due to the server socket not establishing a connection with the client socket
            Socket.EndConnect(asyncResult);
            if (!Socket.Connected)
                return; // Not connected yet, then exit
            Stream = Socket.GetStream();
            ReceivePacket = new Packet();
            StreamBeginRead();
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Error in TCP ConnectCallback\n{exception}");
            Disconnect(true, false);            
        }
    }

    private void BeginReadReceiveCallback(IAsyncResult asyncResult)
    {
        try
        {
            int byteLen = Stream.EndRead(asyncResult);
            if (byteLen <= 0)
            {
                Disconnect(true, false);
                return;
            }

            byte[] data = new byte[byteLen];
            Array.Copy(ReceiveBuffer, data, byteLen);

            ReceivePacket.Reset(HandleData(data));
            StreamBeginRead();
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Error in BeginReadReceiveCallback: {Socket.Client.RemoteEndPoint}...\nError: {exception}");
            Disconnect(true, false);
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
                try
                {
                    Packet packet = new Packet(bytes);
                    byte packetID = packet.ReadByte();
                    string ipAddress = Socket.Client.RemoteEndPoint.ToString().Split(':')[0];
                    PacketHandlerDictionary[packetID](ipAddress, packet);
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"\nError in HandleData of DiscoveryTCPClient...\n{exception}");
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
    private void StreamBeginRead()
    {
        Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
    }
    
}
