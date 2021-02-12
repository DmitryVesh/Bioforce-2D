using System;
using System.Collections.Generic;
using System.Text;

namespace MainServerBioforce2D
{
    class Server
    {
        public DateTime TimeStamp { get; private set; } //given to ensure that server entries are synced when client ReAsksForServers

        public string ServerName { get; private set; } //unique
        public int MaxNumPlayers { get; private set; }
        public string MapName { get; private set; }
        public int CurrentNumPlayers { get; private set; }
        public int Ping { get; private set; }

        public Server(string serverName, int maxNumPlayers, string mapName, int currentNumPlayers, int ping)
        {
            ServerName = serverName;
            MaxNumPlayers = maxNumPlayers;
            MapName = mapName;
            CurrentNumPlayers = currentNumPlayers;
            Ping = ping;

            TimeStamp = DateTime.Now;
        }
    }
}
