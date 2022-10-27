using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BotType
{
    balanced,
    aggressive,
    slow
}
public class Bot : MonoBehaviour
{
    [SerializeField] public BotType BotType;
    public byte BotID { get; set; }
}
