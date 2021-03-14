using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HostStateManager : MonoBehaviour
{
    protected bool Hosting { get; private set; }

    internal void Host(bool shouldHost) =>
        Hosting = shouldHost;

}
