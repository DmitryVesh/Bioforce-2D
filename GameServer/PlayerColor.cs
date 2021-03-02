﻿using System;
using System.Collections.Generic;

namespace GameServer
{
    public class PlayerColor
    {
        public int R { get; private set; }
        public int G { get; private set; }
        public int B { get; private set; }
        public PlayerColor(int r, int g, int b) =>
            (R, G, B) = (r, g, b);

        private static Queue<PlayerColor> AvailablePlayerColors { get; set; } = null;
        private static bool HaveGeneratedBefore { get; set; }

        public static PlayerColor GetRandomColor()
        {
            if (AvailablePlayerColors == null)
                AvailablePlayerColors = GeneratePlayerColors();
            return AvailablePlayerColors.Dequeue();
        }
        public static void GiveBackRandomColor(PlayerColor playerColor) =>
            AvailablePlayerColors.Enqueue(playerColor);

        private static Queue<PlayerColor> GeneratePlayerColors()
        {
            HaveGeneratedBefore = true;
            Queue<PlayerColor> playerColors = new Queue<PlayerColor>();

            Random random = new Random();
            for (int colorCount = 0; colorCount < 125; colorCount++)
            {
                (int r, int g, int b) = (RandomColorVal(random), RandomColorVal(random), RandomColorVal(random));
                PlayerColor color = new PlayerColor(r, g, b);
                if (playerColors.Contains(color))
                {
                    colorCount--;
                    continue;
                }

                playerColors.Enqueue(color);
            }
            
            return playerColors;
        }

        private static int RandomColorVal(Random random) =>
            random.Next(0, 256);
    }
}
