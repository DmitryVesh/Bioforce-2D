using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace MainServerBioforce2D
{
    class Output
    {
        private static StreamWriter StreamWriter;
        private static Timer WriteLogTimer;
        private const double MinuteInMS = 60_000d;
        private static bool HaveAddedLog = false;
        private static string AddToLog = "";

        private static string LogFileName;

        public static void WriteLine(string message)
        {
            HaveAddedLog = true;
            AddToLog += "\n";
            AddToLog += message;

            Console.WriteLine(message);
        }

        public static void Init(string version)
        {
            LogFileName = $"MainServer_{version}.txt";
            StreamWriter = new StreamWriter(LogFileName, true);

            WriteLogTimer = new Timer(MinuteInMS);
            WriteLogTimer.Elapsed += WriteServerLog;
            WriteLogTimer.Start();

            AppDomain.CurrentDomain.ProcessExit += WriteLastEndingLog;
        }

        private static void WriteLastEndingLog(object sender, EventArgs e)
        {
            WriteLine(
                "\n*************************" +
                "\nEnding server" +
                "\n*************************");

            WriteServerLog(null, null);

            StreamWriter.Close();
            StreamWriter.Dispose();
        }

        private static void WriteServerLog(object sender, ElapsedEventArgs e)
        {
            if (!HaveAddedLog)
                return;

            string addToLog =
                $"\n=========================================================" +
                $"\n{DateTime.UtcNow}" +
                $"\n=========================================================" +
                AddToLog;

            StreamWriter.WriteLine(addToLog);
            StreamWriter.Flush();

            HaveAddedLog = false;
            AddToLog = "";
        }
    }
}
