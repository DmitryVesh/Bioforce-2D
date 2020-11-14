﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer
{
    public class ServerRead
    {
        public static void WelcomeRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.ClientDictionary[clientID].tCP.Socket.Client.RemoteEndPoint} connected as player: {clientID}");
            if (clientID == checkClientID)
            {
                Server.ClientDictionary[clientID].SendIntoGame(username);
                return;
            }
            Console.WriteLine($"Error, player {username} is connected as wrong player number");
        }
        public static void UDPTestRead(int clientID, Packet packet)
        {
            int checkClientID = packet.ReadInt();
            string message = packet.ReadString();

            Console.WriteLine($"UDP Test received from: {checkClientID}\n{message}");
            if (clientID == checkClientID)
            {
                return;
            }
            Console.WriteLine($"Error, player {clientID} is connected as wrong player number {checkClientID}");
        }
        public static void PlayerMovementRead(int clientID, Packet packet)
        {
            Quaternion rotation = packet.ReadQuaternion();
            Vector3 position = packet.ReadVector3();
            Vector3 velocity = packet.ReadVector3();
            //Console.WriteLine("Received PlayerMovementRead Packet");

            Server.ClientDictionary[clientID].Player.PlayerMoves(rotation, position, velocity);
        }
        public static void PlayerMovementStatsRead(int clientID, Packet packet)
        {
            float runSpeed = packet.ReadFloat();
            float sprintSpeed = packet.ReadFloat();

            Server.ClientDictionary[clientID].Player.SetPlayerMovementStats(runSpeed, sprintSpeed);
        }
        public static void PlayerAnimationRead()
        {
            //throw new NotImplementedException("Player Animation in Client Send Not set up");
            /*
            float runSpeed = packet.ReadFloat();
            float speedX = packet.ReadFloat();
            bool grounded = packet.ReadBool();
            bool jumped = packet.ReadBool();
            */
        }
    }
}