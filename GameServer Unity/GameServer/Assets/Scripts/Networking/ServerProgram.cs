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
            string[] args 
                = Environment.GetCommandLineArgs().Skip(2).ToArray(); //Skip the fileName of this executable, and the "GameServer" string given to easily identify which processes in linux are MainServer/GameServer

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

            int timeOutSeconds = int.Parse(args[5]);
            Output.WriteLine(timeOutSeconds.ToString());
            
            bool isServerPermanent = bool.Parse(args[6]);
            Output.WriteLine(isServerPermanent.ToString());

            string mainServerIP = args[7];
            Output.WriteLine(mainServerIP);

            try
            {
                Output.InitLogFile($"GameServer_{serverName}");
                SceneManager.LoadScene(mapName);
                MainServerComms.Connect(portMainServer, serverName, mainServerIP);
                Server.StartServer(serverName, maxNumPlayers, mapName, portGame, timeOutSeconds, isServerPermanent);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError starting server:\n{exception}");
            }
        }
    }
}
