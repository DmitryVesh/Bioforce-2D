using System;
using System.Collections;
using System.Collections.Generic;
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

        public bool IsFacingRight { get; private set; }

        private Vector2 LastPositionValidation { get; set; }
        public float RunSpeed { get; set; } = 0;
        public float SprintSpeed { get; set; } = 0;

        public bool IsDead { get; private set; } = false;

        public int PlayerColorIndex { get; set; } = -1; //Represents the index of the color in the color palette

        public bool Paused { get; set; }
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

        Vector2 PlayerPosition { get => transform.position; set => transform.position = value; }// = new Vector2(9.59f, 7.77f);

        private ConstantlySentPlayerData PlayerData { get; set; } = new ConstantlySentPlayerData();

        internal void SetPlayerPosition(Vector2 position)
        {
            PlayerPosition = position;
            PlayerData.SetPlayerPosition(position);
        }
        internal void PlayerMoveState(byte moveState) =>
            PlayerData.SetMoveState(moveState);
        internal void PlayerArmRotation(Quaternion rotation) =>
            PlayerData.SetArmRotation(rotation);
        internal void PlayerArmPosition(Vector2 position) =>
            PlayerData.SetArmPosition(position);

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


        public float Latency2WaySecondsTCP { get; private set; } = 0.300f; //default 0.3s = 300ms
        public float Latency2WaySecondsUDP { get; private set; } = 0.300f; //default 0.3s = 300ms

        private const byte LatencyIDLimit = 40;
        public byte LatencyIDTCP { get; set; } = 0;
        private Dictionary<byte, TimeSpan> LatencyDictionaryTCP { get; set; } = new Dictionary<byte, TimeSpan>();
        public byte LatencyIDUDP { get; set; } = 0;
        private Dictionary<byte, TimeSpan> LatencyDictionaryUDP { get; set; } = new Dictionary<byte, TimeSpan>();

        private static TimeSpan Now { get; set; }

        internal byte GetLatencyIDTCP()
        {
            LatencyIDTCP = GetLatencyID(LatencyIDTCP, LatencyDictionaryTCP);
            return LatencyIDTCP;
        }
        internal byte GetLatencyIDUDP()
        {
            LatencyIDUDP = GetLatencyID(LatencyIDUDP, LatencyDictionaryUDP);
            return LatencyIDUDP;
        }
        private byte GetLatencyID(byte latencyID, Dictionary<byte, TimeSpan> latencyDictionary)
        {
            latencyID = (byte)((latencyID + 1) % LatencyIDLimit);

            if (!latencyDictionary.ContainsKey(latencyID))
                latencyDictionary.Add(latencyID, Now);
            else
                latencyDictionary[latencyID] = Now;

            return latencyID;
        }

        internal void PingAckTCP(byte latencyIDTCP) =>
            Latency2WaySecondsTCP = PingAck(latencyIDTCP, LatencyDictionaryTCP);
        internal void PingAckUDP(byte latencyIDUDP) =>
            Latency2WaySecondsUDP = PingAck(latencyIDUDP, LatencyDictionaryUDP);
        private float PingAck(byte latencyID, Dictionary<byte, TimeSpan> latencyDictionary)
        {
            TimeSpan latencyCheckSent = latencyDictionary[latencyID];
            return (float)(Now - latencyCheckSent).TotalSeconds;
        }

        public void FixedUpdate()
        {
            Now = DateTime.Now.TimeOfDay;

            //Set the state of which to sent the constantly sent packets via: TCP, UDP or by both UDPandTCP
            //TODO: use ping instead
            SendConstantPacketsState sendConstantPacketsState;

            if (PacketSendViaOnlyTCP - Now < TimeSpanZero) //Flag so only send constant packets via TCP, because UDP is not responsive, so less data is sent
                sendConstantPacketsState = SendConstantPacketsState.TCP;
            else if (PacketSendViaTCPAndUDP - Now < TimeSpanZero) //Flag so both constantly sent packets are sent via both UDP and TCP
                sendConstantPacketsState = SendConstantPacketsState.UDPandTCP;
            else //Flag so can send via only UDP
                sendConstantPacketsState = SendConstantPacketsState.UDP;

            CurrentSendConstantPacketsState = sendConstantPacketsState;

            //Kicking or pausing the player based on TCP timeout 
            //TODO: use ping instead
            if (PacketTimeOutTCP - Now < TimeSpanZero)
            {
                Server.ClientDictionary[ID].Disconnect();
                PlayerColor.FreeColor(PlayerColorIndex, ID);
                if (gameObject != null)
                    Destroy(gameObject);
                Output.WriteLine($"\n\tPlayer: {ID} has been kicked, due to GameServer not having received packets in a while...");
                return;
            }
            else if (PacketPauseTCP - Now < TimeSpanZero)
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


            if (PlayerData.HasAnyData())
                NetworkManager.Instance.AddPlayerDataToBeSynchronised(ID, PlayerData);
            PlayerData.Reset();          


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
