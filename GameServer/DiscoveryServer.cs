using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer
{
    public static class DiscoveryServer
    {

        private static Socket ServerSocket { get; set; }
        private static EndPoint RemoteEndPoint;

        public static void StartServer(int port)
        {
            Console.WriteLine("\nTrying to start the Discovery Server...");
            if (ServerSocket == null)
            {
                try
                {
                    ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    if (ServerSocket == null)
                    {
                        Console.WriteLine("Server socket is null");
                        return;
                    }

                    ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    ServerSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackServer), null);
                    Console.WriteLine("\nSuccessfully started the Discovery Server.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in StartServer of DiscoveryServer:\n{exception}");
                }
            }
        }
        private static void AsyncCallbackServer(IAsyncResult result)
        {
            if (ServerSocket != null)
            {
                try
                {
                    int size = ServerSocket.EndReceiveFrom(result, ref RemoteEndPoint);
                    Console.WriteLine($"Player has sent out a Discovery call from: {RemoteEndPoint.ToString()}");

                    //TODO: Send serverName, playerCount, mapName, ping
                    //byte[] pongBytes = Encoding.ASCII.GetBytes("pong");
                    string serverName = "Random Please Work";
                    int playerCount = 21;
                    string mapName = "Please";
                    int ping = 10;
                    byte[] serverDiscover = Encoding.ASCII.GetBytes(serverName).Concat(BitConverter.GetBytes(playerCount)).Concat(Encoding.ASCII.GetBytes(mapName)).Concat(BitConverter.GetBytes(ping)).ToArray();

                    //ServerSocket.SendTo(pongBytes, RemoteEndPoint);
                    ServerSocket.SendTo(serverDiscover, RemoteEndPoint);

                    ServerSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackServer), null);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error in AsyncCallback of Discovery Server: \n{exception}");
                }
            }
        }
        public static void CloseServer()
        {
            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket = null;
            }
        }
    }
}
