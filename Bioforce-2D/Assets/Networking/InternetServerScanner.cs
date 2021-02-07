using System;
using UnityEngine;

public class InternetServerScanner : MonoBehaviour
{
    public static InternetServerScanner Instance { get; set; }
    private InternetDiscoveryClient MainServerSocket { get; set; }
    private bool ReAskTimerIsOn { get; set; }
    private const float ReAskTimerMax = 15f;
    private float ReAskTimerCurrent { get; set; }

    public static void ContactMainServerForServers(int port)
    {
        //1. Make TCP connection with MainServer
        //2. Ask MainServer for servers, after receiving welcome packet from MainServer
        //3. Forward servers to Internet Servers Page - InternetDiscoveryClient will handle InternetDiscoveryServerPackets
        //4. Ask every 15 seconds, for changes on list of servers, either if added or deleted
        //5. Maintain connection, until either: 
        //                                      5.1 loaded into a game, 
        //                                      5.2 onDestroy, 
        //                                      5.3 lost connection to MainServer
        ServerMenu.Instance.AskingForServers = true;
        StartMainServerSocket(port);

        //4.
        Instance.ResetReAskTimer(true);
    }
    public static void ContactMainServerToAddOwnServer(int port)
    {
        ServerMenu.Instance.AskingForServers = false;
        StartMainServerSocket(port);
    }
    private static void StartMainServerSocket(int port)
    {
        try
        {
            if (Instance.MainServerSocket != null) //Already communicating with MainServer for servers
                return;

            Instance.MainServerSocket = new InternetDiscoveryClient();
            Instance.MainServerSocket.OnDisconnectAction += Instance.OnMainServerSocketDisconnected;

            //1.
            Instance.MainServerSocket.Connect(Client.InternetMainServerIP, port);
        }
        catch (Exception exception)
        {
            Debug.Log($"Error in contacting MainServer...\n{exception}");
            Instance.MainServerSocket.Disconnect();
        }
    }

    //Sending Packets
    internal static void SendFirstAskForServersPacket()
    {
        //2.
        using (Packet packet = new Packet())
        {
            packet.Write((int)InternetDiscoveryClientPackets.firstAskForServers);
            Instance.MainServerSocket.SendPacket(packet);
        }
    }
    
    internal static void SendAddServerPacket()
    {
        using (Packet packet = new Packet())
        {
            packet.Write((int)InternetDiscoveryClientPackets.addServer);
            packet.Write(GameServer.Server.ServerName);
            packet.Write(GameServer.Server.MaxNumPlayers);
            packet.Write(GameServer.Server.MapName);
            packet.Write(GameServer.Server.GetCurrentNumPlayers());
            packet.Write(10); //TODO: calculate actualy ping
            Instance.MainServerSocket.SendPacket(packet);
        }
    }
    private void SendAskForServerChangesPacket()
    {
        using (Packet packet = new Packet())
        {
            packet.Write((int)InternetDiscoveryClientPackets.askForServerChanges);
            MainServerSocket.SendPacket(packet);
        }
    }


    //5.3
    private void OnMainServerSocketDisconnected()
    {
        //TODO: Display error message when communication with the MainServer stopped
        ResetReAskTimer(false);
        MainServerSocket = null;
        throw new NotImplementedException("Display error message when communication with the MainServer stopped");
    }
    private void ResetReAskTimer(bool shouldRun)
    {
        ReAskTimerIsOn = shouldRun;
        ReAskTimerCurrent = ReAskTimerMax;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.Log($"InternetSeverScanner instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
    }
    private void FixedUpdate()
    {
        if (!ReAskTimerIsOn)
            return;

        //4.
        ReAskTimerCurrent -= Time.fixedDeltaTime;
        if (ReAskTimerCurrent < 0)
        {
            ResetReAskTimer(true);
            SendAskForServerChangesPacket();
        }
    }

    private void OnDestroy()
    {
        //5.2
        if (MainServerSocket != null)
            MainServerSocket.Disconnect();
        MainServerSocket = null;
        ResetReAskTimer(false);
    }

}
