using System.Collections;
using System.Collections.Generic;
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

    private static bool IsUntouched(EnemyAgent agent)
    {
        if (agent.AI != null && agent.AI.Mode == Agents.AgentMode.Agressive)
            return false;
        var damage = agent.Damage;
        return damage == null || damage.Health >= damage.HealthMax - 0.01f;
    }

    public static IEnumerator Routine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Settings.CleanupCheckIntervalSeconds.Value);

            if (!Settings.ModEnabled.Value || !Settings.CleanupEnabled.Value || !SNet.IsMaster)
                continue;

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
    }
}
