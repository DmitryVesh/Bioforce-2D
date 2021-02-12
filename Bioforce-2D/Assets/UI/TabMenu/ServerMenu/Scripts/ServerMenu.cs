using GameServer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance;
    public ServerEntry ServerEntryConnectTo { get; private set; } = null;
    public bool AskingForServers { get; set; }

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

    private LANServerScanner LANServerScanner { get; set; }

    [SerializeField] private GameObject ServerConnectionErrorPanel;
    [SerializeField] private TextMeshProUGUI ServerConnectionErrorText;

    

    //Packet handler methods
    internal static void ReadWelcomePacket(string ip, Packet packet)
    {
        string message = packet.ReadString();
        Debug.Log($"Connection with MainServer established.\nMessage from MainServer: {message}");
        if (Instance.AskingForServers)
            InternetServerScanner.SendFirstAskForServersPacket();
    }
    internal static void ReadServerDeletedPacket(string ip, Packet packet)
    {
        throw new NotImplementedException();
    }
    internal static void ReadServerModifiedPacket(string ip, Packet packet)
    {
        throw new NotImplementedException();
    }
    internal static void ServerConnectionFull()
    {
        Instance.ServerConnectionErrorText.text =
            "The Server you tried to connect to is Full..." +
            "\nPress Continue to proceed";
        Instance.ServerConnectionErrorPanel.SetActive(true);
        Client.Instance.Disconnect();
    }
    public static void ReadServerDataPacket(string serverIP, Packet packet)
    {
        string serverName = packet.ReadString();
        int currentPlayerCount = packet.ReadInt();
        int maxPlayerCount = packet.ReadInt();
        string mapName = packet.ReadString();
        int ping = packet.ReadInt(); //TODO: look into how to calculate ping

        if (serverIP == Client.InternetMainServerIP)
        {
            Instance.AddInternetServerToPage(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
        }
        else
            Instance.AddLANServerToPage(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
    }
    internal static void ReadJoinServer(string ip, Packet packet)
    {
        
        string serverIP = packet.ReadString();
        int serverPort = packet.ReadInt();
        Debug.Log($"Read JoinServer Packet, Server: {serverIP}:{serverPort}");

        Client.Instance.ConnectToServer(serverIP, serverPort);
    }

    internal static void ReadNoMoreServersAvailable(string ip, Packet packet)
    {
        throw new NotImplementedException();
    }
    internal static void ReadCantJoinServerDeleted(string ip, Packet packet)
    {
        throw new NotImplementedException();
    }

    public static void DisconnectedMainServer()
    {
        Instance.ServerConnectionErrorText.text =
            "Lost connection to Main Server..." +
            "\nCheck your Internet connection" +
            "\n\nPress Continue to proceed";

        Instance.ServerConnectionErrorPanel.SetActive(true);
    }
    public static void TimedOutMainServer()
    {
        Instance.ServerConnectionErrorText.text =
            "Connection to Main Server Timed Out..." +
            "\nCheck your Internet connection" +
            "\n\nPress Continue to proceed";

        Instance.ServerConnectionErrorPanel.SetActive(true);
        Instance.ServerMenuPanel.SetActive(false);
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
        Instance.ServerConnectionErrorText.text =
            "Connection to selected Server timed out..." +
            "\n\nPress Continue to proceed";
        Instance.ServerConnectionErrorPanel.SetActive(true);
        Instance.ServerMenuPanel.SetActive(false);
    }
    public static void Disconnected()
    {
        if (Instance != null)
            Instance.ServerMenuPanel.SetActive(true);
    }

    public void OnServerConnectionErrorPanelContinuePressed()
    {
        ServerConnectionErrorPanel.SetActive(false);
        ServerMenuPanel.SetActive(true);
    }
    public void OnConnectButtonPressed()
    {        
        ServerMenuPanel.SetActive(false);
        //TODO: Deal when the player count is full, e.g. 10/10 players
        if (SelectedManualEntry) //TODO: Make so Discovery tcp client is made to check out the ip address
            throw new NotImplementedException();
        else
        {
            //Send packet to MainServer that you want to join a specific server
            InternetServerScanner.SendJoinServerPacket(Client.PortNumInternetDiscover, ServerEntryConnectTo.ServerName);
        }
            
    }
    public void ShowServerMenu()
    {
        if (!PlayerPrefs.HasKey("Username"))
            DisplayUserRegistration();
        else
        {
            ServerMenuPanel.SetActive(true);
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
        ServerConnectionErrorPanel.SetActive(false);
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

        LANServerScanner = gameObject.GetComponent<LANServerScanner>();
    }

    

    private void LoadServersForSelectedServersPage()
    {
        if (SelectedManualEntry)
            return;

        if (SelectedServersPage == null) //Set default case
            SelectedServersPage = InternetServersPage;

        KeyServerFields.SetActive(true);
        if (SelectedServersPage == LANServersPage)
            StartCoroutine(LANServerScanner.GetLANServerAddressUDPBroadcast(Client.PortNumLANDiscover));
        else
            InternetServerScanner.ContactMainServerForServers(Client.PortNumInternetDiscover);

    }
    private void AddLANServerToPage(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) =>
        LANServersPage.EnqueEntry(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
    private void AddInternetServerToPage(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) =>
        InternetServersPage.EnqueEntry(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);

}