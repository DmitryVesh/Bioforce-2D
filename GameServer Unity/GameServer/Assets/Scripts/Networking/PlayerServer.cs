using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Output;

namespace GameServer
{
    public class PlayerServer : MonoBehaviour
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

        public int PlayerColorIndex { get; set; } = -1; //Represents the index of the color in the color palette

        public bool Paused { get; private set; }
        private bool LastPaused { get; set; }
        public TimeSpan PacketTimeOutTCP { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 1, 0);
        public TimeSpan PacketPauseTCP { get; set; } = DateTime.Now.TimeOfDay + new TimeSpan(0, 0, 20);
        public bool ReadyToPlay { get; set; }
        public readonly TimeSpan TimeSpanZero = new TimeSpan(0, 0, 0);

        private TimeSpan PacketSendViaOnlyTCP { get; set; }
        private TimeSpan PacketSendViaTCPAndUDP { get; set; }
        public SendConstantPacketsState CurrentSendConstantPacketsState { get; private set; } = SendConstantPacketsState.UDPandTCP;
        
        public float CurrentInvincibilityTime { get; set; }

        private string ChatEntryToSend { get; set; } = "";

        private void OnDestroy()
        {
            Output.WriteLine($"\n\tPlayer:{ID} - \"{Username}\" is destroyed/removed from server");
        }

        public void Init(byte iD, string username)
        {
            ID = iD;
            Username = username;            
            IsFacingRight = true;            
            CurrentHealth = MaxHealth;            
        }
        public void SetFinalPlayerData(Vector2 position, int playerColor)
        {
            transform.position = position;
            LastPositionValidation = position;

            PlayerColorIndex = playerColor;
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

        public void RestoreHealth(int healthRestore)
        {
            CurrentHealth += healthRestore;
            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
        }

        internal void PickedUpAdrenaline(float invincibilityTime)
        {
            ResetInvincibilityTime(invincibilityTime);
        }
        internal void ResetInvincibilityTime(float invincibilityTime)
        {
            CurrentInvincibilityTime = invincibilityTime;
            StartCoroutine(DecreaseInvincibilityTime());
        }
        private IEnumerator DecreaseInvincibilityTime()
        {
            do
            {
                yield return new WaitForFixedUpdate();
                CurrentInvincibilityTime -= Time.fixedDeltaTime;
            } while (CurrentInvincibilityTime > 0);
        }

        internal void LastPacketReceivedTCP(TimeSpan timeOfDay)
        {
            PacketTimeOutTCP = timeOfDay + new TimeSpan(0, 0, 10);
            PacketPauseTCP = timeOfDay + new TimeSpan(0, 0, 2);
        }
        internal void LastPacketReceivedUDP(TimeSpan timeOfDay)
        {
            PacketSendViaOnlyTCP = timeOfDay + new TimeSpan(0, 0, 5);
            PacketSendViaTCPAndUDP = timeOfDay + new TimeSpan(0, 0, 0,0, 500);
        }

        internal void MessageToSend(string text) =>
            ChatEntryToSend = $"[{Username}]: {text}";

        public void FixedUpdate()
        {
            TimeSpan now = DateTime.Now.TimeOfDay;

            SendConstantPacketsState sendConstantPacketsState;

            if (PacketSendViaOnlyTCP - now < TimeSpanZero) //Flag so only send constant packets via TCP, because UDP is not responsive, so less data is sent
                sendConstantPacketsState = SendConstantPacketsState.TCP;
            else if (PacketSendViaTCPAndUDP - now < TimeSpanZero) //Flag so both constantly sent packets are sent via both UDP and TCP
                sendConstantPacketsState = SendConstantPacketsState.UDPandTCP;
            else //Flag so can send via only UDP
                sendConstantPacketsState = SendConstantPacketsState.UDP;

            CurrentSendConstantPacketsState = sendConstantPacketsState;

            if (PacketTimeOutTCP - now < TimeSpanZero)
            {
                Server.ClientDictionary[ID].Disconnect();
                PlayerColor.FreeColor(PlayerColorIndex, ID);
                if (gameObject != null)
                    Destroy(gameObject);
                Output.WriteLine($"\n\tPlayer: {ID} has been kicked, due to GameServer not having received packets in a while...");
                return;
            }
            else if (PacketPauseTCP - now < TimeSpanZero)
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

            if (ChatEntryToSend != "")
            {
                ServerSend.ChatMessage(ChatEntryToSend, ID);
                ChatEntryToSend = "";
            }

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
