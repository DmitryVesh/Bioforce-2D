using System;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int DataBufferSize = 4096;

    public string IPAddress = "127.0.0.1";
    public int PortNum = 28020; //Must be the same as GameServer Port
    public int ClientID = 0;
    public TCP tCP;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log($"Client instance already exists, destroying {gameObject.name}");
            Destroy(this);
        }
    }

    private void Start()
    {
        tCP = new TCP();
    }

    public void ConnectToServer()
    {
        tCP.Connect();
    }



    //TODO: almost same as GameServer TCP class, should make TCP Client class and TCP Server class
    public class TCP
    {
        public TcpClient Socket { get; private set; }
        private byte[] ReceiveBuffer;
        private NetworkStream Stream;


        public void Connect()
        {
            Socket = new TcpClient();
            Socket.ReceiveBufferSize = DataBufferSize;
            Socket.SendBufferSize = DataBufferSize;

            ReceiveBuffer = new byte[DataBufferSize];

            Socket.BeginConnect(instance.IPAddress, instance.PortNum, ConnectCallback, Socket);
        }
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Socket.EndConnect(asyncResult);
            if (!Socket.Connected) { return; } // No connected yet, then exit

            Stream = Socket.GetStream();
            StreamBeginRead();

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
                Console.WriteLine($"\nError in BeginReadReceiveCallback...\nError: {exception}");
            }
        }
        private void StreamBeginRead()
        {
            Stream.BeginRead(ReceiveBuffer, 0, DataBufferSize, BeginReadReceiveCallback, null);
        }
    }
}
