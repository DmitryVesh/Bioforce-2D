using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileSafeCanvasOverlay : MonoBehaviour
{
    void Start()
    {
        var show = !GameManager.IsMobileSupported && GameManager.TestingTouchInEditor;
        gameObject.SetActive(show);
    }
}
