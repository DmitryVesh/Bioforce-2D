﻿using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class GameLogic
    {
        public static void Update()
        {
            foreach (ClientServer client in Server.ClientDictionary.Values)
            {
                if (client.Player != null)
                    client.Player.Update();
            }
        }
    }
}
