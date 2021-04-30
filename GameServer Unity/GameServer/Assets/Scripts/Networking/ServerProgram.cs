using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Output;

namespace GameServer
{
    public class ServerProgram : MonoBehaviour
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        public static bool IsRunning { get; private set; } = true;

        private void Start()
        {
            //Skip the fileName of this executable, and the "GameServer" string given to easily identify which processes in linux are MainServer/GameServer
            string[] args = Environment.GetCommandLineArgs().Skip(2).ToArray(); 

            StartServerProgram(args);
        }

        public static void StartServerProgram(string[] args)
        {
            string serverName = args[0];
            Output.WriteLine(serverName);

            int maxNumPlayers = int.Parse(args[1]);
            Output.WriteLine(maxNumPlayers.ToString());

            string mapName = args[2];
            Output.WriteLine(mapName);

            int portGame = int.Parse(args[3]);
            Output.WriteLine(portGame.ToString());

            int portMainServer = int.Parse(args[4]);
            Output.WriteLine(portMainServer.ToString());

            int timeOut = int.Parse(args[5]);
            Output.WriteLine(timeOut.ToString());

            try
            {
                Output.InitLogFile($"GameServer_{serverName}");
                SceneManager.LoadScene(mapName);
                MainServerComms.Connect(portMainServer, serverName, timeOut);
                Server.StartServer(serverName, maxNumPlayers, mapName, portGame);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError starting server:\n{exception}");
            }
        }
    }
}
