using System;
using System.Collections.Generic;

namespace GameServer
{
    public class PlayerColor
    {
        public static List<int> AvailablePlayerColors { get; set; } = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }; 

        internal static void FreeColor(int colorToFree, int clientID)
        {
            if (colorToFree == -1) //First default choice is always -1
                return;

            if (AvailablePlayerColors.Contains(colorToFree)) 
                return;

            AvailablePlayerColors.Add(colorToFree);
            ServerSend.ColorIsAvailable(colorToFree, clientID);
        }

        internal static void TakeColor(int colorToTake, int clientID)
        {
            if (!AvailablePlayerColors.Contains(colorToTake))
            {
                //TODO: Send player, that the color is already taken and to choose any color available
                ServerSend.PlayerTriedTakingAlreadyTakenColor(clientID, AvailablePlayerColors);
                return;
            }

            AvailablePlayerColors.Remove(colorToTake);
            ServerSend.ColorIsTaken(colorToTake, clientID);
        }




        //public int R { get; private set; }
        //public int G { get; private set; }
        //public int B { get; private set; }
        //public PlayerColor(int r, int g, int b) =>
        //    (R, G, B) = (r, g, b);

        //private static Queue<PlayerColor> AvailablePlayerColors { get; set; } = null;



        //public static PlayerColor GetRandomColor()
        //{
        //    if (AvailablePlayerColors == null)
        //        AvailablePlayerColors = GeneratePlayerColors();
        //    return AvailablePlayerColors.Dequeue();
        //}
        //public static void GiveBackRandomColor(PlayerColor playerColor) =>
        //    AvailablePlayerColors.Enqueue(playerColor);

        //private static Queue<PlayerColor> GeneratePlayerColors()
        //{
        //    Queue<PlayerColor> playerColors = new Queue<PlayerColor>();

        //    Random random = new Random();
        //    for (int colorCount = 0; colorCount < 125; colorCount++)
        //    {
        //        (int r, int g, int b) = (RandomColorVal(random), RandomColorVal(random), RandomColorVal(random));
        //        PlayerColor color = new PlayerColor(r, g, b);
        //        if (playerColors.Contains(color))
        //        {
        //            colorCount--;
        //            continue;
        //        }

        //        playerColors.Enqueue(color);
        //    }

        //    return playerColors;
        //}

        //private static int RandomColorVal(Random random) =>
        //    random.Next(0, 256);


    }
}
