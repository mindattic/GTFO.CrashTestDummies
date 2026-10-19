using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Player;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Every AmbientIntervalSeconds, scatters a random (AmbientCountMin-AmbientCountMax) number of more
/// dummies near the local player, skipping any spot currently inside a player's view cone, so they
/// never visibly pop in - they're just there next time you look.
/// </summary>
internal static class AmbientSpawner
{
    private const float MinIntervalSeconds = 1f;

    public static IEnumerator Routine()
    {
        while (true)
        {
            // A zero or negative interval would run a full scatter every frame.
            yield return new WaitForSeconds(Math.Max(MinIntervalSeconds, Settings.AmbientIntervalSeconds.Value));

            if (!Settings.ModEnabled.Value || !Settings.AmbientSpawnEnabled.Value || !SNet.IsMaster)
                continue;

            if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent == null)
                continue;

            var count = UnityEngine.Random.Range(Settings.AmbientCountMin.Value, Settings.AmbientCountMax.Value + 1);
            // Run through the safety wrapper: an exception from one ambient scatter must not end this
            // while(true) loop and silently stop ambient spawning for the rest of the level.
            yield return CoroutineSafety.Run(
                EnemySpawner.SpawnAround(
                    localAgent.transform.position,
                    Settings.AmbientSkipFirstMeters.Value,
                    Settings.AmbientMaxDistanceMeters.Value,
                    Settings.MinSpacingMeters.Value,
                    count,
                    point => Visibility.IsHiddenFromAllPlayers(point, Settings.OutOfSightHalfAngleDegrees.Value)),
                "ambient spawn");
        }
    }
}
