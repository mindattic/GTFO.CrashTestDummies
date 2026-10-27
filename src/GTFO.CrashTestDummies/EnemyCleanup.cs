using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>
/// Tracks every dummy this mod has spawned so <see cref="AliveCount"/> reflects how many are currently
/// alive (a steady-state population, not a lifetime total), and recycles ones that have sat untouched
/// and out of sight for a while so the population keeps refilling instead of permanently maxing out.
/// </summary>
internal static class EnemyCleanup
{
    private readonly record struct TrackedEnemy(EnemyAgent Agent, float SpawnTime);

    private const float MinIntervalSeconds = 1f;

    private static readonly List<TrackedEnemy> Tracked = new();

    public static void Track(EnemyAgent? agent)
    {
        if (agent != null)
            Tracked.Add(new TrackedEnemy(agent, Time.time));
    }

    /// <summary>How many tracked dummies are still alive. Prunes any that are gone as it counts.</summary>
    public static int AliveCount
    {
        get
        {
            Tracked.RemoveAll(t => t.Agent == null || !t.Agent.Alive);
            return Tracked.Count;
        }
    }

    public static void ResetForNewExpedition() => Tracked.Clear();

    /// <summary>
    /// Emergency release valve for <see cref="PerformanceGuard"/>: immediately recycles up to
    /// <paramref name="maxCount"/> tracked dummies that are currently hidden from every player, oldest
    /// first, regardless of how long they've been alive or whether they're mid-fight. Unlike the
    /// age-gated sweep in <see cref="Routine"/>, this doesn't wait for CleanupAfterSeconds or require
    /// the dummy to be untouched - it only has to be out of sight, so framerate recovery never costs a
    /// player a dummy they can actually see. Returns how many were recycled.
    /// </summary>
    public static int DespawnHiddenEnemies(int maxCount)
    {
        if (maxCount <= 0)
            return 0;

        var halfAngle = Settings.OutOfSightHalfAngleDegrees.Value;
        var candidates = Tracked
            .Where(t => t.Agent != null && t.Agent.Alive)
            .OrderBy(t => t.SpawnTime)
            .ToList();

        var removed = 0;
        foreach (var entry in candidates)
        {
            if (removed >= maxCount)
                break;

            if (!Visibility.IsHiddenFromAllPlayers(entry.Agent.transform.position, halfAngle))
                continue;

            entry.Agent.m_replicator?.Despawn();
            Tracked.Remove(entry);
            removed++;
        }

        return removed;
    }

    private static bool IsUntouched(EnemyAgent agent)
    {
        if (agent.AI != null && agent.AI.Mode == Agents.AgentMode.Agressive)
            return false;
        var damage = agent.Damage;
        return damage == null || damage.Health >= damage.HealthMax - 0.01f;
    }

    /// <param name="run">The <see cref="LevelRoutines.Run"/> this loop belongs to; it ends once that level is over.</param>
    public static IEnumerator Routine(int run)
    {
        while (true)
        {
            // A zero or negative interval would sweep every tracked dummy every frame.
            yield return new WaitForSeconds(Math.Max(MinIntervalSeconds, Settings.CleanupCheckIntervalSeconds.Value));

            if (!LevelRoutines.IsCurrent(run))
                yield break;

            if (!Settings.CleanupEnabled.Value || !SNet.IsMaster)
                continue;

            // Wrapped like every other forever-loop in this mod: a bad agent (a destroyed replicator, a
            // torn-down camera) must not throw out of this while(true) and silently stop cleanup - and
            // with it the population cap's recycling - for the rest of the level.
            try
            {
                var now = Time.time;
                var removed = 0;
                for (var i = Tracked.Count - 1; i >= 0; i--)
                {
                    var entry = Tracked[i];
                    if (entry.Agent == null || !entry.Agent.Alive)
                    {
                        Tracked.RemoveAt(i);
                        continue;
                    }

                    if (now - entry.SpawnTime < Settings.CleanupAfterSeconds.Value)
                        continue;

                    // Only untouched dummies are recycled: one that's been hurt or is fighting stays.
                    if (!IsUntouched(entry.Agent))
                        continue;

                    if (!Visibility.IsHiddenFromAllPlayers(entry.Agent.transform.position, Settings.OutOfSightHalfAngleDegrees.Value))
                        continue;

                    entry.Agent.m_replicator?.Despawn();
                    Tracked.RemoveAt(i);
                    removed++;
                }

                if (removed > 0)
                    Plugin.Logger.LogInfo($"CrashTestDummies: recycled {removed} forgotten dummies ({AliveCount} still alive).");
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"CrashTestDummies: cleanup sweep failed and was skipped this pass: {e}");
            }
        }
    }
}
