using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using GTFO.CrashTestDummies.Core;
using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;
using UnityEngine;

namespace GTFO.CrashTestDummies;

// PatchAll ignores classes without a class-level [HarmonyPatch], even if every method has one.
[HarmonyPatch]
internal static class LevelPatches
{
    private static readonly SpawnGate Gate = new();

    // CoroutineHost is a plugin-level component that outlives any single expedition, so these infinite
    // loops have to be stopped by hand when a new level starts building, or retrying a level would stack
    // up one running loop per attempt.
    private static Coroutine? _ambientRoutine;
    private static Coroutine? _cleanupRoutine;

    [HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
    [HarmonyPrefix]
    private static void Pre_Build()
    {
        Gate.Reset();
        EnemyCleanup.ResetForNewExpedition();
        StopRoutines();
    }

    // The ambient and cleanup loops otherwise only stop at the next Builder.Build, so without this
    // they keep running - and the ambient loop can keep spawning - all through the success/fail
    // end screen, right up until the next expedition actually starts generating.
    [HarmonyPatch(typeof(GS_ExpeditionSuccess), nameof(GS_ExpeditionSuccess.Enter))]
    [HarmonyPostfix]
    private static void Post_ExpeditionSuccess() => StopRoutines();

    [HarmonyPatch(typeof(GS_ExpeditionFail), nameof(GS_ExpeditionFail.Enter))]
    [HarmonyPostfix]
    private static void Post_ExpeditionFail() => StopRoutines();

    private static void StopRoutines()
    {
        if (_ambientRoutine != null)
        {
            Plugin.CoroutineHostInstance.StopCoroutine(_ambientRoutine);
            _ambientRoutine = null;
        }

        if (_cleanupRoutine != null)
        {
            Plugin.CoroutineHostInstance.StopCoroutine(_cleanupRoutine);
            _cleanupRoutine = null;
        }
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

        Plugin.CoroutineHostInstance.StartCoroutine(StartupSequence(localAgent).WrapToIl2Cpp());
    }

    // Runs the initial drop-point scatter to completion before the ambient spawner or cleanup loop get a
    // chance to touch the population cap, so the initial 100 is never starved by a low AmbientIntervalSeconds.
    private static IEnumerator StartupSequence(PlayerAgent localAgent)
    {
        // Safety-wrapped: an exception thrown mid-scatter must not skip the statements below it and
        // leave ambient spawning and cleanup never started for the whole level.
        yield return CoroutineSafety.Run(EnemySpawner.SpawnRoutine(localAgent), "startup scatter");

        _ambientRoutine = Plugin.CoroutineHostInstance.StartCoroutine(AmbientSpawner.Routine().WrapToIl2Cpp());
        _cleanupRoutine = Plugin.CoroutineHostInstance.StartCoroutine(EnemyCleanup.Routine().WrapToIl2Cpp());
    }

    [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.OnSyncDoorStatusChange))]
    [HarmonyPostfix]
    private static void Post_SecurityDoorStatusChange(LG_SecurityDoor __instance, pDoorState state, bool isRecall)
    {
        if (isRecall)
            return;

        DoorAmbush.OnSecurityDoorStatusChanged(__instance, state.status);
    }
}
