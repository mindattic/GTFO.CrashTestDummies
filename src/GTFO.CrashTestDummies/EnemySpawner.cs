using System.Collections;
using Agents;
using AIGraph;
using Enemies;
using GameData;
using GTFO.CrashTestDummies.Core;
using Player;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

internal static class EnemySpawner
{
    private const int RandomPointsPerRoom = 15;

    // A room's centre can sit well away from its far corners, so look at rooms a bit past the max distance.
    private const float RoomReachMargin = 40f;

    private static EnemyDataBlock? _cachedTarget;
    private static string? _cachedFilter;

    /// <summary>
    /// Waits briefly for the level to settle, then scatters sleeping enemies around the level
    /// between SkipFirstMeters and MaxDistanceMeters from <paramref name="localAgent"/>'s drop point.
    /// Host-only: enemy spawning is authoritative on the master client.
    /// </summary>
    public static IEnumerator SpawnRoutine(PlayerAgent localAgent)
    {
        yield return new WaitForSeconds(Settings.SpawnDelaySeconds.Value);

        if (!SNet.IsMaster)
            yield break;

        if (localAgent == null || localAgent.CourseNode == null)
        {
            Plugin.Logger.LogWarning("CrashTestDummies: the local player has no course node yet - nothing spawned.");
            yield break;
        }

        yield return SpawnAround(
            localAgent.transform.position,
            Settings.SkipFirstMeters.Value,
            Settings.MaxDistanceMeters.Value,
            Settings.MinSpacingMeters.Value,
            Settings.SpawnCount.Value,
            point => Visibility.IsHiddenFromAllPlayers(point, Settings.OutOfSightHalfAngleDegrees.Value));
    }

    /// <summary>
    /// Host-only: scatters up to <paramref name="count"/> enemies between <paramref name="skipMeters"/> and
    /// <paramref name="maxMeters"/> from <paramref name="origin"/>, at the level's own enemy spawn spots and
    /// random walkable points in nearby rooms. Shared by the level-start scatter, the door-triggered ambush,
    /// and the periodic ambient spawn. <paramref name="isAllowed"/>, if given, drops any candidate spot it
    /// rejects (e.g. spots currently in a player's view) before spots are picked. <paramref name="count"/>
    /// is clamped so the number of dummies <see cref="EnemyCleanup"/> is currently tracking as alive never
    /// exceeds <c>MaxTotalEnemies</c> - a steady-state population cap, not a one-time budget.
    /// </summary>
    public static IEnumerator SpawnAround(
        Vector3 origin, float skipMeters, float maxMeters, float minSpacing, int count,
        Func<Vector3, bool>? isAllowed = null)
    {
        var target = GetTargetEnemy();
        if (target == null)
        {
            Plugin.Logger.LogError("CrashTestDummies: no eligible enemy data block found - nothing spawned.");
            yield break;
        }

        var remaining = Settings.MaxTotalEnemies.Value - EnemyCleanup.AliveCount;
        if (remaining <= 0)
        {
            Plugin.Logger.LogInfo(
                $"CrashTestDummies: MaxTotalEnemies ({Settings.MaxTotalEnemies.Value}) already alive - nothing spawned.");
            yield break;
        }

        var cappedCount = Math.Min(count, remaining);

        var mode = Settings.SpawnAsSleepers.Value ? AgentMode.Hibernate : AgentMode.Agressive;
        // Drop spots outside the distance band before the (raycasting, per-player) isAllowed check, so
        // it only runs on spots SpawnPicker could actually use.
        var minSqr = skipMeters > 0f ? skipMeters * skipMeters : 0f;
        var maxSqr = maxMeters * maxMeters;
        var spots = GatherSpawnSpots(origin, maxMeters)
            .Where(s =>
            {
                var sqr = (s.Position - origin).sqrMagnitude;
                return sqr >= minSqr && sqr <= maxSqr;
            })
            .ToList();
        if (isAllowed != null)
            spots = spots.Where(s => isAllowed(s.Position)).ToList();
        Shuffle(spots);

        var picks = SpawnPicker.Pick(
            spots.Select(s => ToNumerics(s.Position)).ToList(),
            ToNumerics(origin),
            skipMeters,
            maxMeters,
            minSpacing,
            cappedCount);

        if (picks.Count < cappedCount)
            Plugin.Logger.LogWarning(
                $"CrashTestDummies: only {picks.Count} of {cappedCount} spots fit the distance and spacing rules.");

        Plugin.Logger.LogInfo($"CrashTestDummies: spawning {picks.Count}x '{target.name}' ({mode}).");

        for (var i = 0; i < picks.Count; i++)
        {
            // Re-check the live count, not just the snapshot taken before this loop started: a door
            // ambush and an ambient scatter both pass that snapshot check before either has spawned
            // anything, and this loop yields every 5 spawns, so only a check against the current
            // total - taken again on every iteration - keeps two overlapping spawns under the cap.
            if (EnemyCleanup.AliveCount >= Settings.MaxTotalEnemies.Value)
            {
                Plugin.Logger.LogInfo(
                    $"CrashTestDummies: MaxTotalEnemies ({Settings.MaxTotalEnemies.Value}) reached mid-spawn - stopping early.");
                break;
            }

            var spot = spots[picks[i]];
            var agent = EnemyAllocator.Current.SpawnEnemy(target.persistentID, spot.Node, mode, spot.Position, spot.Rotation, null);
            EnemyCleanup.Track(agent);

            if (i % 5 == 4)
                yield return null;
        }

        Plugin.Logger.LogInfo(
            $"CrashTestDummies: done spawning ({EnemyCleanup.AliveCount}/{Settings.MaxTotalEnemies.Value} of the population cap alive).");
    }

