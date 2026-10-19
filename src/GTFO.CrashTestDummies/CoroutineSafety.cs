using System;
using System.Collections;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Drains a coroutine one step at a time, catching and logging any exception it throws instead of
/// letting it propagate into whatever is running the coroutine. A plain "yield return subRoutine"
/// can't be wrapped in try/catch (C# forbids yield inside a try block with a catch clause), and an
/// unhandled exception there would otherwise kill the caller too - stopping a longer-lived loop for
/// the rest of the level, or skipping statements queued to run after it.
/// </summary>
internal static class CoroutineSafety
{
    public static IEnumerator Run(IEnumerator routine, string label)
    {
        while (true)
        {
            object? current;
            try
            {
                if (!routine.MoveNext())
                    yield break;
                current = routine.Current;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"CrashTestDummies: {label} threw and was stopped early: {ex}");
                yield break;
            }

            yield return current;
        }
    }
}
