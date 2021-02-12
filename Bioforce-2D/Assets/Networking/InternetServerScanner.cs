using System;
using System.Collections;
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
    public bool ContactMainServerToAddOwnServer(string serverName, int maxNumPlayers, string mapName, int port)
    {
        try
        {
            ServerMenu.Instance.AskingForServers = false;
            StartMainServerSocket(port);
            StartCoroutine(Instance.SendAddServerPacket(serverName, maxNumPlayers, mapName));
            return true;
        }
        catch (Exception exception)
        {
            Debug.Log($"Error contacting MainServer to add own server.\n{exception}");
            return false;
        }
    }
    private static void StartMainServerSocket(int port)
    {
        try
        {
            if (Instance.MainServerSocket != null) //Already communicating with MainServer for servers
                return;

            Instance.MainServerSocket = new InternetDiscoveryClient();
            Instance.MainServerSocket.OnDisconnectAction += Instance.OnMainServerSocketDisconnected;
            Instance.MainServerSocket.OnTimedOutAction += Instance.OnMainServerSocketTimedOut;
            //1.
            Instance.MainServerSocket.Connect(Client.InternetMainServerIP, port);
        }
        catch (Exception exception)
        {
            Debug.Log($"Error in contacting MainServer...\n{exception}");
            Instance.MainServerSocket.Disconnect(false, true);
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
        Debug.Log("Sent FirstAskForServers Packet to MainServer");
    }
    
    internal IEnumerator SendAddServerPacket(string serverName, int maxNumPlayers, string mapName)
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"Sent Add Server Packet: {serverName}");
        using (Packet packet = new Packet())
        {
            packet.Write((int)InternetDiscoveryClientPackets.addServer);
            packet.Write(serverName);
            packet.Write(maxNumPlayers);
            packet.Write(mapName);
            packet.Write(1);
            packet.Write(10); //TODO: calculate actualy ping

            Instance.MainServerSocket.SendPacket(packet);
        }
        //Now wait for 
    }
    private void SendAskForServerChangesPacket()
    {
        using (Packet packet = new Packet())
        {
            packet.Write((int)InternetDiscoveryClientPackets.askForServerChanges);
            MainServerSocket.SendPacket(packet);
        }
    }
    internal static void SendJoinServerPacket(int port, string serverName)
    {
        try
        {
            StartMainServerSocket(port);
            using (Packet packet = new Packet((int)InternetDiscoveryClientPackets.joinServerNamed))
            {
                packet.Write(serverName);
                Instance.MainServerSocket.SendPacket(packet);
            }
        }
        catch (Exception exception)
        {
            Debug.Log($"Error SendJoinServerPacket to server: {serverName}...\n{exception}");
        }
    }


    //5.3
    private void OnMainServerSocketDisconnected()
    {
        ResetReAskTimer(false);
        MainServerSocket = null;
        ServerMenu.DisconnectedMainServer();
    }
    private void OnMainServerSocketTimedOut()
    {
        ResetReAskTimer(false);
        MainServerSocket = null;
        ServerMenu.TimedOutMainServer();
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
            Destroy(gameObject);
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
            MainServerSocket.Disconnect(false, false);
        MainServerSocket = null;
        ResetReAskTimer(false);
    }

    
}
