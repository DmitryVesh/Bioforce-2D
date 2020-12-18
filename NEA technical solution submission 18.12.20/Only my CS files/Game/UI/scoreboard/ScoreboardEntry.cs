using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreboardEntry : MonoBehaviour
{
    public int ID { get; private set; }
    public int Score { get; private set; }
    private string Username { get; set; }
    private int Kills { get; set; }
    private int Deaths { get; set; }


    private TextMeshProUGUI PingText { get; set; }
    private TextMeshProUGUI PlayerText { get; set; }
    private TextMeshProUGUI KillsText { get; set; }
    private TextMeshProUGUI DeathsText { get; set; }
    private TextMeshProUGUI ScoreText { get; set; }

    public void Init()
    {
        PingText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        PlayerText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        KillsText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        DeathsText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        ScoreText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
    }
    public void Set(int iD, string username, int kills, int deaths, int score)
    {
        ID = iD;
        Score = score;
        Kills = kills;
        Username = username;
        Deaths = deaths;

        ResetTexts();
    }
    public void Set(ScoreboardEntry entry)
    {
        ID = entry.ID;
        Score = entry.Score;
        Kills = entry.Kills;
        Username = entry.Username;
        Deaths = entry.Deaths;

        ResetTexts();
    }
    public void AddKill()
    {
        Kills++;
        Score += 3;
        KillsText.text = Kills.ToString();
        ScoreText.text = Score.ToString();
    }
    public void AddDeath()
    {
        Deaths++;
        DeathsText.text = Deaths.ToString();
    }
    private void ResetTexts()
    {
        PlayerText.text = Username;
        KillsText.text = Kills.ToString();
        DeathsText.text = Deaths.ToString();
        ScoreText.text = Score.ToString();
    }
    public static void PrintValues(ScoreboardEntry entry)
    {
        Debug.Log($"ID: {entry.ID}, Name: {entry.Username}, Kills: {entry.Kills}, Deaths: {entry.Deaths}, Score: {entry.Score}");
    }
}