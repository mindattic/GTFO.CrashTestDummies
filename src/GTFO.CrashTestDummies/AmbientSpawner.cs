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
    public static IEnumerator Routine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Settings.AmbientIntervalSeconds.Value);

            if (!Settings.ModEnabled.Value || !Settings.AmbientSpawnEnabled.Value || !SNet.IsMaster)
                continue;

            if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent == null)
                continue;

            var count = UnityEngine.Random.Range(Settings.AmbientCountMin.Value, Settings.AmbientCountMax.Value + 1);
            yield return EnemySpawner.SpawnAround(
                localAgent.transform.position,
                Settings.AmbientSkipFirstMeters.Value,
                Settings.AmbientMaxDistanceMeters.Value,
                Settings.MinSpacingMeters.Value,
                count,
                point => Visibility.IsHiddenFromAllPlayers(point, Settings.OutOfSightHalfAngleDegrees.Value));
        }
    }
}
