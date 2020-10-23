using System;
using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public int ID { get; private set; }
        public TCP tCP { get; private set; }
        private static int DataBufferSize { get; set; } = 4096;

        public Client(int iD)
        {
            ID = iD;
            tCP = new TCP(iD);
        }

        public class TCP
        {
            public TcpClient Socket { get; private set; }
            private int ID { get; }
            private byte[] ReceiveBuffer;
            private NetworkStream Stream;


            public TCP(int iD) => ID = iD;
            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                Stream = Socket.GetStream();
                ReceiveBuffer = new byte[DataBufferSize];

                StreamBeginRead();
            }
            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        Stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"\nError has occured when sending data from client {ID}\nError{exception}");
                }
            }
            
            private void BeginReadReceiveCallback(IAsyncResult asyncResult)
            {
                try
                {
                    int byteLen = Stream.EndRead(asyncResult);
                    if (byteLen <= 0)
                    {
                        // TODO: Disconnect
                        return;
                    }

                    byte[] data = new byte[byteLen];
                    Array.Copy(ReceiveBuffer, data, byteLen);

                    //TODO: handle data
                    StreamBeginRead();
                }
                catch (Exception exception)
                {
                    // TODO: disconnect
                    Console.WriteLine($"\nError in BeginReadReceiveCallback of client {ID}...\nError: {exception}");
                }
            }
            private void StreamBeginRead()
            {
                Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
            }
        }
    }
}
