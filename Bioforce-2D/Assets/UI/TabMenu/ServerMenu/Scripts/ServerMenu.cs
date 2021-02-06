using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance;
    public ServerEntry ServerEntryConnectTo { get; private set; } = null;

    private GameObject ServerMenuPanel { get; set; }
    private GameObject PlayerRegistrationPanel { get; set; }

    [SerializeField] private MenuButton ConnectButton;    

    [SerializeField] private GameObject ServerPageHolder;
    [SerializeField] private GameObject KeyServerFields;


    //Server pages & Manual Entries
    private ServersPage SelectedServersPage { get; set; } = null;
    private Dictionary<GameObject, ServersPage> ServerPagesDict { get; set; }
    private ServersPage InternetServersPage { get; set; }
    private ServersPage LANServersPage { get; set; }
    private GameObject ManualEntryObject { get; set; }
    private bool SelectedManualEntry { get; set; }
    private GameObject CreateServerObject { get; set; }

    private LANServerScanner ServerScanner { get; set; }

    public static void ReadServerDataPacket(string serverIP, Packet packet)
    {
        string serverName = packet.ReadString();
        int currentPlayerCount = packet.ReadInt();
        int maxPlayerCount = packet.ReadInt();
        string mapName = packet.ReadString();
        int ping = packet.ReadInt(); //TODO: look into how to calculate ping

        Instance.AddServerToPage(serverName, currentPlayerCount, maxPlayerCount, mapName, ping, serverIP);
    }
    public static void SetManualIPAddress(bool ipValid) =>
        Instance.ConnectButton.Interactable = ipValid;

    public static void SetEntrySelected(ServerEntry serverEntry)
    {
        Instance.ServerEntryConnectTo = serverEntry;
        Instance.ConnectButton.Interactable = true;
    }
    public static void ServerConnectionTimeOut()
    {
        //TODO: 9000 Implement Server Connection Time out
        throw new NotImplementedException();
    }
    public static void Disconnected()
    {
        if (Instance != null)
            Instance.ServerMenuPanel.SetActive(true);
    }

    public void OnConnectButtonPressed()
    {        
        ServerMenuPanel.SetActive(false);
        //TODO: Deal when the player count is full, e.g. 10/10 players
        if (SelectedManualEntry) //TODO: Make so Discovery tcp client is made to check out the ip address
            throw new NotImplementedException();
        else
            Client.Instance.ConnectToServer(ServerEntryConnectTo.ServerIP);
    }
    public void ShowServerMenu()
    {
        if (!PlayerPrefs.HasKey("Username"))
            DisplayUserRegistration();
        else
        {
            ServerMenuPanel.SetActive(true);
            LoadServersForSelectedServersPage();
        }
    }
    public void HideServerMenu() =>
        ServerMenuPanel.SetActive(false);
    public void DisplayUserRegistration()
    {
        HideServerMenu();
        PlayerRegistrationPanel.SetActive(true);
        PlayerRegistration.Instance.DisplayUserRegistration();
    }

    

    public void SetSelectedPage(GameObject selectedPage)
    {       
        ConnectButton.Interactable = false;
        SelectedManualEntry = selectedPage == ManualEntryObject;
        bool hasPage = ServerPagesDict.ContainsKey(selectedPage);
        if (hasPage)
        {
            SelectedServersPage = ServerPagesDict[selectedPage];
            LoadServersForSelectedServersPage();    
        }
        else
            KeyServerFields.SetActive(false);            
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"ServerMenu instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
        ServerMenuPanel = transform.GetChild(0).gameObject;
        PlayerRegistrationPanel = transform.GetChild(1).gameObject;

        HideServerMenu();
        PlayerRegistrationPanel.SetActive(false);
    }
    private void Start()
    {
        ConnectButton.Interactable = false;
        ServerPagesDict = new Dictionary<GameObject, ServersPage>();


        GameObject InternetPage = ServerPageHolder.transform.GetChild(0).gameObject;
        InternetServersPage = InternetPage.GetComponent<ServersPage>();
        GameObject LANPage = ServerPageHolder.transform.GetChild(1).gameObject;
        LANServersPage = LANPage.GetComponent<ServersPage>();

        ServerPagesDict.Add(InternetPage, InternetServersPage);
        ServerPagesDict.Add(LANPage, LANServersPage);

        ManualEntryObject = ServerPageHolder.transform.GetChild(2).gameObject;
        CreateServerObject = ServerPageHolder.transform.GetChild(3).gameObject;

        ServerScanner = gameObject.GetComponent<LANServerScanner>();
    }

    private void LoadServersForSelectedServersPage()
    {
        if (SelectedManualEntry)
            return;

        if (SelectedServersPage == null) //Set default case
            SelectedServersPage = InternetServersPage;

        KeyServerFields.SetActive(true);
        if (SelectedServersPage == LANServersPage)
        {
            //TODO: Fix problem when a server is running on local machine, an error occurs, currently just
            //Adding localHost, however this would cause to miss other potential Servers found in LAN
            StartCoroutine(ServerScanner.GetLANServerAddressUDPBroadcast(Client.PortNumDiscover));
        }
        else //SelectedServerPage is InterntServersPage
        {

        }

    }
    private void AddServerToPage(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping, string ip) =>
        SelectedServersPage.EnqueEntry(serverName, currentPlayerCount, maxPlayerCount, mapName, ping, ip);

    
}


//TODO: make so Client.TCP Inherits from lesser version of this one
public class DiscoveryTCPClient
{
    public TcpClient Socket { get; private set; }
    private byte[] ReceiveBuffer;
    private NetworkStream Stream;
    private Packet ReceivePacket;

    private int DataBufferSize = 4096;

    private delegate void PacketHandler(string ip, Packet packet);
    private Dictionary<int, PacketHandler> PacketHandlerDictionary { get; set; } = new Dictionary<int, PacketHandler>();

    public DiscoveryTCPClient()
    {
        PacketHandlerDictionary.Add((int)DiscoveryServerPackets.serverData, ServerMenu.ReadServerDataPacket);
    }

    public void Connect(string ipAddressConnectTo, int portNum)
    {
        Socket = new TcpClient();
        Socket.ReceiveBufferSize = DataBufferSize;
        Socket.SendBufferSize = DataBufferSize;

        ReceiveBuffer = new byte[DataBufferSize];

        Socket.BeginConnect(ipAddressConnectTo, portNum, ConnectCallback, Socket);
        Debug.Log($"A DiscoveryTCPClient trying to connect to: {ipAddressConnectTo}...");
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
            Debug.Log($"Error, sending data to server from Client via TCP.\nException {exception}");
        }
    }

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
            Debug.Log($"\nError in BeginReadReceiveCallback...\nError: {exception}");
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
                string ipAddress = Socket.Client.RemoteEndPoint.ToString().Split(':')[0];
                PacketHandlerDictionary[packetID](ipAddress, packet);
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
    private void Disconnect()
    {
        Socket = null;
        Stream = null;
        ReceiveBuffer = null;
        ReceivePacket = null;
    }
}
