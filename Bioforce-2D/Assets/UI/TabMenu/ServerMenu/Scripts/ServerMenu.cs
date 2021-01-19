using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance;

    private GameObject ServerMenuPanel { get; set; }
    private GameObject PlayerRegistrationPanel { get; set; }

    [SerializeField] private MenuButton ConnectButton;

    [SerializeField] private GameObject ServerPageHolder;
    private ServersPage SelectedServersPage { get; set; } = null;
    private Dictionary<GameObject, ServersPage> ServerPagesDict { get; set; }

    //All the server pages
    private ServersPage InternetServersPage { get; set; }
    private ServersPage LANServersPage { get; set; }

    private LANServerScanner ServerScanner { get; set; }

    public static void ReadServerDataPacket(Packet packet)
    {
        string serverName = packet.ReadString();
        int playerCount = packet.ReadInt();
        string mapName = packet.ReadString();
        int ping = packet.ReadInt(); //TODO: look into how to calculate ping

        Instance.AddServerToPage(serverName, playerCount, mapName, ping);
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
        SelectedServersPage = ServerPagesDict[selectedPage];
        LoadServersForSelectedServersPage();
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
        int pages = ServerPageHolder.transform.childCount;
        ServerPagesDict = new Dictionary<GameObject, ServersPage>(pages);

        GameObject InternetPage = ServerPageHolder.transform.GetChild(0).gameObject;
        InternetServersPage = InternetPage.GetComponent<ServersPage>();

        GameObject LANPage = ServerPageHolder.transform.GetChild(1).gameObject;
        LANServersPage = LANPage.GetComponent<ServersPage>();

        ServerPagesDict.Add(InternetPage, InternetServersPage);
        ServerPagesDict.Add(LANPage, LANServersPage);

        LANServerScanner.DiscoveryClientManager.OnServerFoundEvent += AddServerToPage;
        ServerScanner = gameObject.AddComponent<LANServerScanner>();
    }

    private void LoadServersForSelectedServersPage()
    {
        if (SelectedServersPage == null)
            SelectedServersPage = InternetServersPage;

        if (SelectedServersPage == LANServersPage)
        {
            //TODO: Fix problem when a server is running on local machine, an error occurs, currently just
            //Adding localHost, however this would cause to miss other potential Servers found in LAN
            StartCoroutine(ServerScanner.GetLANServerAddressUDPBroadcast(Client.PortNumDiscover));
        }

    }
    private void AddServerToPage(string serverName, int playerCount, string mapName, int ping) =>
        SelectedServersPage.EnqueEntry(serverName, playerCount, mapName, ping);
}

public enum DiscoveryServerPackets
{
    serverData
}
//TODO: make so Client.TCP Inherits from lesser version of this one
public class DiscoveryTCP
{
    public TcpClient Socket { get; private set; }
    private byte[] ReceiveBuffer;
    private NetworkStream Stream;
    private Packet ReceivePacket;

    private int DataBufferSize = 4096;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> PacketHandlerDictionary { get; set; } = new Dictionary<int, PacketHandler>();

    public DiscoveryTCP()
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
