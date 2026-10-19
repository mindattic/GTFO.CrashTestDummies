using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Every coroutine this mod runs for the current level: the startup scatter, the ambient and cleanup
/// loops and each door ambush. CoroutineHost outlives any single expedition, so they're all stopped
/// together when the level ends (leaving InLevel, or a new level building). <see cref="Run"/> changes
/// on every stop, so a routine that captured an older value knows its level is over even if it was
/// somehow missed.
/// </summary>
internal static class LevelRoutines
{
    private static readonly List<Coroutine> Running = new();

    /// <summary>Changes every time the level's routines are stopped.</summary>
    public static int Run { get; private set; }

    public static bool IsCurrent(int run) => run == Run;

    public static void Start(IEnumerator routine)
    {
        var coroutine = Plugin.CoroutineHostInstance.StartCoroutine(routine.WrapToIl2Cpp());
        if (coroutine != null)
            Running.Add(coroutine);
    }

    public static void StopAll()
    {
        Run++;
        foreach (var coroutine in Running)
        {
            if (coroutine != null)
                Plugin.CoroutineHostInstance.StopCoroutine(coroutine);
        }

        Running.Clear();
    }
}
