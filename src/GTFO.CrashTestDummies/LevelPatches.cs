using System.Collections;
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

    // Set once this expedition's startup scatter has been started, so re-entering InLevel in the same
    // expedition (a checkpoint reload) restarts the ambient and cleanup loops without a second scatter.
    private static bool _expeditionStarted;

    // Whether the ambient and cleanup loops are running for the current InLevel stay.
    private static bool _loopsStarted;

    [HarmonyPatch(typeof(Builder), nameof(Builder.Build))]
    [HarmonyPrefix]
    private static void Pre_Build()
    {
        Gate.Reset();
        _expeditionStarted = false;
        EnemyCleanup.ResetForNewExpedition();
        DoorAmbush.ResetForNewExpedition();
        StopRoutines();
    }

    // Leaving InLevel ends the level for this mod's purposes: expedition success or fail, aborting to
    // the lobby, or dropping back to a checkpoint. Without this the loops (and any door ambush still
    // spawning) only stopped at the next Builder.Build.
    [HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Exit))]
    [HarmonyPostfix]
    private static void Post_ExitLevel() => StopRoutines();

    [HarmonyPatch(typeof(GS_ExpeditionSuccess), nameof(GS_ExpeditionSuccess.Enter))]
    [HarmonyPostfix]
    private static void Post_ExpeditionSuccess() => StopRoutines();

    [HarmonyPatch(typeof(GS_ExpeditionFail), nameof(GS_ExpeditionFail.Enter))]
    [HarmonyPostfix]
    private static void Post_ExpeditionFail() => StopRoutines();

    private static void StopRoutines()
    {
        LevelRoutines.StopAll();
        _loopsStarted = false;
    }

    // The player agent already exists in the elevator while the level is still generating, so the
    // trigger is entering the InLevel state, after the cage has landed in the first zone.
    [HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Enter))]
    [HarmonyPostfix]
    private static void Post_EnterLevel()
    {
        if (!Gate.TryBegin(SNet.IsMaster))
        {
            // Back in the level after leaving it (a checkpoint reload): the loops were stopped on the
            // way out, so start them again. The drop-point scatter stays once per expedition.
            if (_expeditionStarted && SNet.IsMaster)
                StartLoops();
            return;
        }

        // The gate is spent either way, so mark the expedition started before the agent check: bailing
        // out first would leave the ambient and cleanup loops unstarted for the whole expedition, a
        // checkpoint reload included (that path only restarts them for a started expedition).
        _expeditionStarted = true;

        if (!PlayerManager.TryGetLocalPlayerAgent(out var localAgent) || localAgent == null)
        {
            Plugin.Logger.LogWarning("CrashTestDummies: entered the level without a local player agent - skipping the drop-point scatter.");
            StartLoops();
            return;
        }

        LevelRoutines.Start(StartupSequence(localAgent, LevelRoutines.Run));
    }

    // Runs the initial drop-point scatter to completion before the ambient spawner or cleanup loop get a
    // chance to touch the population cap, so it's never starved by a low AmbientIntervalSeconds.
    private static IEnumerator StartupSequence(PlayerAgent localAgent, int run)
    {
        // Safety-wrapped: an exception thrown mid-scatter must not skip the statements below it and
        // leave ambient spawning and cleanup never started for the whole level.
        yield return CoroutineSafety.Run(EnemySpawner.SpawnRoutine(localAgent), "startup scatter");

        // The level may have ended while the scatter ran; never start loops for a level that's over.
        if (!LevelRoutines.IsCurrent(run))
            yield break;

        StartLoops();
    }

    private static void StartLoops()
    {
        if (_loopsStarted)
            return;

        _loopsStarted = true;
        var run = LevelRoutines.Run;
        LevelRoutines.Start(AmbientSpawner.Routine(run));
        LevelRoutines.Start(EnemyCleanup.Routine(run));
        LevelRoutines.Start(PerformanceGuard.Routine(run));
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
