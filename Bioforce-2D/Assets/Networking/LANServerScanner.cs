using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public  class LANServerScanner : MonoBehaviour
{
    private static int PortNum { get; set; }

    public static DiscoveryUDPClient DiscoveryClientManager { get; private set; } = new DiscoveryUDPClient();
    private bool AskedAlready { get; set; }

    //Calling UDP broadcast...
    public IEnumerator GetLANServerAddressUDPBroadcast(int portNum)
    {
        if (AskedAlready)
            yield break;
        AskedAlready = true;

        PortNum = portNum;
        DiscoveryClientManager.ScanHost();
        string address = DiscoveryClientManager.StartClient(portNum);

        if (address != "waiting")
        {
            DiscoveryClientManager.CloseClient();
            yield break;   
        }

        StartCoroutine(DiscoveryClientManager.SendPing(portNum));
        
        yield return new WaitForSeconds(DiscoveryClientManager.GetTotalPingTime());
        DiscoveryClientManager.PrintAllAddressesFound();
        DiscoveryClientManager.CloseClient();

        AskedAlready = false;
    }
    private void OnDestroy()
    {
        if (DiscoveryClientManager != null) 
            DiscoveryClientManager.CloseClient();
    }

    public class DiscoveryUDPClient
    {
        public List<string> ServerAddresses { get; set; } = new List<string>(); // IP Addresses found on the LAN with a server

        private List<string> LocalFullAddresses { get; set; } = new List<string>(); // Addresses of player's machine
        private List<string> BroadcastAddresses { get; set; } = new List<string>(); // Broadcast addresses of the player's LAN connections

        private Socket BroadcastClientSocket { get; set; }
        private EndPoint BroadcastRemoteEndPoint; //Cant be property due to be used as a ref in BeginReceiveFrom

        private int NumberOfPings { get; set; } = 5;
        private float WaitBeforePings { get; set; } = 0.5f;
        private float ExtraTimeGivenForPingsToComplete { get; set; } = 5;

        public delegate void ServerFound(string serverName, int currentPlayerCount, int maxPlayerCount, string mapName, int ping);

        private List<DiscoveryTCPClient> DiscoveryTCPs { get; set; } = new List<DiscoveryTCPClient>();

        public void PrintAllAddressesFound()
        {
            if (ServerAddresses.Count == 0)
            {
                Debug.Log("No addresses found....");
                return;
            }

            Debug.Log("Addresses found:");
            foreach (string address in ServerAddresses)
                Debug.Log(address);
        }
        public float GetTotalPingTime() =>
            (NumberOfPings * BroadcastAddresses.Count * WaitBeforePings) + ExtraTimeGivenForPingsToComplete;

        public string StartClient(int port)
        {
            if (BroadcastClientSocket == null)
            {
                try
                {
                    BroadcastClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    
                    if (BroadcastClientSocket == null)
                    {
                        Debug.Log("Client socket is null");
                        return null;
                    }
                    BroadcastClientSocket.Bind(new IPEndPoint(IPAddress.Any, port));
    
                    BroadcastClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    BroadcastClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
    
                    BroadcastRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    
                    BroadcastClientSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref BroadcastRemoteEndPoint, new AsyncCallback(AsyncCallbackBroadcastSocket), null);
                    return "waiting";
                }
                catch (SocketException SocketException)
                {
                    if (SocketException.SocketErrorCode.Equals(SocketError.AddressAlreadyInUse))
                    {
                        Debug.Log($"Error in making and binding the client socket\nA server is already running on machine: \n{SocketException}");
                        CloseClient();
                        string localHostIP = "127.0.0.1";
                        AddNewClient(localHostIP);
                        return localHostIP; //Return localHost due to server running on local machine
                    }
                    Debug.Log($"Unexpected SocketException in Start Client:\n{SocketException}");
                    return null;
                }
                catch (Exception exception)
                {
                    Debug.Log($"Unexpected error in Starting client:\n{exception}");
                    return null;
                }
            }
            return null;
        }
        public void CloseClient()
        {
            if (BroadcastClientSocket != null)
            {
                BroadcastClientSocket.Close();
                BroadcastClientSocket = null;
                BroadcastRemoteEndPoint = null;
            }
        }

        public IEnumerator SendPing(int port)
        {
            ServerAddresses.Clear();

            if (BroadcastClientSocket != null)
            {
                for (int i = 0; i < NumberOfPings; i++)
                {
                    foreach (string ipAddress in BroadcastAddresses)
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                        byte[] str = Encoding.ASCII.GetBytes("ping");
                        try
                        {
                            BroadcastClientSocket.SendTo(str, ipEndPoint);
                        }
                        catch (Exception exception)
                        {
                            Debug.Log($"Error sending out a ping to: {ipEndPoint}\n{exception}");
                        }
                        yield return new WaitForSeconds(WaitBeforePings);
                    }
                }
            }
        }
        public void ScanHost()
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!adapter.OperationalStatus.Equals(OperationalStatus.Up))
                    continue;
                foreach (UnicastIPAddressInformation unicastIPInfo in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string localAddress = unicastIPInfo.Address.ToString();
                        if (!LocalFullAddresses.Contains(localAddress))
                            LocalFullAddresses.Add(localAddress);

                        string broadcastAddress = GetBroadcastAddress(unicastIPInfo);
                        if (BroadcastAddresses.Contains(broadcastAddress))
                            continue;

                        Debug.Log($"Local machine ip address: {unicastIPInfo.Address}, {unicastIPInfo.IPv4Mask}, broadCast address: {broadcastAddress}");
                        BroadcastAddresses.Add(broadcastAddress);
                    }
                }
            }
            Debug.Log("Finished searching local network addresses.");
        }

        private void AsyncCallbackBroadcastSocket(IAsyncResult result)
        {
            if (BroadcastClientSocket != null)
            {
                try
                {
                    int size = BroadcastClientSocket.EndReceiveFrom(result, ref BroadcastRemoteEndPoint);
                    string address = BroadcastRemoteEndPoint.ToString().Split(':')[0];
                    
                    if (!ServerAddresses.Contains(address) && !LocalFullAddresses.Contains(address))
                    {
                        Debug.Log($"Got a server address: {address}");
                        AddNewClient(address);
                    }

                    //Have to keep listening on the same BroadCast socket, due to other broadcast addresses may reply
                    BroadcastClientSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref BroadcastRemoteEndPoint, new AsyncCallback(AsyncCallbackBroadcastSocket), null);
                }
                catch (Exception exception)
                {
                    Debug.Log($"Error in AsyncCallback of Broadcast socket:\n{exception}");
                }
            }
        }

        private void AddNewClient(string address)
        {
            ServerAddresses.Add(address);
            LANDiscoveryClient discoveryTCP = new LANDiscoveryClient();
            DiscoveryTCPs.Add(discoveryTCP);
            discoveryTCP.Connect(address, PortNum);
        }

        private static string GetBroadcastAddress(UnicastIPAddressInformation unicastAddress)
        {
            uint ipAddress = BitConverter.ToUInt32(unicastAddress.Address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(unicastAddress.IPv4Mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress)).ToString();
        }
    }
}
