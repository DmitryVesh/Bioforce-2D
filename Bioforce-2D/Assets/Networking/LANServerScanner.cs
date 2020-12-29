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
            Debug.Log($"Gonna try to connect to ip: {ip}");
            ScanPortAsync(ip, ipCount - 1);
        }
        await AllIPsChecked();
        Debug.Log($"Have waited for AllIPsChecked");

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
            Debug.Log($"Have called Connection Result for IP: {ip}");
            await Task.Run(() =>
            {
                connectionResult.AsyncWaitHandle.WaitOne(1000);
            });

            Debug.Log($"Have waited for IP:{ip} second...");
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
            Debug.Log("Calling while true loop...");
            bool allIPsChecked = await Task.Run(() =>
            {
                for (int ipIndex = 0; ipIndex < IPsChecked.Length; ipIndex++)
                {
                    if (!IPsChecked[ipIndex])
                    {
                        Debug.Log($"IP: {ipIndex + 1} is false");
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
