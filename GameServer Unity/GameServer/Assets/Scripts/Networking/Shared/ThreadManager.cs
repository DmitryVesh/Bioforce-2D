using System;
using System.Collections.Generic;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using UnityEngine.Output;
#else
using Output = System.Console;
#endif

namespace Shared
{
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
    public class ThreadManager : MonoBehaviour
    {
        private void FixedUpdate()
        {
            UpdateMain();
        }
#else
    public class ThreadManager
    {
#endif

        private static readonly List<Action> ActionsToExecute = new List<Action>();
        private static readonly List<Action> CopiedActionsToExecute = new List<Action>();
        private static bool HasActionToExecute { get; set; } = false;

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
            {
                Output.WriteLine("No action to execute on main thread!");
                return;
            }

            lock (ActionsToExecute)
            {
                ActionsToExecute.Add(action);
                HasActionToExecute = true;
            }
        }

        public static void ExecuteAfterTime(Action action, int miliseconds)
        {
            if (action == null)
            {
                Output.WriteLine("Null action was given to thread...");
                return;
            }

        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (HasActionToExecute)
            {
                CopiedActionsToExecute.Clear();
                lock (ActionsToExecute)
                {
                    CopiedActionsToExecute.AddRange(ActionsToExecute);
                    ActionsToExecute.Clear();
                    HasActionToExecute = false;
                }

                for (int i = 0; i < CopiedActionsToExecute.Count; i++)
                    CopiedActionsToExecute[i]();
            }
        }   
    }
}