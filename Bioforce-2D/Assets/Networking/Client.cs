using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;
using UnityEngine.SceneManagement;
using TMPro;

public class Client : MonoBehaviour
{
    public static Client Instance { get => instance; }
    private static Client instance;

    public static int DataBufferSize = 4096;

    public const int PortNumInternetDiscover = 28020;
    public const int PortNumLANDiscover = 28021;

    public const int PortNumInternetDiscoverTesting = 28420;

    public static int PortNumInternetToConnectTo
    {
        get
        {
            return Instance.IsTesting ? PortNumInternetDiscoverTesting : PortNumInternetDiscover;
        }
    }
    [SerializeField] private bool IsTesting = false;

    public static int PortNumGame { get; private set; }
    public static string MainServerIP;
    //[SerializeField] public string MainServerIP = "18.134.197.3";
    //public const string MainServerIP = "127.0.0.1";

    public byte ClientID { get; set; } = 0;
    public TCP tCP { get; set; }
    public UDP uDP { get; set; }

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<byte, PacketHandler> PacketHandlerDictionary;
    public bool Connected { get; set; } = false;

    private float TimerTimeOutTime = 10;
    private bool TimerRunning { get; set; } = false;
    private float Timer { get; set; }

    private TimeSpan PacketTimeOutTCP { get; set; }
    private TimeSpan PacketPauseTCP { get; set; }
    private readonly TimeSpan TimeSpanZero = new TimeSpan(0, 0, 0);
    private bool LastLostConnection { get; set; }

    private TimeSpan PacketSendViaOnlyTCP { get; set; }
    private TimeSpan PacketSendViaTCPAndUDP { get; set; }

    private byte FixedFrameCounter { get; set; } = 0;

    public float Latency1WaySecondsTCP { get; private set; } = 0.15f;
    public float Latency2WayMSTCP { get => (Latency1WaySecondsTCP * 2000); }
    public float Latency1WaySecondsUDP { get; private set; } = 0.15f;

