using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        public static void Update()
        {
            foreach (Client client in Server.ClientDictionary.Values)
            {
                if (client.Player != null)
                {
                    client.Player.Update();
                }
            }
            ThreadManager.UpdateMain();
        }
    }
}