    private readonly record struct SpawnSpot(AIG_CourseNode Node, Vector3 Position, Quaternion Rotation);

    // Each nearby room offers its designer-placed enemy spawn points (where the game puts its own sleepers)
    // plus a few random walkable points, so even rooms without authored points get some.
    private static List<SpawnSpot> GatherSpawnSpots(Vector3 origin, float maxMeters)
    {
        var spots = new List<SpawnSpot>();
        var nodes = AIG_CourseNode.s_allNodes;
        if (nodes == null)
            return spots;

        var reach = maxMeters + RoomReachMargin;
        for (var n = 0; n < nodes.Count; n++)
        {
            var node = nodes[n];
            if (node == null || Vector3.Distance(node.Position, origin) > reach)
                continue;

            var authored = node.m_area?.m_enemySpawnPoints;
            if (authored != null)
            {
                for (var i = 0; i < authored.Count; i++)
                {
                    var point = authored[i];
                    if (point != null)
                        spots.Add(new SpawnSpot(node, point.position, point.rotation));
                }
            }

            for (var i = 0; i < RandomPointsPerRoom; i++)
                spots.Add(new SpawnSpot(node, node.GetRandomPositionInside(),
                    Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)));
        }

        return spots;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static System.Numerics.Vector3 ToNumerics(Vector3 v) => new(v.x, v.y, v.z);

    private static EnemyDataBlock? GetTargetEnemy()
    {
        var filter = Settings.EnemyNameFilter.Value;
        if (_cachedTarget != null && _cachedFilter == filter)
            return _cachedTarget;

        var allBlocks = GameDataBlockBase<EnemyDataBlock>.GetAllBlocks();
        if (allBlocks == null || allBlocks.Length == 0)
            return null;

        var blocksById = new Dictionary<uint, EnemyDataBlock>();
        var candidates = new List<EnemyCandidate>();
        foreach (var block in allBlocks)
        {
            if (block == null)
                continue;

            var balancing = EnemyBalancingDataBlock.GetBlock(block.BalancingDataId);
            blocksById[block.persistentID] = block;
            candidates.Add(new EnemyCandidate(
                block.persistentID,
                block.name,
                block.internalEnabled,
                EnemyPrefabManager.GetEnemyPrefab(block.persistentID) != null,
                balancing?.Health.HealthMax ?? 0f));
        }

        var selection = DummySelector.Select(candidates, filter);
        if (selection.FilterUnmatched)
            Plugin.Logger.LogWarning(
                $"CrashTestDummies: no enemy matched filter '{filter}', falling back to the weakest enemy by max health.");

        var chosen = selection.Chosen;
        if (chosen != null)
            Plugin.Logger.LogInfo($"CrashTestDummies: dummy type is '{chosen.Name}' (id {chosen.Id}, max health {chosen.HealthMax}).");

        _cachedTarget = chosen != null ? blocksById[chosen.Id] : null;
        _cachedFilter = filter;
        return _cachedTarget;
    }
}