    public bool IsPlaying { get; set; }

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
    public void ConnectToServer(string ip, int port)
    {
        PortNumGame = port;
        Output.WriteLine($"Client going to try and connect to: {ip}:{port}");
        InitClientData();

        tCP = new TCP();
        uDP = new UDP(ip);

        ResetTimeOutTimer();

        StartCoroutine(ConnectTCP(ip));   
    }
    private IEnumerator ConnectTCP(string ip)
    {
        yield return new WaitForSeconds(3); //Delays connection to server, as problems when query it too fast
        tCP.Connect(ip);
    }
    public void Disconnect()
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
                Output.WriteLine($"Error, tried to close TCP and UDP sockets:\n{exception}");
            }
            IsPlaying = false;
            Connected = false;

            Output.WriteLine($"You, client: {ClientID} have been disconnected.");

            GameManager.Instance.DisconnectLoadMainMenu();
        }
    }
    public void SuccessfullyConnected(byte assignedID)
    {
        Connected = true;
        IsPlaying = false;
        ResetTimeOutTimer(false);
        ClientID = assignedID;
        SetPacketTimeoutsTCP(DateTime.Now.TimeOfDay);
    }

    public class TCP
    {
        public TcpClient Socket { get; private set; }
        private byte[] ReceiveBuffer;
        private NetworkStream Stream;
        private Packet ReceivePacket;


        public void Connect(string ip)
        {
            Socket = new TcpClient();
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;

            ReceiveBuffer = new byte[DataBufferSize];

            Socket.BeginConnect(ip, PortNumGame, ConnectCallback, Socket);
            
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
                Output.WriteLine($"Error, sending data to server from Client: {Instance.ClientID} via TCP.\nException {exception}");
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket.EndConnect(asyncResult);
                if (!Socket.Connected) { return; } // Not connected yet, then exit

                Stream = Socket.GetStream();
                ReceivePacket = new Packet();
                StreamBeginRead();
            }
            catch (Exception exception)
            {
                Output.WriteLine($"Error in TCP ConnectCallback\n{exception}");
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
                Output.WriteLine($"\nError in BeginReadReceiveCallback...\nError: {exception}");
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
                    byte packetID = byte.MaxValue;
                    try
                    {
                        using (Packet packet = new Packet(bytes))
                        {
                            packetID = packet.ReadByte();
                            PacketHandlerDictionary[packetID](packet);
                        }
                    }
                    catch (KeyNotFoundException exception)
                    {
                        Output.WriteLine($"Error in Handle data of TCP Packet {packetID} ...\n{exception}");
                    }
                    catch (Exception exception)
                    {
                        Output.WriteLine($"Error in Handle data of not know TCP Packet...\n{exception}");
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
            if (ReceivePacket.UnreadLength() >= sizeof(byte)) //TODO: Might cause problems when switching the packetLen from int -> short or whatever: if byte, >= 1, ushort >= 2
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

        public UDP(string ip)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), PortNumGame);
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
                //packet.InsertInt(Instance.ClientID);
                packet.InsertByte(Instance.ClientID);
                if (Socket != null)
                    Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"Error, sending data to server from Client: {Instance.ClientID} via UDP.\nException {exception}");
                Disconnect();
            }
        }
        private void BeginReadReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = Socket.EndReceive(result, ref ipEndPoint);
                SocketBeginReceive();

                if (data.Length < sizeof(byte))
                {
                    Instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"Player {Instance.ClientID} Can't access the server via UDP.\nDisconnecting from server.\n{exception}");
                Disconnect();
            }
        }
        private void HandleData(byte[] data)
        {
            using (Packet packetInit = new Packet(data))
            {
                byte packetLen = packetInit.ReadPacketLen();
                data = packetInit.ReadBytes(packetLen);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    byte packetID = packet.ReadByte();
                    PacketHandlerDictionary[packetID](packet);
                }
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


    [SerializeField] private GameObject Canvas_TestingIP;
    [SerializeField] private TMP_InputField InputField_TestingIP;

    private void Awake()
    {
        Singleton.Init(ref instance, this);

        SceneManager.sceneLoaded += OnSceneChange;          
    }
    public void On_InputField_Change(string input)
    {
        MainServerIP = input;
    }
    private void OnSceneChange(Scene scene, LoadSceneMode _)
    {
        if (scene != SceneManager.GetSceneByName(GameManager.SceneMainMenuName)) //IP field should be hidden if not Main Menu scene
        {
            Canvas_TestingIP.SetActive(false);
            return;
        }

        if (!IsTesting) //IP field should't be visible if not testing/in release mode
            return;

        //Scene is Main Menu, and IsTesting
        Canvas_TestingIP.SetActive(true);
    }

    bool shouldSendMoveState = false;
    PlayerMovingState moveState = PlayerMovingState.idleRight;

    bool shouldSendWorldPosition = false;

    public static readonly Vector2 SpawningVector2 = new Vector2(31.08f, 5.65f);
    Vector2 playerPosition = SpawningVector2; //Default TODO: Change with actual player position

    bool shouldSendArmPosition = false;
    Vector2 armPosition = Vector2.zero;

    bool shouldSendArmRotation = false;
    Quaternion armRotation = Quaternion.identity;

    internal void FlagMoveStateToBeSent(PlayerMovingState currentMovingState) =>
        (shouldSendMoveState, moveState) = (true, currentMovingState);
    internal void FlagWorldPositionToBeSent(Vector2 playerPosition) =>
        (shouldSendWorldPosition, this.playerPosition) = (true, playerPosition);
    internal void FlagArmPositionToBeSent(Vector2 currentPosition) =>
        (shouldSendArmPosition, armPosition) = (true, currentPosition);
    internal void FlagArmRotationToBeSent(Quaternion currentRotation) =>
        (shouldSendArmRotation, armRotation) = (true, currentRotation);

    private void SendPlayerData()
    {
        if (shouldSendMoveState || shouldSendWorldPosition || shouldSendArmPosition || shouldSendArmRotation)
        {
            ClientSend.ConstantPlayerData
                (
                shouldSendMoveState, moveState,
                shouldSendWorldPosition, playerPosition,
                shouldSendArmPosition, armPosition,
                shouldSendArmRotation, armRotation
                );

            shouldSendMoveState = false;
            shouldSendWorldPosition = false;
            shouldSendArmPosition = false;
            shouldSendArmRotation = false;
        }
    }

    private void FixedUpdate()
    {
        if (Connected)
        {
            if (IsPlaying)
                SendPlayerData();

            if (FixedFrameCounter++ % 2 == 0)
                return;

            ClientSend.PlayerConnectedTCPPacket();
            ClientSend.PlayerConnectedUDPPacket();

            TimeSpan now = DateTime.Now.TimeOfDay;

            SendConstantPacketsState sendConstantPacketsState;

            if (PacketSendViaOnlyTCP - now < TimeSpanZero) //Flag so only send constant packets via TCP, because UDP is not responsive, so less data is sent
                sendConstantPacketsState = SendConstantPacketsState.TCP;
            else if (PacketSendViaTCPAndUDP - now < TimeSpanZero) //Flag so both constantly sent packets are sent via both UDP and TCP
                sendConstantPacketsState = SendConstantPacketsState.UDPandTCP;
            else //Flag so can send via only UDP
                sendConstantPacketsState = SendConstantPacketsState.UDP;

            ClientSend.SendConstantPacketsState = sendConstantPacketsState;

            if (PacketTimeOutTCP - now < TimeSpanZero)
            {
                //TODO: add message when disconnected because of a time out
                Output.WriteLine("Lost connection with GameServer");
                Disconnect();
                return;
            }

            bool lostConnection = PacketPauseTCP - now < TimeSpanZero;
            if (LastLostConnection != lostConnection)
            {
                LastLostConnection = lostConnection;
                GameManager.Instance.InvokeLostConnectionEvent(lostConnection);
            }
        }

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


    #region Ping Stuff
    
    public void PlayerConnectedAcknAndPingTCP(TimeSpan now, ushort latency2WayMSTCP)
    {
        SetPacketTimeoutsTCP(now);

        Latency1WaySecondsTCP = latency2WayMSTCP / 2000f;
    }
    private void SetPacketTimeoutsTCP(TimeSpan now)
    {
        PacketTimeOutTCP = now + new TimeSpan(0, 0, 10);
        PacketPauseTCP = now + new TimeSpan(0, 0, 2);
    }
    public void PlayerConnectedAcknAndPingUDP(TimeSpan now, ushort latency2WayMSUDP)
    {
        PacketSendViaOnlyTCP = now + new TimeSpan(0, 0, 5);
        PacketSendViaTCPAndUDP = now + new TimeSpan(0, 0, 0, 0, 500);

        Latency1WaySecondsUDP = latency2WayMSUDP / 2000f;
    }

    #endregion Ping Stuff

    private void ResetTimeOutTimer(bool runTimer = true)
    {
        TimerRunning = runTimer;
        Timer = 0;
    }
    private void ConnectionTimedOut()
    {
        Disconnect();
        ServerMenu.ServerConnectionTimeOut();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
    private void Start()
    {
        InputField_TestingIP.text = MainServerIP;

        GameStateManager.ServerShuttingDown += Disconnect;
    }

    

    private void OnDestroy()
    {
        Disconnect();
        GameStateManager.ServerShuttingDown -= Disconnect;

        SceneManager.sceneLoaded -= OnSceneChange;
    }
    private void OnApplicationPause(bool pause)
    {
        if (Connected)
            ClientSend.PausedGame(pause);
    }

    private void InitClientData()
    {
        PacketHandlerDictionary = new Dictionary<byte, PacketHandler>();
        PacketHandlerDictionary.Add((byte)ServerPackets.welcome, ClientRead.WelcomeRead);
        PacketHandlerDictionary.Add((byte)ServerPackets.udpTest, ClientRead.UDPTestRead);
        PacketHandlerDictionary.Add((byte)ServerPackets.spawnPlayer, ClientRead.SpawnPlayer);
        //PacketHandlerDictionary.Add((byte)ServerPackets.playerPosition, ClientRead.PlayerPosition);
        //PacketHandlerDictionary.Add((byte)ServerPackets.playerRotationAndVelocity, ClientRead.PlayerRotationAndVelocity);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerMovementStats, ClientRead.PlayerMovementStats);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerDisconnect, ClientRead.PlayerDisconnect);
        PacketHandlerDictionary.Add((byte)ServerPackets.bulleShot, ClientRead.BulletShot);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerDied, ClientRead.PlayerDied);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerRespawned, ClientRead.PlayerRespawned);
        PacketHandlerDictionary.Add((byte)ServerPackets.tookDamage, ClientRead.TookDamage);
        PacketHandlerDictionary.Add((byte)ServerPackets.serverIsFull, ClientRead.ServerIsFull);
        //PacketHandlerDictionary.Add((byte)ServerPackets.armPositionRotation, ClientRead.ArmPositionRotation);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerPausedGame, ClientRead.PlayerPausedGame);
        PacketHandlerDictionary.Add((byte)ServerPackets.stillConnectedTCP, ClientRead.PlayerStillConnectedTCP);
        PacketHandlerDictionary.Add((byte)ServerPackets.stillConnectedUDP, ClientRead.PlayerStillConnectedUDP);
        PacketHandlerDictionary.Add((byte)ServerPackets.askPlayerDetails, ClientRead.AskingForPlayerDetails);
        PacketHandlerDictionary.Add((byte)ServerPackets.freeColor, ClientRead.FreeColor);
        PacketHandlerDictionary.Add((byte)ServerPackets.takeColor, ClientRead.TakeColor);
        PacketHandlerDictionary.Add((byte)ServerPackets.triedTakingTakenColor, ClientRead.TriedTakingTakenColor);
        PacketHandlerDictionary.Add((byte)ServerPackets.generatedPickup, ClientRead.GeneratedPickupItem);
        PacketHandlerDictionary.Add((byte)ServerPackets.playerPickedUpItem, ClientRead.PlayerPickedUpItem);
        PacketHandlerDictionary.Add((byte)ServerPackets.chatMessage, ClientRead.ChatMessage);
        PacketHandlerDictionary.Add((byte)ServerPackets.gameState, ClientRead.ReadGameState);
        PacketHandlerDictionary.Add((byte)ServerPackets.constantPlayerData, ClientRead.ConstantPlayerData);
    }
       
}
