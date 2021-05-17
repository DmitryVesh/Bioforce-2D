using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Output;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TimerText;
    private bool ClockIsRunning { get; set; } = false;

    private void Awake()
    {
        GameStateManager.WaitingForGame += SetWaitingClock;
        GameStateManager.GameActivated += ActivateClock;
        GameStateManager.GameEnded += EndClock;
        GameStateManager.GameRestarting += SetWaitingClock;
    }
    private void OnDestroy()
    {
        GameStateManager.WaitingForGame -= SetWaitingClock;
        GameStateManager.GameActivated -= ActivateClock;
        GameStateManager.GameEnded -= EndClock;
        GameStateManager.GameRestarting -= SetWaitingClock;
    }

    private void EndClock()
    {
        ClockIsRunning = false;
        Output.WriteLine("Server has ended game");
    }
    private void SetWaitingClock()
    {
        TimerText.text = "Waiting";
        ClockIsRunning = false;
    }
    private void ActivateClock(float remainingTimeSeconds)
    {
        ClockIsRunning = true;
        StartCoroutine(StartClock(remainingTimeSeconds));
    }

    private IEnumerator StartClock(float remainingTimeSeconds)
    {
        TimeSpan timeWhenMilisecondsShouldBeShown = new TimeSpan(0, 0, 10);
        while (ClockIsRunning)
        {
            //Check if the remainingTime is less than or equal to 0
            //If it is, then show Finished, or show error?
            if (remainingTimeSeconds <= 0)
            {
                Output.WriteLine("Finishing before receiving message from server that the game is ended...");
                break;
            }

            TimeSpan time = TimeSpan.FromSeconds(remainingTimeSeconds);
            
            string text;
            if (time >= timeWhenMilisecondsShouldBeShown)
                text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            else
                text = string.Format("{0:D1}:{1:D3}", time.Seconds, time.Milliseconds);
            
            TimerText.text = text;

            yield return new WaitForFixedUpdate();
            remainingTimeSeconds -= Time.fixedDeltaTime;
        }

        TimerText.text = "Finished";
    }
}
