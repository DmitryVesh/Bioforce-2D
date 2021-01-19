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
    private static List<string> IPsWithOpenGamePort { get; set; }

    private static int PortNum { get; set; }
    private static int TimeOut { get; set; } = 1000;
    private static IPList IPList { get; set; }
    private static string LANIP { get; set; }

    public static DiscoveryClient DiscoveryClientManager { get; private set; } = new DiscoveryClient();

    //Calling UDP broadcast...
    public IEnumerator GetLANServerAddressUDPBroadcast(int portNum)
    {
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
    }
    public static List<string> GetIPsFromLANScan() =>
        DiscoveryClientManager.ServerAddresses;
    
    private void OnDestroy()
    {
        if (DiscoveryClientManager != null) 
            DiscoveryClientManager.CloseClient();
    }

    public class DiscoveryClient
    {
        public List<string> ServerAddresses { get; set; } = new List<string>(); // Addresses found on the LAN with a server

        private List<string> LocalFullAddresses { get; set; } = new List<string>(); // Addresses of player's machine
        private List<string> BroadcastAddresses { get; set; } = new List<string>(); // Broadcast addresses of the player's LAN connections

        private Socket BroadcastClientSocket { get; set; }
        private EndPoint BroadcastRemoteEndPoint; //Cant be property due to be used as a ref in BeginReceiveFrom

        private int NumberOfPings { get; set; } = 5;
        private float WaitBeforePings { get; set; } = 0.5f;
        private float ExtraTimeGivenForPingsToComplete { get; set; } = 5;

        public delegate void ServerFound(string serverName, int playerCount, string mapName, int ping);
        public event ServerFound OnServerFoundEvent;

        private List<DiscoveryTCP> DiscoveryTCPs { get; set; } = new List<DiscoveryTCP>();

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
                        ServerAddresses.Add("127.0.0.1");
                        return "127.0.0.1"; //Return localHost due to server running on local machine
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
            Debug.Log("Running scanning host...");
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

                        Debug.Log($"Local machine ip address: {unicastIPInfo.Address}");
                        BroadcastAddresses.Add(GetBroadcastAddress(unicastIPInfo));
                    }
                }
            }
            Debug.Log("Finished searching Network Interfaces");
        }

        private void AsyncCallbackBroadcastSocket(IAsyncResult result)
        {
            if (BroadcastClientSocket != null)
            {
                try
                {
                    int size = BroadcastClientSocket.EndReceiveFrom(result, ref BroadcastRemoteEndPoint);
                    string address = BroadcastRemoteEndPoint.ToString().Split(':')[0];
                    //Stable
                    //if (!LocalFullAddresses.Contains(address) && !ServerAddresses.Contains(address))
                    if (!ServerAddresses.Contains(address) && !LocalFullAddresses.Contains(address))
                    {
                        Debug.Log($"Got a server address: {address}");
                        ServerAddresses.Add(address);
                        //OnServerFoundEvent?.Invoke("Example", 10, "Jesus", 250);
                        DiscoveryTCP discoveryTCP = new DiscoveryTCP();
                        DiscoveryTCPs.Add(discoveryTCP);
                        discoveryTCP.Connect(address, PortNum);
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
        private static string GetBroadcastAddress(UnicastIPAddressInformation unicastAddress)
        {
            uint ipAddress = BitConverter.ToUInt32(unicastAddress.Address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(unicastAddress.IPv4Mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress)).ToString();
        }
    }

    //Trying to connect to every single ip in local network...
    //Works on Windows and Mac (sometimes doesn't work though), but not on iOS
    public static async Task<string> GetLANIPAddressConnectToScanAllIPs(int portNum)
    {
        (string lanIP, bool NullOrLocalHost) = GetLeadingLANIPAddress();
        if (NullOrLocalHost)
            return lanIP;

        LANIP = lanIP;
        PortNum = portNum;
        IPsWithOpenGamePort = new List<string>();
        IPList = new IPList(lowestIP: 1, highestIP: 254);
        int numberOfThreadsToUse = IPList.GetDifBetweenLowestAndHighestIP();

        Thread[] AllThreadsScanning = new Thread[numberOfThreadsToUse];

        for (int threadCount = 0; threadCount < numberOfThreadsToUse; threadCount++)
        {
            await Task.Run(() =>
            {
                Thread thread = new Thread(new ThreadStart(ScanIP));
                thread.Priority = System.Threading.ThreadPriority.AboveNormal;
                thread.Start();
                AllThreadsScanning[threadCount] = thread;
            });
        }
        foreach (Thread thread in AllThreadsScanning)
            thread.Join();

        Debug.Log("Finished LAN scanning execution");
        return GetIPConnectTo();
    }

    private static async void ScanIP()
    {
        int ipLastByte;
        while ((ipLastByte = IPList.GetNextIP()) != -1)
        {
            string ip = string.Concat(LANIP, ipLastByte);
            bool connected;
            try { connected = await Connect(ip, PortNum, TimeOut); }
            catch { continue; }

            if (connected)
                IPsWithOpenGamePort.Add(ip);
        }
    }
    private static async Task<bool> Connect(string ip, int portNum, int timeOut)
    {
        TcpClient tcp = new TcpClient();
        IsIPOpen tcpState = new IsIPOpen(tcp, true);

        IAsyncResult connectionResult = tcp.BeginConnect(ip, portNum, ConnectCallback, tcpState);
        tcpState.IsTCPOpen = await Task.Run(() => connectionResult.AsyncWaitHandle.WaitOne(timeOut, false));

        if (tcpState.IsTCPOpen == false || tcp.Connected == false)
        {
            Debug.Log("Sending false to ip " + ip);
            tcpState.TCP.Close();
            return false;
        }
        else 
        {
            Debug.Log("Sending true to ip " + ip);
            tcpState.TCP.Close();
            return true;
        }
    }
    private static void ConnectCallback(IAsyncResult asyncResult)
    {
        IsIPOpen currentState = (IsIPOpen)asyncResult.AsyncState;
        TcpClient tcp = currentState.TCP;

        try { tcp.EndConnect(asyncResult); }
        catch { return; }

        if (tcp.Connected && currentState.IsTCPOpen)
            return;

        tcp.Close();
    }
    private class IsIPOpen
    {
        public TcpClient TCP { get; private set; }
        public bool IsTCPOpen { get; set; }

        public IsIPOpen(TcpClient tcp, bool isTCPOpen) => 
            (TCP, IsTCPOpen) = (tcp, isTCPOpen);
    }

    private static string GetIPConnectTo()
    {
        string ipToConnectTo;
        if (IPsWithOpenGamePort.Count == 1)
        {
            Debug.Log("1 IP with open game port...");
            ipToConnectTo = IPsWithOpenGamePort[0];
        }
        else if (IPsWithOpenGamePort.Count == 0)
        {
            Debug.LogWarning("No ips with open game port...");
            ipToConnectTo = null;
        }
        else
        {
            ipToConnectTo = "many";
            Debug.LogWarning("Multiple ips with open game port...");
            foreach (string ip in IPsWithOpenGamePort)
            {
                Debug.Log($"\t{ip}");
            }
        }
        return ipToConnectTo;
    }
    private static (string, bool) GetLeadingLANIPAddress()
    {
        string ip = GetLocalIPAddress();
        if (ip == null || ip.StartsWith("127.0.0"))
            return (ip, true); 

        string[] splitLocalIP = ip.Split('.');
        return (string.Concat(splitLocalIP[0], '.', splitLocalIP[1], '.', splitLocalIP[2], '.'), false);
    }   
    private static string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        Debug.LogWarning("Couldn't find any hosts on LAN network...");
        return null;
    }
    
}
