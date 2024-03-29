﻿using Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;


namespace GameServer
{
    class DiscoveryTCPClientServer
    {
        public TcpClient TCPClient { get; private set; }
        public int ID { get; private set; }

        private static int DataBufferSize { get; set; } = 4096;
        private byte[] ReceiveBuffer;
        private NetworkStream Stream;
        private Packet ReceivePacket;

        public DiscoveryTCPClientServer(int id) => 
            ID = id;
        public void Connect(TcpClient client)
        {
            TCPClient = client;

            TCPClient.ReceiveBufferSize = DataBufferSize;
            TCPClient.SendBufferSize = DataBufferSize;

            Stream = TCPClient.GetStream();
            ReceiveBuffer = new byte[DataBufferSize];
            ReceivePacket = new Packet();

            StreamBeginRead();
        }
        public void Disconnect()
        {
            TCPClient.Close();
            TCPClient = null;
            Stream = null;
            ReceiveBuffer = null;
            ReceivePacket = null;
        }

        //Sending packets
        public void SendServerData(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping)
        {
            using (Packet packet = new Packet((int)LANDiscoveryServerPackets.serverData))
            {
                packet.Write(serverName);
                packet.Write(currentPlayerCount);
                packet.Write(maxPlayerCount);
                packet.Write(mapName);
                packet.Write(ping);
                SendPacket(packet);
            }
        }

        private void SendPacket(Packet packet)
        {
            packet.WriteLength();

            try
            {
                if (TCPClient != null)
                    Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception exception)
            {
                //Disconnect();
                Console.WriteLine($"\n\tError, occured when sending TCP data from client {ID}\nError{exception}");
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
                    Disconnect();
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
                Console.WriteLine($"\n\tError in BeginReadReceiveCallback of client {TCPClient.Client.RemoteEndPoint}...\nError: {exception}");
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
                    using (Packet packet = new Packet(bytes))
                    {
                        int packetId = packet.ReadInt();
                        DiscoveryServer.PacketHandlerDictionary[packetId](ID, packet);
                    }
                });
                packetLen = 0;

                if (ExitHandleData(ref packetLen))
                    return true;
            }
            if (packetLen < 2)
            {
                return true;
            }

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

        
    }
}