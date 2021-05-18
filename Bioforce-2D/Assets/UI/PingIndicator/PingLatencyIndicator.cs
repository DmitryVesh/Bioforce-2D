using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PingLatencyIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI PingText;
    [SerializeField] [Range(0.5f, 10f)] float TimeInBetweenPingTextUpdates = 1.5f;

    private void Start()
    {
        StartCoroutine(UpdatePingText());
    }

    private IEnumerator UpdatePingText()
    {
        while (true)
        {
            PingText.text = $"{Math.Round(Client.Instance.Latency2WayMSTCP, 0, MidpointRounding.AwayFromZero)}MS";
            yield return new WaitForSecondsRealtime(TimeInBetweenPingTextUpdates);
        }
    }
}
