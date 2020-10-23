using System;

namespace GameServer
{
    static class Program
    {
        static void Main()
        {
            string programTitleName = "GameServer";
            Console.Title = programTitleName;

            int maxNumPlayers = 10;
            int portNum = 28020; //Unused port, checked Wiki page https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers for unused ports
            Server.StartServer(maxNumPlayers, portNum);

            Console.WriteLine($"Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
