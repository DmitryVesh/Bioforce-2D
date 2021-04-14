using GameServer;
using System;
using System.IO;
using System.Timers;
using UnityEngine;

public class Output //: MonoBehaviour
{
    //public static Output Instance { get; set; }

    //private StreamWriter StreamWriter;
    //private Timer WriteLogTimer;
    //private const double MinuteInMS = 60_000d;
    //private static bool HaveAddedLog = false;
    //private static string AddToLog = "";

    public static void WriteLine(string message)
    {
        //HaveAddedLog = true;
        //AddToLog += message;

        if (Application.isBatchMode)
            Console.WriteLine(message);
        else
            Debug.Log(message);
    }

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else if (Instance != this)
    //    {
    //        Debug.Log($"Output instance already exists, destroying {gameObject.name}");
    //        Destroy(gameObject);
    //    }        
    //}
    //private void OnDestroy()
    //{
    //    StreamWriter.Close();
    //}

    //public void Init(string serverName)
    //{
    //    StreamWriter = new StreamWriter($"GameServer{serverName}.txt", true);

    //    WriteLogTimer = new Timer(MinuteInMS);
    //    WriteLogTimer.Elapsed += WriteServerLog;
    //    WriteLogTimer.Start();
    //}

    //private void WriteServerLog(object sender, ElapsedEventArgs e)
    //{
    //    if (!HaveAddedLog)
    //        return;

    //    string addToLog =
    //        $"\n=========================================================" +
    //        $"\n{DateTime.UtcNow}" +
    //        $"\n=========================================================" +
    //        AddToLog;

    //    StreamWriter.WriteLine(addToLog);
        
    //    HaveAddedLog = false;
    //    AddToLog = "";
    //}
}
