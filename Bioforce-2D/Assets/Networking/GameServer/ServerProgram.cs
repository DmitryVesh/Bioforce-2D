using System;
using UnityEngine;

namespace GameServer
{
    class ServerProgram : MonoBehaviour
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        private static bool isRunning = true;

        public static bool StartServerProgram(string serverName, int maxNumPlayers, string mapName, int port)
        {
            try
            {
                int portNumGame = port; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
                int portNumLANDiscover = 28021;


                DiscoveryServer.StartServer(portNumLANDiscover);
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
