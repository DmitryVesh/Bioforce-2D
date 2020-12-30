using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class LANServerScanner
{
    private static List<string> IPsWithOpenGamePort;
    private static bool[] IPsChecked;
    private static int PortNum;

    private static int SmallestFalseIPIndex;
    private static int TimeOut = 1000;
    private static List<Task> ScanPortAsyncCalls;

    public static async Task<string> GetLANIPAddress(int portNum)
    {
        PortNum = portNum;

        string localPlayerIPAddress = GetLocalIPAddress();
        if (localPlayerIPAddress == null)
            return null; //TODO: add no internet connection handler

        else if (localPlayerIPAddress.StartsWith("127.0.0"))
            return localPlayerIPAddress;

        string[] splitLocalIP = localPlayerIPAddress.Split('.');
        string LANIP = string.Concat(splitLocalIP[0], '.', splitLocalIP[1], '.', splitLocalIP[2], '.');

        ScanPortAsyncCalls = new List<Task>();
        IPsWithOpenGamePort = new List<string>();
        //IPsChecked = new bool[253];
        for (int ipCount = 1; ipCount < 254; ipCount++) //Start at 1, because 0 identifies network, doesn't reach 255, because 255 is broadcast 
        {
            string ip = string.Concat(LANIP, ipCount.ToString());
            ScanPortAsyncCalls.Add(Task.Run(() => ScanPortNew(ip, ipCount - 1)));
        }

        //SmallestFalseIPIndex = 0;
        //await AllIPsChecked();

        await Task.WhenAll(ScanPortAsyncCalls);

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

    private static void ScanPortNew(string ip, int index)
    {
        TcpClient scanner = new TcpClient();
        if (scanner.ConnectAsync(ip, PortNum).Wait(1000))
        {
            // connection successful
            IPsWithOpenGamePort.Add(ip);
            Debug.Log($"IP: {ip} is connected after 1 second");
        }
        scanner.Close();

    }
    private static void ScanPort(string ip, int index)
    {   
        try
        {
            using (TcpClient scanner = new TcpClient())
            {
                IAsyncResult connectionResult = scanner.BeginConnect(ip, PortNum, ConnectCallback, null);
                bool success = connectionResult.AsyncWaitHandle.WaitOne(TimeOut);
                //bool success = await Task.Run(() => connectionTask.AsyncWaitHandle.WaitOne(TimeOut));

                scanner.Close();
                if (success)
                {
                    IPsWithOpenGamePort.Add(ip);
                    Debug.Log($"IP: {ip} is connected after 1 second");
                }
                //IPsChecked[index] = true;
            }

        }
        catch (Exception exception)
        {
            //IPsChecked[index] = true;
            Debug.LogError($"Error in ScanPortAsync at IP: {ip}\n{exception}");
        }
    }
    private static void ConnectCallback(IAsyncResult asyncResult)
    {

    }
    private static async Task AllIPsChecked()
    {
        while (true)
        {
            Thread.Sleep(500);
            //Debug.Log("Calling while true loop...");
            bool allIPsChecked = await Task.Run(() =>
            {
                for (int ipIndex = SmallestFalseIPIndex; ipIndex < IPsChecked.Length; ipIndex++)
                {
                    if (!IPsChecked[ipIndex])
                    {
                        SmallestFalseIPIndex = ipIndex;
                        //Debug.Log($"IP: {ipIndex + 1} is false");
                        return false;
                    }
                }
                return true;
            });

            if (allIPsChecked)
                return;
        }
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
