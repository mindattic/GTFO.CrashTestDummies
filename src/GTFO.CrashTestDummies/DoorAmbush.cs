using System.Collections.Generic;
using AIGraph;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using LevelGeneration;
using Player;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

/// <summary>Scatters extra dummies on the far side of a security door the moment an alarm scan unlocks it.</summary>
internal static class DoorAmbush
{
    // Doors (by Unity instance ID) seen alarm-locked and not yet opened. An alarm door passes through
    // several states between locked and open (puzzle activated, unlocked, opening), so it's the
    // "was alarm-locked" fact that's remembered, not just the last status; a door that was never
    // alarm-locked never ambushes, and each one ambushes once.
    private static readonly HashSet<int> AlarmLocked = new();

    public static void OnSecurityDoorStatusChanged(LG_SecurityDoor? door, eDoorStatus status)
    {
        if (door == null || !Settings.ModEnabled.Value || !Settings.AmbushOnAlarmDoors.Value || !SNet.IsMaster)
            return;

        var key = door.GetInstanceID();
        if (status == eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm)
        {
            AlarmLocked.Add(key);
            return;
        }

        if (status != eDoorStatus.Open || !AlarmLocked.Remove(key))
            return;

        var farNode = FindFarSideNode(door);
        if (farNode == null)
        {
            Plugin.Logger.LogWarning("CrashTestDummies: alarm door opened but no far-side node was found - nothing spawned.");
            return;
        }

        Plugin.Logger.LogInfo($"CrashTestDummies: alarm door opened - ambushing the far side near {farNode.Position}.");
        Plugin.CoroutineHostInstance.StartCoroutine(
            EnemySpawner.SpawnAround(
                farNode.Position,
                Settings.AmbushSkipFirstMeters.Value,
                Settings.AmbushMaxDistanceMeters.Value,
                Settings.MinSpacingMeters.Value,
                Settings.AmbushCount.Value,
                point => Visibility.IsHiddenFromAllPlayers(point, Settings.OutOfSightHalfAngleDegrees.Value)).WrapToIl2Cpp());
    }

    // A security door's gate connects exactly the two AI-graph nodes on either side of it. Whichever one
    // is farther from whoever is standing at the door is "the other side".
    private static AIG_CourseNode? FindFarSideNode(LG_SecurityDoor door)
    {
        var nodes = door.Gate?.Nodes;
        if (nodes == null || nodes.Count == 0)
            return null;

        var triggerPos = door.transform.position;
        if (PlayerManager.TryGetCloseEnoughPlayerAgent(triggerPos, 15f, out var nearPlayer) && nearPlayer != null)
            triggerPos = nearPlayer.transform.position;

        AIG_CourseNode? farthest = null;
        var farthestDist = -1f;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i]?.TryCast<AIG_CourseNode>();
            if (node == null)
                continue;

            var dist = Vector3.Distance(node.Position, triggerPos);
            if (dist > farthestDist)
            {
                farthestDist = dist;
                farthest = node;
            }
        }

        return farthest;
    }
}
