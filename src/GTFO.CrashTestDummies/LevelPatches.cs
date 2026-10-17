using BepInEx.Unity.IL2CPP.Utils.Collections;
using GTFO.CrashTestDummies.Core;
using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;

namespace GTFO.CrashTestDummies;

// PatchAll ignores classes without a class-level [HarmonyPatch], even if every method has one.
[HarmonyPatch]
internal static class LevelPatches
{
    private static readonly SpawnGate Gate = new();

    [HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
    [HarmonyPrefix]
    private static void Pre_Build()
    {
        Gate.Reset();
    }

    // The player agent already exists in the elevator while the level is still generating, so the
    // trigger is entering the InLevel state, after the cage has landed in the first zone.
    [HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Enter))]
    [HarmonyPostfix]
    private static void Post_EnterLevel()
    {
        if (!Gate.TryBegin(Settings.ModEnabled.Value, SNet.IsMaster))
            return;

        if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent == null)
        {
            Plugin.Logger.LogWarning("CrashTestDummies: entered the level without a local player agent - nothing spawned.");
            return;
        }

        Plugin.CoroutineHostInstance.StartCoroutine(EnemySpawner.SpawnRoutine(localAgent).WrapToIl2Cpp());
    }
}
