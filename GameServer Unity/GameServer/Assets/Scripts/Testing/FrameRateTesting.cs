using UnityEngine;
using UnityEngine.Output;

public class FrameRateTesting : MonoBehaviour
{
    private int CurrentFrameFixedCount { get; set; } = 0;
    private void FixedUpdate()
    {
        CurrentFrameFixedCount = (CurrentFrameFixedCount + 1) % ApplicationSetup.UpdateFrameRate;
        if (CurrentFrameFixedCount == 0)
            Output.WriteLine($"FixedUpdate all: {ApplicationSetup.UpdateFrameRate} called");
    }

    private int CurrentFrameUpdateCount { get; set; } = 0;
    private void Update()
    {
        CurrentFrameUpdateCount = (CurrentFrameUpdateCount + 1) % ApplicationSetup.UpdateFrameRate;
        if (CurrentFrameUpdateCount == 0)
            Output.WriteLine($"Update all: {ApplicationSetup.UpdateFrameRate} called");
    }
}
