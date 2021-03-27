using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private Sprite IconWithinRange;
    [SerializeField] private Sprite IconOutOfRange;

    private void Start()
    {
        Minimap.SubscribeIcon(this);
    }
}
