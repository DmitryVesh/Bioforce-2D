using System;
using System.Numerics;
using System.Timers;

namespace GameServer
{
    class PlayerServer
    {
        public int ID { get; private set; }
        public string Username { get; private set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Score { get; set; }

        public int MaxHealth { get; set; } = 50;
        public int CurrentHealth { get; set; }

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Quaternion Rotation { get; private set; }
        public bool IsFacingRight { get; private set; }

        private Vector2 LastPositionValidation { get; set; }
        public float RunSpeed { get; set; } = 0;
        public float SprintSpeed { get; set; } = 0;

        public bool IsDead { get; private set; } = false;

        public int PlayerColor { get; set; } = -1;//Represents the index of the color in the color palette

        public bool Paused { get; private set; }
        private bool LastPaused { get; set; }
        public TimeSpan PacketTimeOut { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 1, 0);
        public TimeSpan PacketPause { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 0, 20);
        public bool ReadyToPlay { get; set; }

        public PlayerServer(int iD, string username)
        {
            ID = iD;
            Username = username;            
            IsFacingRight = true;            
            CurrentHealth = MaxHealth;            
        }
        public void SetPlayerData(Vector2 position, int playerColor)
        {
            Position = position;
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
        public void PlayerMoves(bool isFacingRight, Vector2 position, Vector2 velocity, Quaternion rotation)
        {
            IsFacingRight = isFacingRight;
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
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

        public void Update()
        {
            
            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan zero = new TimeSpan(0, 0, 0);
            
            if (PacketTimeOut - now < zero)
            {
                Server.ClientDictionary[ID].Disconnect();
                Console.WriteLine($"\n\tPlayer: {ID} has been kicked, due to GameServer not having received packets in a while...");
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
            MovePlayer();
        }

        private void MovePlayer()
        {
            if (!ReadyToPlay)
                return;

            ServerSend.PlayerPositionButLocal(ID, Position);
            ServerSend.PlayerRotationAndVelocity(ID, IsFacingRight, Velocity, Rotation);

            /*
            if (MovePlayerSent >= ServerProgram.Ticks)
            {
                if (ValidMove())
                    ServerSend.PlayerPositionButLocal(ID, Position);
                else
                {
                    ServerSend.PlayerPosition(ID, Position);
                    Console.WriteLine($"\n\tPlayer: {ID}, had an invalid move... Maybe cheating...");
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

        private bool ValidMove()
        {
            bool valid = true;
            //TODO: fix the x axis validation

            Vector2 validPosition = new Vector2(0, 0);
            //Validating X
            float xLast = LastPositionValidation.X;
            float xMaxTravelledRight = xLast + SprintSpeed + 0.5f;
            float xMaxTravelledLeft = xLast - SprintSpeed - 0.5f;
            Console.WriteLine($"\txLast: {xLast}\nxMaxTravelledLeft: {xMaxTravelledLeft}\nxMaxTravelledRight: {xMaxTravelledRight} ");

            float xNow = Position.X;
            if (xNow < xMaxTravelledLeft || xNow > xMaxTravelledRight)
            {
                valid = false;
                if (xNow < 0)
                    validPosition.X = xMaxTravelledLeft;
                else
                    validPosition.X = xMaxTravelledRight;
            }
            else
            {
                validPosition.X = Position.X;
            }

            //Validating Y
            //TODO: Validate Y
            validPosition.Y = Position.Y;

            LastPositionValidation = validPosition;

            return valid;
        }
    }
}
