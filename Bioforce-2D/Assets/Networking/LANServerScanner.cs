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

        IPsWithOpenGamePort = new List<string>();
        IPsChecked = new bool[254];
        for (int ipCount = 1; ipCount < 255; ipCount++) //Start at 1, because 0 identifies network, doesn't reach 255, because 255 is broadcast 
        {
            string ip = string.Concat(LANIP, ipCount.ToString());
            ScanPortAsync(ip, ipCount - 1);
        }
        await AllIPsChecked();

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
    
    private static async void ScanPortAsync(string ip, int index)
    {
        using (TcpClient scanner = new TcpClient())
        {
            IAsyncResult connectionResult = scanner.ConnectAsync(ip, PortNum);
            await Task.Run(() =>
            {
                connectionResult.AsyncWaitHandle.WaitOne(100);
            });

            if (scanner.Connected)
                IPsWithOpenGamePort.Add(ip);
            scanner.Close();
            IPsChecked[index] = true;
        }
    }

    private static async Task AllIPsChecked()
    {
        while (true)
        {
            bool allIPsChecked = await Task.Run(() =>
            {
                foreach (bool ipToCheck in IPsChecked)
                {
                    if (!ipToCheck)
                        return false;
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
