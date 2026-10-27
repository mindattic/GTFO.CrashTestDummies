using System.Collections;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Self-management safety valve: every PerformanceCheckIntervalSeconds, measures the host's average
/// framerate since the last check and, if it's below MinAcceptableFPS, recycles a batch of currently
/// out-of-sight dummies (see <see cref="EnemyCleanup.DespawnHiddenEnemies"/>) to claw back performance.
/// This is on top of (not instead of) the age-gated cleanup sweep, so the mod can never become the
/// reason the game chugs regardless of what the population cap or cleanup interval are set to.
/// </summary>
internal static class PerformanceGuard
{
    private const float MinIntervalSeconds = 1f;

    /// <param name="run">The <see cref="LevelRoutines.Run"/> this loop belongs to; it ends once that level is over.</param>
    public static IEnumerator Routine(int run)
    {
        while (true)
        {
            var frameAtStart = Time.frameCount;
            var timeAtStart = Time.unscaledTime;

            // A zero or negative interval would measure a single frame's instantaneous (and noisy) rate.
            yield return new WaitForSeconds(Math.Max(MinIntervalSeconds, Settings.PerformanceCheckIntervalSeconds.Value));

            if (!LevelRoutines.IsCurrent(run))
                yield break;

            if (!Settings.PerformanceGuardEnabled.Value || !SNet.IsMaster)
                continue;

            // Wrapped like every other forever-loop in this mod: a bad read here must not throw out of
            // this while(true) and silently stop the performance guard for the rest of the level.
            try
            {
                var elapsed = Time.unscaledTime - timeAtStart;
                if (elapsed <= 0f)
                    continue;

                var averageFps = (Time.frameCount - frameAtStart) / elapsed;
                if (averageFps >= Settings.MinAcceptableFPS.Value)
                    continue;

                var removed = EnemyCleanup.DespawnHiddenEnemies(Settings.PerformanceDespawnBatchSize.Value);
                if (removed > 0)
                    Plugin.Logger.LogInfo(
                        $"CrashTestDummies: framerate dipped to {averageFps:F0} (below {Settings.MinAcceptableFPS.Value:F0}) - " +
                        $"recycled {removed} unseen dummies ({EnemyCleanup.AliveCount} left alive).");
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"CrashTestDummies: performance guard check failed and was skipped this pass: {e}");
            }
        }
    }
}
