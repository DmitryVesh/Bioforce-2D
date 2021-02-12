using System;
using System.Diagnostics;

namespace MainServerBioforce2D
{
    class GameServerProcess : Process
    {
        public GameServerEventHandler OnGameServerExited { get; set; }
        string ServerName { get; set; }
        public int ServerPort { get; private set; }

        public GameServerProcess(string serverName, int serverPort)
        {
            ServerName = serverName;
            ServerPort = serverPort;
            Exited += new EventHandler(GameServerExited);            
        }
        private void GameServerExited(object obj, EventArgs args) 
        {
            OnGameServerExited?.Invoke(this, new GameServerArgs(ServerName, ServerPort));
        }
    }

    public delegate void GameServerEventHandler(object sender, GameServerArgs args);
    public class GameServerArgs : EventArgs
    {
        public string ServerName { get; private set; }
        public int ServerPort { get; private set; }
        public GameServerArgs(string serverName, int serverPort) =>
            (ServerName, ServerPort) = (serverName, serverPort);
    }
    
}
