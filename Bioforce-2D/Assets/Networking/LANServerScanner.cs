using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class LANServerScanner
{
    private static List<string> IPsWithOpenGamePort { get; set; }

    private static int PortNum { get; set; }
    private static int TimeOut { get; set; } = 2000;
    private static IPList IPList { get; set; }
    private static string LANIP { get; set; }

    public static async Task<string> GetLANIPAddressConnectTo(int portNum)
    {
        (string lanIP, bool NullOrLocalHost) = GetLeadingLANIPAddress();
        if (NullOrLocalHost)
            return lanIP;

        LANIP = lanIP;
        PortNum = portNum;
        IPsWithOpenGamePort = new List<string>();
        IPList = new IPList(lowestIP: 1, highestIP: 254);
        int numberOfThreadsToUse = IPList.GetDifBetweenLowestAndHighestIP();

        Thread[] AllThreads = new Thread[numberOfThreadsToUse];

        for (int threadCount = 0; threadCount < numberOfThreadsToUse; threadCount++)
        {
            await Task.Run(() =>
            {
                Thread thread = new Thread(new ThreadStart(ScanIP));
                thread.Start();
                AllThreads[threadCount] = thread;
            });
        }

        for (int threadCount = 0; threadCount < numberOfThreadsToUse; threadCount++)
        {
            AllThreads[threadCount].Join();
        }
        Debug.Log("Finished execution");
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
        tcpState.IsTCPOpen = await Task.Run(() =>  connectionResult.AsyncWaitHandle.WaitOne(timeOut, false));

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
