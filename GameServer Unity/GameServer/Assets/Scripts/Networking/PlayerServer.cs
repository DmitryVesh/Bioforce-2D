using System;
using System.Timers;
using UnityEngine;

namespace GameServer
{
    class PlayerServer : MonoBehaviour
    {
        public byte ID { get; private set; }
        public string Username { get; private set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Score { get; set; }

        public int MaxHealth { get; set; } = 50;
        public int CurrentHealth { get; set; }

        private byte MoveState { get; set; }
        public bool IsFacingRight { get; private set; }
        private Vector2 LastPosition { get; set; }
        private byte LastMoveState { get; set; }


        private Vector2 ArmPosition { get; set; }
        private Quaternion ArmRotation { get; set; }
        private Vector2 LastPositionArms { get; set; }
        private Quaternion LastRotationArms { get; set; }


        private Vector2 LastPositionValidation { get; set; }
        public float RunSpeed { get; set; } = 0;
        public float SprintSpeed { get; set; } = 0;

        public bool IsDead { get; private set; } = false;

        public int PlayerColor { get; set; } = -1; //Represents the index of the color in the color palette

        public bool Paused { get; private set; }
        private bool LastPaused { get; set; }
        public TimeSpan PacketTimeOut { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 1, 0);
        public TimeSpan PacketPause { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 0, 20);
        public bool ReadyToPlay { get; set; }
       

        public void Init(byte iD, string username)
        {
            ID = iD;
            Username = username;            
            IsFacingRight = true;            
            CurrentHealth = MaxHealth;            
        }
        public void SetPlayerData(Vector2 position, int playerColor)
        {
            transform.position = position;
            LastPositionValidation = position;

            PlayerColor = playerColor;
        }
        public void Died()
        {
            Deaths++;
            IsDead = true;
        }
        public void AddKill()
        {
            Kills++;
            Score += 3;
        }
        public void Respawned()
        {
            IsDead = false;
            CurrentHealth = MaxHealth;
        }
        public void PlayerMoves(Vector2 position, byte moveState)
        {
            transform.position = position;
            MoveState = moveState;
        }
        internal void PlayerArmPosition(Vector2 position, Quaternion rotation)
        {
            ArmPosition = position;
            ArmRotation = rotation;
        }
        public void SetPlayerMovementStats(float runSpeed, float sprintSpeed)
        {
            RunSpeed = runSpeed;
            SprintSpeed = sprintSpeed;
        }

        internal void LastPacketReceived(TimeSpan timeOfDay)
        {
            PacketTimeOut = timeOfDay + new TimeSpan(0, 0, 10);
            PacketPause = timeOfDay + new TimeSpan(0, 0, 2);
        }

        public void FixedUpdate()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan zero = new TimeSpan(0, 0, 0);
            
            if (PacketTimeOut - now < zero)
            {
                Server.ClientDictionary[ID].Disconnect();
                Output.WriteLine($"\n\tPlayer: {ID} has been kicked, due to GameServer not having received packets in a while...");
                return;
            }
            else if (PacketPause - now < zero)
            {
                if (Paused) //Shouldn't send that the player is paused more than once
                    return;

                Paused = true;
                ServerSend.PlayerPausedGame(ID, Paused);
                return;
            }
            

            if (Paused)
            {
                Paused = false;
                ServerSend.PlayerPausedGame(ID, Paused);
            }

            SendPlayerData();

        }

        private void SendPlayerData()
        {
            if (!ReadyToPlay)
                return;

            if (LastPositionArms != ArmPosition || LastRotationArms != ArmRotation)
            {
                ServerSend.ArmPositionRotation(ID, ArmPosition, ArmRotation);
                LastPositionArms = ArmPosition;
                LastRotationArms = ArmRotation;
            }

            if (LastPosition != (Vector2)transform.position || MoveState != LastMoveState)
            {
                ServerSend.PlayerPositionButLocal(ID, transform.position, MoveState);
                LastPosition = transform.position;
                LastMoveState = MoveState;
            }
            //ServerSend.PlayerRotationAndVelocity(ID, Velocity);

            /*
            if (MovePlayerSent >= ServerProgram.Ticks)
            {
                if (ValidMove())
                    ServerSend.PlayerPositionButLocal(ID, Position);
                else
                {
                    ServerSend.PlayerPosition(ID, Position);
                    Output.WriteLine($"\n\tPlayer: {ID}, had an invalid move... Maybe cheating...");
                }
                MovePlayerSent = -1;
            }
            else
                ServerSend.PlayerPositionButLocal(ID, Position);

            //ServerSend.PlayerVelocity(ID, Velocity);
            ServerSend.PlayerRotationAndVelocity(ID, IsFacingRight, Velocity, Rotation);
            MovePlayerSent += 1;
            */
            //ServerSend.PlayerAnimation(ID, )
        }

        /*
        private bool ValidMove()
        {
            bool valid = true;
            //TODO: fix the x axis validation

            Vector2 validPosition = new Vector2(0, 0);
            //Validating X
            float xLast = LastPositionValidation.x;
            float xMaxTravelledRight = xLast + SprintSpeed + 0.5f;
            float xMaxTravelledLeft = xLast - SprintSpeed - 0.5f;
            Output.WriteLine($"\txLast: {xLast}\nxMaxTravelledLeft: {xMaxTravelledLeft}\nxMaxTravelledRight: {xMaxTravelledRight} ");

            float xNow = transform.position.x;
            if (xNow < xMaxTravelledLeft || xNow > xMaxTravelledRight)
            {
                valid = false;
                if (xNow < 0)
                    validPosition.x = xMaxTravelledLeft;
                else
                    validPosition.x = xMaxTravelledRight;
            }
            else
            {
                validPosition.x = transform.position.x;
            }

            //Validating Y
            //TODO: Validate Y
            validPosition.y = transform.position.y;

            LastPositionValidation = validPosition;

            return valid;
        }
        */

    }
}
