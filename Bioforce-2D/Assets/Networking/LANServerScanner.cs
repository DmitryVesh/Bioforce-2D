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

    //Calling UDP broadcast...
    public IEnumerator GetLANServerAddressUDPBroadcast(int portNum)
    {
        //return await OldBroadcast(portNum);
        LanManager lanManager = new LanManager();
        lanManager.ScanHost();
        string address = lanManager.StartClient(portNum);
        if (address != "waiting")
            AddressToConnecTo = null;

        StartCoroutine(lanManager.SendPing(portNum));
        lanManager.PrintAllAddressesFound();
        //lanManager.CloseClient();
        yield return new WaitForSeconds(10);
    }

    //Works on my router, but not on iOS hotspot, probably because of the subnet mask of iOS not being 255.255.255.0 but being 255.255.255.240
    //So have to find subnet mask, in order to show a correct broadcast address...
    public class LanManager
    {
        // Addresses of player's machine
        public List<string> LocalFullAddresses { get; private set; }
        public List<string> LocalSubAddresses { get; private set; }
    
        // Addresses found on the LAN with a server
        public List<string> ServerAddresses { get; private set; }
    
        private Socket ClientSocket { get; set; }
        private EndPoint RemoteEndPoint; //Cant be property due to be used as a ref in BeginReceiveFrom
    
        public LanManager()
        {
            ServerAddresses = new List<string>();
            LocalFullAddresses = new List<string>();
            LocalSubAddresses = new List<string>();
        }
        public void PrintAllAddressesFound()
        {
            if (ServerAddresses.Count == 0)
                Console.WriteLine("No addresses found....");
            foreach (string address in ServerAddresses)
                Console.WriteLine(address);
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
                        Console.WriteLine("Client socket is null");
                        return null;
                    }
    
                    ClientSocket.Bind(new IPEndPoint(IPAddress.Any, port));
    
                    ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
    
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    
                    ClientSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackClient), null);
                    return "waiting";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return LocalFullAddresses[0];
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
                int numberOfPings = 4;

                for (int i = 0; i < numberOfPings; i++)
                {
                    foreach (string subAddress in LocalSubAddresses)
                    {
                        //TODO: get a broad cast address, because iOS hotspot has subnet 255.255.255.240, so broadcast address is different
                        IPEndPoint destinationEndPoint = new IPEndPoint(IPAddress.Parse(subAddress + ".255"), port);
                        byte[] str = Encoding.ASCII.GetBytes("ping");

                        ClientSocket.SendTo(str, destinationEndPoint);

                        yield return new WaitForSeconds(1);
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
