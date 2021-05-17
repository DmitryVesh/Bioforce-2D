using Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Output;

namespace GameServer
{
    public class MainServerComms
    {
        public const int DataBufferSize = 4096;
        public const string LocalIP = "127.0.0.1";

        public static TCP tCP { get; set; }

        private delegate void PacketHandler(Packet packet);
        private static Dictionary<byte, PacketHandler> PacketHandlerDictionary;
        public static bool EstablishedConnection { get; set; } = false;
        public static int Port { get; private set; }
        public static string ServerName { get; set; }
        private static int TimeOut { get; set; } 
        public static bool ServerEndsWhenNoPlayersOn
        {
            get => TimeOut != -1;
        }

        public static void Connect(int port, string serverName, int timeOut)
        {
            Port = port;
            ServerName = serverName;
            TimeOut = timeOut;

            Output.WriteLine($"\n\t\tMainServerComms trying to connect:{Port}");
            InitIncomingPacketHandler();

            tCP = new TCP();
            tCP.Connect(LocalIP);            

            Application.quitting += OnApplicationQuiting;
        }

        private static void OnApplicationQuiting()
        {
            Output.WriteLine($"\t\tEnding MainServerComms: {Port}");
            Application.Quit();
            Disconnect();
        }

        public static void Disconnect()
        {
            if (EstablishedConnection)
            {
                try
                {
                    tCP.Socket.Close();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"\n\t\tError, tried to close TCP sockets of MainServerComms:\n{exception}");
                }
                EstablishedConnection = false;
                MainServerCommsSend.ShuttingDown(ServerName);

                Output.WriteLine($"\n\t\tMainServerComms: {Port} has disconnected.");
            }

        }

        public static void Update()
        {
            Output.WriteLine($"\n\t\tStarted MainServerComms Update thread.");
            DateTime TickTimer = DateTime.Now;
            double SendTime = 1000;
            int numTimesServerIsEmpty = 0;

            while (EstablishedConnection)
            {
                while (TickTimer < DateTime.Now)
                {
                    int currentNumPlayers = Server.GetCurrentNumPlayers();
                    if (currentNumPlayers == 0)
                        numTimesServerIsEmpty += 1;
                    else
                        numTimesServerIsEmpty = 0;

                    if (TimeOut != -1 && numTimesServerIsEmpty >= TimeOut)
                    {
                        Output.WriteLine("" +
                            "\n\t\t------------------------------------------------------------" +
                            $"\n\t\tGameServer {ServerName} is shutting down due to no players present..." +
                            "\n\t\t------------------------------------------------------------");
                        Application.Quit(0);
                        EstablishedConnection = false;
                        break;
                    }

                    if (EstablishedConnection)
                        MainServerCommsSend.ServerData(ServerName, currentNumPlayers, Server.MaxNumPlayers, Server.MapName);

                    TickTimer = TickTimer.AddMilliseconds(SendTime);

                    if (TickTimer > DateTime.Now)
                        Thread.Sleep(TickTimer - DateTime.Now);
                }
            }
            Output.WriteLine($"\t\tEnding MainServerComms server: {Port}");
        }
        
        public class TCP
        {
            public TcpClient Socket { get; private set; }
            private byte[] ReceiveBuffer;
            private NetworkStream Stream;
            private Packet ReceivePacket;

            public void Connect(string ip)
            {
                try
                {
                    Socket = new TcpClient();
                    Socket.ReceiveBufferSize = DataBufferSize;
                    Socket.SendBufferSize = DataBufferSize;

                    ReceiveBuffer = new byte[DataBufferSize];

                    Socket.BeginConnect(ip, Port, ConnectCallback, Socket);
                }
                catch (Exception e)
                {
                    Output.WriteLine($"Error in Connecting of MainServerComms...\n{e}");
                }
            }
            public void SendPacket(Packet packet)
            {
                try
                {
                    if (Socket == null)
                        Output.WriteLine("" +
                            "\n*************************************" +
                            "\nSocket is null" +
                            "\n*************************************");
                    if (Stream == null)
                        Output.WriteLine("" +
                            "\n*************************************" +
                            "\nStream is null" +
                            "\n*************************************");
                    if (packet == null)
                        Output.WriteLine("" +
                            "\n*************************************" +
                            "\nPacket is null" +
                            "\n*************************************");

                    if (Socket != null)
                        Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"\n\t\tGameServer: {ServerName} error, sending data to MainServerComms: {Port}\nException {exception}");
                }
            }

            private void ConnectCallback(IAsyncResult asyncResult)
            {
                try
                {
                    Socket.EndConnect(asyncResult);
                    if (!Socket.Connected) 
                    {
                        Output.WriteLine($"\n\t\tGameServer: {ServerName} socket to MainServer is not yet connected");
                        return; 
                    } // Not connected yet, then exit

                    Stream = Socket.GetStream();
                    ReceivePacket = new Packet();
                    StreamBeginRead();

                    Output.WriteLine($"\n\t\tGameSever: {ServerName} has end connected ConnectCallback from MainServer");
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"\n\t\tError in MainServerComms: {Port} TCP ConnectCallback\n{exception}");
                }
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

                    ReceivePacket.Reset(HandleData(data));
                    StreamBeginRead();
                }
                catch (Exception exception)
                {
                    Output.WriteLine($"\n\t\tError in MainServerComms: {Port} BeginReadReceiveCallback...\nError: {exception}");
                    Disconnect();
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
                        Packet packet = new Packet(bytes);
                        byte packetID = packet.ReadByte();
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
            private void Disconnect()
            {
                MainServerComms.Disconnect();

                Socket = null;
                Stream = null;
                ReceiveBuffer = null;
                ReceivePacket = null;
            }
        }

        private static void InitIncomingPacketHandler()
        {
            PacketHandlerDictionary = new Dictionary<byte, PacketHandler>();
            PacketHandlerDictionary.Add((byte)MainServerToServer.welcome, MainServerCommsRead.Welcome);
        }
    }
}