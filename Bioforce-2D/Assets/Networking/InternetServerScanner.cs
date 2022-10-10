using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEngine.Output;
using UnityEngine.Singleton;
using Shared;

public class InternetServerScanner : MonoBehaviour
{
    public static InternetServerScanner Instance { get => instance; }
    private static InternetServerScanner instance;

    private InternetDiscoveryClient MainServerSocket { get; set; }

    private const double ReAskTimerMax = 5000d;
    private TimeSpan TwoSecondsTimeSpan { get; } = TimeSpan.FromSeconds(2d);
    private TimeSpan LastTimeAskedForServers { get; set; }
    private Timer AutoReAskTimer { get; set; }

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
            Output.WriteLine($"Error contacting MainServer to add own server.\n{exception}");
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
            Instance.MainServerSocket.Connect(Client.MainServerIPStatic, port);
        }
        
        catch (Exception exception)
        {
            Output.WriteLine($"Error in contacting MainServer...\n{exception}");
            Instance.MainServerSocket.Disconnect(false, true);
        }
    }

    public void Disconnect()
    {
        if (MainServerSocket != null)
            MainServerSocket.Disconnect(false, false);
        MainServerSocket = null;
    }

    //Sending Packets
    internal static void SendFirstAskForServersPacket()
    {
        //2.
        using (Packet packet = new Packet())
        {
            packet.Write((byte)InternetDiscoveryClientPackets.firstAskForServers);
            Instance.MainServerSocket.SendPacket(packet);
        }
        Output.WriteLine("Sent FirstAskForServers Packet to MainServer");

        //4.
        Instance.AutoReAskTimer = new Timer(ReAskTimerMax);
        Instance.AutoReAskTimer.Elapsed += AutoReAskTimer_Elapsed;

        Instance.ResetReAskTimer(true);
    }
    
    internal IEnumerator SendAddServerPacket(string serverName, int maxNumPlayers, string mapName)
    {
        yield return new WaitForSeconds(0.5f);
        Output.WriteLine($"Sent Add Server Packet: {serverName}");
        using (Packet packet = new Packet())
        {
            packet.Write((byte)InternetDiscoveryClientPackets.addServer);
            packet.Write(serverName);
            packet.Write(maxNumPlayers);
            packet.Write(mapName);
            packet.Write(1);
            packet.Write(10); //TODO: calculate actualy ping

            Instance.MainServerSocket.SendPacket(packet);
        }
        //Now wait for 
    }
    public void UserAskForServerChanges()
    {
        //if there is no connection between MainServer and client
        //Get into contact with the MainServer
        if (MainServerSocket is null || !MainServerSocket.IsConnected())
        {
            ContactMainServerForServers(Client.PortNumInternetToConnectTo);
            return;
        }

        //if last time asked was greater than 2 seconds ago
        //then ask again
        TimeSpan now = DateTime.Now.TimeOfDay;
        if (now - LastTimeAskedForServers > TwoSecondsTimeSpan)
        {
            LastTimeAskedForServers = now;
            SendAskForServerChangesPacket();
        }
    }
    private void SendAskForServerChangesPacket()
    {
        using (Packet packet = new Packet())
        {
            packet.Write((byte)InternetDiscoveryClientPackets.askForServerChanges);
            MainServerSocket?.SendPacket(packet);
        }
    }
    internal static void SendJoinServerPacket(int port, string serverName)
    {
        try
        {
            StartMainServerSocket(port);
            using (Packet packet = new Packet((byte)InternetDiscoveryClientPackets.joinServerNamed))
            {
                packet.Write(serverName);
                Instance.MainServerSocket.SendPacket(packet);
            }
        }
        catch (Exception exception)
        {
            Output.WriteLine($"Error SendJoinServerPacket to server: {serverName}...\n{exception}");
        }
    }
    private static void AutoReAskTimer_Elapsed(object sender, ElapsedEventArgs e) =>
        Instance.UserAskForServerChanges();
    private void ResetReAskTimer(bool shouldRun)
    {
        if (shouldRun)
            AutoReAskTimer?.Start();
        else
            AutoReAskTimer?.Stop();
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

    private void Awake()
    {
        Singleton.Init(ref instance, this);
    }

    private void OnDestroy()
    {
        //5.2
        Disconnect();
        ResetReAskTimer(false);
    }

}
