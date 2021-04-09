﻿using System;
using System.Linq;
using System.Threading;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameServer
{
    public class ServerProgram : MonoBehaviour
    {
        public const int Ticks = 30;
        public const int MillisecondsInTick = 1000 / Ticks;

        public static bool IsRunning { get; private set; } = true;
        [SerializeField] public bool IsTesting = false;

        private void Start()
        {
            float example = -10f;
            byte[] bytes = BitConverter.GetBytes(example);
            string output = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                output += bytes[i];
                Output.WriteLine(bytes[i].ToString());
            }
            string newOutput = "";
            for (int i = output.Length - 1; i >= 0; i--)
            {
                newOutput += output[i];
            }
            Output.WriteLine($"{newOutput}");

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
                SceneManager.LoadScene(mapName);
                MainServerComms.Connect(portMainServer, serverName, timeOut);
                Server.StartServer(serverName, maxNumPlayers, mapName, portGame);
            }
            catch (Exception exception)
            {
                Output.WriteLine($"\tError starting server:\n{exception}");
            }
        }

        //private void FixedUpdate()
        //{
        //    if (IsTesting && Input.GetKey(KeyCode.C))
        //        Debug.developerConsoleVisible = !Debug.developerConsoleVisible;
        //}
    }
}