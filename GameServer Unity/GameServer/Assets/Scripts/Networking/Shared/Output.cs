using System;
using UnityEngine;

public class Output
{
    public static void WriteLine(string message)
    {
        if (Application.isBatchMode)
            Console.WriteLine(message);
        else
            Debug.Log(message);
    }
}
