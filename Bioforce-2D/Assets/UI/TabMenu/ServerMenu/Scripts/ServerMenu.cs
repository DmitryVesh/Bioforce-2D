using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;

public class ServerMenu : MonoBehaviour
{
    public static ServerMenu Instance { get => instance; }
    private static ServerMenu instance;

    public ServerEntry ServerEntryConnectTo { get; private set; } = null;
    public bool AskingForServers { get; set; }

    private GameObject ServerMenuPanel { get; set; }
    private GameObject PlayerRegistrationPanel { get; set; }

    [SerializeField] private MenuButton ConnectButton;    

    [SerializeField] private GameObject ServerPageHolder;
    [SerializeField] private GameObject KeyServerFields;

    //Server pages & Manual Entries
    public ServersPage SelectedServersPage { get; private set; } = null;
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
        string gameVersionLatest = packet.ReadString();
        Output.WriteLine($"Connection with MainServer established.\nMessage from MainServer: {message}" +
            $"\nLatest GameVersion: {gameVersionLatest}");

        if (!VersionCompatibility.Instance.DoGameVersionsMatch(gameVersionLatest))
        {
            VersionCompatibility.Instance.DisplayPanel();
            InternetServerScanner.Instance.Disconnect();
            return;
        }

        if (Instance.AskingForServers)
            InternetServerScanner.SendFirstAskForServersPacket();
        Instance.InternetServersPage.RegainedConnectionToMainServer();
    }
    internal static void ReadServerDeletedPacket(string ip, Packet packet)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            string serverName = packet.ReadString();
            Instance.InternetServersPage.DeleteServer(serverName);
        });
    }
    internal static void ReadServerModifiedPacket(string ip, Packet packet)
    {
        string serverName = packet.ReadString();
        int currentNumPlayers = packet.ReadInt();
        int maxNumPlayers = packet.ReadInt();
        string mapName = packet.ReadString();
        int ping = packet.ReadInt();

        ThreadManager.ExecuteOnMainThread(() =>
        {
            Instance.InternetServersPage.ModifyServer(serverName, currentNumPlayers, maxNumPlayers, mapName, ping);
        });
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

        if (serverIP == Client.MainServerIP)
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
        Output.WriteLine($"Read JoinServer Packet, Server: {serverIP}:{serverPort}");

        Client.Instance.ConnectToServer(serverIP, serverPort);
    }

    internal static void ReadNoMoreServersAvailable(string ip, Packet packet)
    {
        Instance.ServerConnectionErrorText.text =
            "No more Servers can be Created..." +
            "\nJoin Existing Servers" +
            "\nPress Continue to proceed";
        Instance.ServerConnectionErrorPanel.SetActive(true);
    }
    internal static void ReadCantJoinServerDeleted(string ip, Packet packet)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            string serverName = packet.ReadString();
            Instance.InternetServersPage.DeleteServer(serverName);
            ServerConnectionTimeOut();
        });
    }

    public static void DisconnectedMainServer()
    {
        Instance.InternetServersPage.LostConnectionToMainServer();   
    }
    public static void TimedOutMainServer()
    {
        Instance.ServerConnectionErrorPanel.SetActive(true);
        Instance.ServerMenuPanel.SetActive(false);
        Instance.ServerConnectionErrorText.text =
            "Connection to Main Server Timed Out..." +
            "\nCheck your Internet connection" +
            "\n\nPress Continue to proceed";
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

    public void OnServerConnectionErrorPanelContinuePressed()
    {
        ServerConnectionErrorPanel.SetActive(false);
        ServerMenuPanel.SetActive(true);
    }
    public void OnConnectButtonPressed()
    {        
        ServerMenuPanel.SetActive(false);
        if (SelectedManualEntry)
            ServerConnectionTimeOut();
        else
        {
            //Send packet to MainServer that you want to join a specific server
            InternetServerScanner.SendJoinServerPacket(Client.PortNumInternetToConnectTo, ServerEntryConnectTo.ServerName);
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
        Singleton.Init(ref instance, this);

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

        SelectedServersPage = InternetServersPage;
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
            InternetServerScanner.ContactMainServerForServers(Client.PortNumInternetToConnectTo);

    }
    private void AddLANServerToPage(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) =>
        LANServersPage.EnqueEntry(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);
    private void AddInternetServerToPage(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping) =>
        InternetServersPage.EnqueEntry(serverName, currentPlayerCount, maxPlayerCount, mapName, ping);

}