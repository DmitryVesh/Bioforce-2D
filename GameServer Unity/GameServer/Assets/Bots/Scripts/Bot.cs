using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameServer;

public enum BotType
{
    balanced,
    aggressive,
    slow
}
public class Bot : MonoBehaviour
{
    [SerializeField] public BotType BotType;

    public ushort BotID { get; set; }
    public PlayerServer Player { get; private set; }

    public Bot(BotType botType)
    {
        BotType = botType;
    }

    public void SetActive(bool active) =>
        gameObject.SetActive(active);

    //internal void Spawn(Vector2 spawnPosition)
    //{
    //    Player.SetPlayerPosition(spawnPosition);

    //    gameObject.transform.position = spawnPosition;
    //}
}
