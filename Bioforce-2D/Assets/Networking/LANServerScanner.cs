using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public static string AddressToConnecTo { get; private set; }
    public static DiscoveryClient BroadCastManager { get; private set; }

    //Calling UDP broadcast...
    public IEnumerator GetLANServerAddressUDPBroadcast(int portNum)
    {
        BroadCastManager = new DiscoveryClient();
        BroadCastManager.ScanHost();
        string address = BroadCastManager.StartClient(portNum);
        if (address != "waiting")
        {
            AddressToConnecTo = address;
            yield break;
        }

        StartCoroutine(BroadCastManager.SendPing(portNum));
        
        yield return new WaitForSeconds(BroadCastManager.GetTotalPingTime());
        BroadCastManager.PrintAllAddressesFound();
        BroadCastManager.CloseClient();
    }
    private void OnDestroy()
    {
        if (BroadCastManager != null) 
            BroadCastManager.CloseClient();
    }

    //Works on my router, but not on iOS hotspot, probably because of the subnet mask of iOS not being 255.255.255.0 but being 255.255.255.240
    //So have to find subnet mask, in order to show a correct broadcast address...
    public class DiscoveryClient
    {
        // Addresses of player's machine
        public List<string> LocalFullAddresses { get; private set; } = new List<string>();
        public List<string> LocalSubAddresses { get; private set; } = new List<string>();

        // Addresses found on the LAN with a server
        public List<string> ServerAddresses { get; private set; } = new List<string>();

        private Socket ClientSocket { get; set; }
        private EndPoint RemoteEndPoint; //Cant be property due to be used as a ref in BeginReceiveFrom

        private int NumberOfPings { get; set; } = 5;
        private float WaitBeforePings { get; set; } = 0.5f;
        private float ExtraTimeGivenForPingsToComplete { get; set; } = 5;

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
        public float GetTotalPingTime()
        {
            return (NumberOfPings * LocalSubAddresses.Count * WaitBeforePings) + ExtraTimeGivenForPingsToComplete;
        }

        public string StartClient(int port)
        {
            if (ClientSocket == null)
            {
                try
                {
                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    
                    if (ClientSocket == null)
                    {
                        Debug.Log("Client socket is null");
                        return null;
                    }
                    ClientSocket.Bind(new IPEndPoint(IPAddress.Any, port));
    
                    ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
    
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    
                    ClientSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackClient), null);
                    return "waiting";
                }
                catch (SocketException SocketException)
                {
                    CloseClient();
                    Debug.Log($"Error in making and binding the client socket\nA server is probably already running on machine: \n{SocketException}");
                    return LocalFullAddresses[0];
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
            if (ClientSocket != null)
            {
                ClientSocket.Close();
                ClientSocket = null;
            }
        }

        public IEnumerator SendPing(int port)
        {
            ServerAddresses.Clear();

            if (ClientSocket != null)
            {
                for (int i = 0; i < NumberOfPings; i++)
                {
                
                    foreach (string subAddress in LocalSubAddresses)
                    {
                        //TODO: get a broad cast address, because iOS hotspot has subnet 255.255.255.240, so broadcast address isdifferent
                        string ip = subAddress + ".255";
                        IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                        byte[] str = Encoding.ASCII.GetBytes("ping");
                        try
                        {
                            ClientSocket.SendTo(str, destinationEndPoint);
                        }
                        catch (Exception exception)
                        {
                            Debug.Log($"Error sending out a ping to: {ip}\n{exception}");
                        }
                        yield return new WaitForSeconds(WaitBeforePings);
                    }
                }
            }
        }
        private void AsyncCallbackClient(IAsyncResult result)
        {
            if (ClientSocket != null)
            {
                try
                {
                    int size = ClientSocket.EndReceiveFrom(result, ref RemoteEndPoint);
                    string address = RemoteEndPoint.ToString().Split(':')[0];
    
                    if (!LocalFullAddresses.Contains(address) && !ServerAddresses.Contains(address))
                    {
                        Debug.Log($"Got a server address: {address}");
                        AddressToConnecTo = address;
                        ServerAddresses.Add(address);
                    }
    
                    ClientSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackClient), null);
                }
                catch (Exception exception)
                {
                    Debug.Log(exception);
                }
            }
        }
    
        public void ScanHost()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
    
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    
                    string address = ip.ToString();
                    string subAddress = address.Remove(address.LastIndexOf('.'));
    
                    LocalFullAddresses.Add(address);
    
                    if (!LocalSubAddresses.Contains(subAddress))
                        LocalSubAddresses.Add(subAddress);
                    Debug.Log($"Local machine address: {address}");
                }
            }
            /*
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPInfo in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {

                    }
                }
            }
            */
        }
    }

    //Trying to connect to every single ip in local network...
    //Works on Windows and Mac, but not on iOS
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
