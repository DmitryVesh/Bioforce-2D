using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer
{
    public class DiscoveryServer
    {

        private Socket ServerSocket { get; set; }
        private EndPoint RemoteEndPoint;

        public void StartServer(int port)
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        private void AsyncCallbackServer(IAsyncResult result)
        {
            if (ServerSocket != null)
            {
                try
                {
                    int size = ServerSocket.EndReceiveFrom(result, ref RemoteEndPoint);
                    byte[] pongBytes = Encoding.ASCII.GetBytes("pong");

                    ServerSocket.SendTo(pongBytes, RemoteEndPoint);

                    ServerSocket.BeginReceiveFrom(new byte[1024], 0, 1024, SocketFlags.None, ref RemoteEndPoint, new AsyncCallback(AsyncCallbackServer), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public void CloseServer()
        {
            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket = null;
            }
        }
    }
}
