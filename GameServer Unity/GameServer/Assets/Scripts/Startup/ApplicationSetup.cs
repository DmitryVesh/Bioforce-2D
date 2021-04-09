using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationSetup : MonoBehaviour
{
    public const int UpdateFrameRate = 30;
    private void Awake()
    {
        Application.targetFrameRate = UpdateFrameRate;
        QualitySettings.vSyncCount = 0;
    }
}
