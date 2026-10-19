using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;

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
    /// <summary>Changes every time the level's routines are stopped.</summary>
    public static int Run { get; private set; }

    public static bool IsCurrent(int run) => run == Run;

    public static void Start(IEnumerator routine) =>
        Plugin.CoroutineHostInstance.StartCoroutine(routine.WrapToIl2Cpp());

    public static void StopAll()
    {
        Run++;

        // StopAllCoroutines, not StopCoroutine on each started handle: a routine that yields another
        // IEnumerator (the startup sequence's scatter, each ambient scatter) hands it to Unity as a
        // separate child coroutine, and stopping the parent leaves that child running - it would keep
        // spawning into a level that's over. Every child runs on CoroutineHost too, and nothing else
        // uses CoroutineHost, so stopping all of its coroutines stops exactly this level's routines.
        Plugin.CoroutineHostInstance.StopAllCoroutines();
    }
}
