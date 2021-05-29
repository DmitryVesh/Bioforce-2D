using System;

namespace MainServerBioforce2D
{
    struct Server
    {
        public DateTime TimeStamp { get; private set; } //given to ensure that server entries are synced when client ReAsksForServers

        public string ServerName { get; private set; } //unique
        public byte ServerState { get; private set; }
        public int MaxNumPlayers { get; private set; }
        public string MapName { get; private set; }
        public int CurrentNumPlayers { get; private set; }
        public int Ping { get; private set; }

        public Server(string serverName, byte serverState, int maxNumPlayers, string mapName, int currentNumPlayers, int ping)
        {
            ServerName = serverName;
            ServerState = serverState;
            MaxNumPlayers = maxNumPlayers;
            MapName = mapName;
            CurrentNumPlayers = currentNumPlayers;
            Ping = ping;

            TimeStamp = DateTime.Now;
        }

        public bool MatchesName(string name) =>
            ServerName == name;
        public static bool operator ==(Server server1, Server server2) =>
            server1.ServerName == server2.ServerName;
        public static bool operator !=(Server server1, Server server2) =>
            server1.ServerName != server2.ServerName;
    }
}
