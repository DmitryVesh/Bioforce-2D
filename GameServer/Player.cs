using System;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int ID { get; private set; }
        public string Username { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Quaternion Rotation { get; private set; }

        private Vector3 LastPosition { get; set; }
        public float RunSpeed { get; set; } = 0;
        public float SprintSpeed { get; set; } = 0;
        private int MovePlayerSent { get; set; } = 0;

        public bool IsDead { get; private set; } = false;

        public Player(int iD, string username, Vector3 position)
        {
            ID = iD;
            Username = username;
            Position = position;
            Rotation = Quaternion.Identity;
            LastPosition = position;
        }
        public void Died()
        {
            IsDead = true;
        }
        public void Respawned()
        {
            IsDead = false;
        }
        public void PlayerMoves(Quaternion rotation, Vector3 position, Vector2 velocity)
        {
            Rotation = rotation;
            Position = position;
            Velocity = velocity;
        }
        public void SetPlayerMovementStats(float runSpeed, float sprintSpeed)
        {
            RunSpeed = runSpeed;
            SprintSpeed = sprintSpeed;
        }
        public void Update()
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            if (MovePlayerSent >= Program.Ticks)
            {
                if (ValidMove())
                    ServerSend.PlayerPositionButLocal(ID, Position);
                else
                {
                    ServerSend.PlayerPosition(ID, Position);
                    Console.WriteLine($"\nPlayer: {ID}, had an invalid move... Maybe cheating...");
                }
                MovePlayerSent = -1;
            }
            else
                ServerSend.PlayerPositionButLocal(ID, Position);

            //ServerSend.PlayerVelocity(ID, Velocity);
            ServerSend.PlayerRotationAndVelocity(ID, Rotation, Velocity);
            MovePlayerSent += 1;
            //ServerSend.PlayerAnimation(ID, )
        }

        private bool ValidMove()
        {
            bool valid = true;
            Vector3 validPosition = new Vector3(0, 0, 0);
            //Validating X
            float xLast = LastPosition.X;
            float xMaxTravelledRight = xLast + SprintSpeed + 0.5f;
            float xMaxTravelledLeft = xLast - SprintSpeed - 0.5f;
            //Console.WriteLine($"xLast: {xLast}\nxMaxTravelledLeft: {xMaxTravelledLeft}\nxMaxTravelledRight: {xMaxTravelledRight} ");

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

            validPosition.Z = Position.Z;

            LastPosition = validPosition;

            return valid;
        }
    }
}
