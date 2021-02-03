using System;
using System.Threading;
using UnityEngine;

namespace GameServer
{
    class ServerProgram : MonoBehaviour
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        private static bool isRunning = true;

        public static bool Start(string serverName, int maxNumPlayers, string mapName)
        {
            try
            {
                int portNumGame = 28020; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
                int portNumDiscover = portNumGame + 1;


                DiscoveryServer.StartServer(portNumDiscover);
                Server.StartServer(serverName, maxNumPlayers, mapName, portNumGame);

                return true;
            }
            catch (Exception exception)
            {
                Debug.Log($"Error starting server:\n{exception}");
                return false;
            }
        }
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        private void FixedUpdate()
        {
            if (isRunning)
            {
                GameLogic.Update();
            }
        }
    }
}
