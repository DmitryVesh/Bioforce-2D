using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        //TODO: 900 implement a scoring kill count system.
        //private static Dictionary<int, int> GameKillScore = new Dictionary<int, int>();

        public static void Update()
        {
            foreach (Client client in Server.ClientDictionary.Values)
            {
                if (client.player != null)
                {
                    client.player.Update();
                }
            }
            ThreadManager.UpdateMain();
        }
    }
}
