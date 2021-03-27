using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

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
    protected Dictionary<int, PacketHandler> PacketHandlerDictionary { get; set; } = new Dictionary<int, PacketHandler>();

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
            Debug.Log($"A DiscoveryTCPClient trying to connect to: {ipAddressConnectTo}...");
        }
        catch (Exception exception)
        {
            Debug.Log($"Error Connecting DiscoveryTCPClient...{exception}");
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
            Debug.Log($"Error, sending data to server from Client via TCP.\nException {exception}");
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
            Debug.Log($"Error in TCP ConnectCallback\n{exception}");
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
            Debug.Log($"Error in BeginReadReceiveCallback: {Socket.Client.RemoteEndPoint}...\nError: {exception}");
            Disconnect(true, false);
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
                try
                {
                    Packet packet = new Packet(bytes);
                    int packetID = packet.ReadInt();
                    string ipAddress = Socket.Client.RemoteEndPoint.ToString().Split(':')[0];
                    PacketHandlerDictionary[packetID](ipAddress, packet);
                }
                catch (Exception exception)
                {
                    Debug.Log($"\nError in HandleData of DiscoveryTCPClient...\n{exception}");
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
    private void StreamBeginRead()
    {
        Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
    }
    
}
