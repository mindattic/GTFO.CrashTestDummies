# GTFO.CrashTestDummies

Dev repo for **GTFO.CrashTestDummies**, a BepInEx IL2CPP plugin for GTFO that keeps a steady
population of sleeping Strikers alive around you to practice stealth or shooting on while testing
other mods. Three independent triggers feed the same population cap, each scattered in its own
10-100 m band around its own origin (not one band covering the whole level): once around your drop
point when you land, again around the far side of each alarm-locked security door as it opens, and
a recurring trickle near wherever you currently are. Every trigger, plus the cleanup that recycles
forgotten dummies, is gated by the same "is any player currently looking at this spot" check, so
nothing ever visibly pops in or out.

## How it works

All three triggers share the same core routine (`EnemySpawner.SpawnAround`): gather candidate spots
from every room (`AIG_CourseNode.s_allNodes`) near an origin point, optionally drop any that fail an
`isAllowed` predicate, shuffle what's left, and keep up to `count` that are between `skipMeters` and
`maxMeters` of that origin and at least `MinSpacingMeters` apart. What differs between triggers is the
origin, the distance/count settings, and (for door ambush and ambient) that `isAllowed` is the shared
visibility check. `count` is also clamped so `EnemyCleanup.AliveCount` (how many of this mod's
dummies are currently alive) never exceeds `MaxTotalEnemies` - a steady-state population cap, not a
one-time budget.

**Out of sight, for every trigger and for cleanup** (`Visibility.cs`, `OutOfSightHalfAngleDegrees`,
default 70°): a candidate point is hidden from a player if it's outside this many degrees of the
centre of their `FPSCamera`'s forward direction (from `PlayerAgent.EyePosition`), *or* if it's inside
that cone but a `Physics.Linecast` between their eye and the point hits something first (round a
corner, behind a crate). It's hidden only if every alive player in `PlayerManager.PlayerAgentsInLevel`
fails to see it. Only the local player has an `FPSCamera`, so on the host every client and bot is
treated as looking every way at once: for them only the linecast can hide a point.

**Startup ordering**: `Post_EnterLevel` runs a `StartupSequence` coroutine that `yield return`s the
on-drop scatter directly (a nested `IEnumerator` runs to completion before its caller resumes), and
only *then* starts the ambient and cleanup loops. So the on-drop 100 always gets first claim on the
population cap, regardless of how aggressively `Ambient.*` is tuned.

**On drop** (`SpawnCount`/`SkipFirstMeters`/`MaxDistanceMeters`, default 100 / 10 m / 100 m from the
drop point):

- A prefix on `LevelGeneration.Builder.Build` resets a once-per-expedition flag.
- A postfix on `GS_InLevel.Enter` fires once the cage has landed and you're standing in the first
  zone. (Not `LocalPlayerAgent.Setup`: the player agent already exists in the elevator while the
  level is still generating, so it has no course node yet.) On the host only (`SNet.IsMaster`), it
  starts a coroutine that waits `SpawnDelaySeconds`, then gathers candidate spots from every room
  (`AIG_CourseNode.s_allNodes`) near the drop point: each room's designer-placed
  `LG_Area.m_enemySpawnPoints` (with their authored facing) plus a few `GetRandomPositionInside()`
  points. The candidates are shuffled, then `SpawnPicker` keeps up to `SpawnCount` (default 100) that
  are between `SkipFirstMeters` (10) and `MaxDistanceMeters` (100) from the drop point and at least
  `MinSpacingMeters` (3) from each other. Each is spawned with `EnemyAllocator.Current.SpawnEnemy`
  on its own room's course node, in `Hibernate` mode unless `SpawnAsSleepers` is off.
- The dummy type comes from `GameDataBlockBase<EnemyDataBlock>`, limited to enabled blocks with a
  built prefab: of those whose name contains `EnemyNameFilter` (default `Striker_Hibernate`, the
  plain sleeping Striker), the one with the lowest max health is used. If nothing matches, it uses
  the weakest enemy in the game data. The chosen type is logged to `BepInEx/LogOutput.log`.

**On each alarm door** (`AmbushCount`/`AmbushSkipFirstMeters`/`AmbushMaxDistanceMeters`, default
20 / 10 m / 100 m from the far side of *that* door, wherever it is in the level):

- A postfix on `LevelGeneration.LG_SecurityDoor.OnSyncDoorStatusChange` watches every door's networked
  status. `DoorAmbush` remembers each door's previous `eDoorStatus` (keyed by Unity instance ID) so it
  can catch the one transition that matters: `Closed_LockedWithChainedPuzzle_Alarm` (an alarm-scan
  lock) going to `Open`. On that transition (host only), it reads the door's `Gate.Nodes` - the two
  AI-graph nodes the door connects - finds whichever player is standing at the door, and picks the
  node farther from them as "the other side."
- It then calls the same `EnemySpawner.SpawnAround` routine as the level-start scatter, but centered
  on that far node instead of the drop point, using the `Ambush.*` distance/count settings and the
  shared visibility filter. `MinSpacingMeters`, `EnemyNameFilter` and `SpawnAsSleepers` are shared
  across all triggers.

**Ambient trickle** (`AmbientSpawner.cs`; `AmbientCountMin`/`AmbientCountMax`/`AmbientSkipFirstMeters`/
`AmbientMaxDistanceMeters`, default 50-100 / 10 m / 100 m from the local player):

- An infinite coroutine loops `yield return new WaitForSeconds(AmbientIntervalSeconds)` (default 60),
  then calls `SpawnAround` centered on the local player's current position with a random count in
  `[AmbientCountMin, AmbientCountMax]` and the shared visibility filter as `isAllowed`.
- Started from `StartupSequence` only after the on-drop scatter finishes, and the returned `Coroutine`
  is stopped in `Pre_Build` so retrying a level doesn't stack up extra loops (`CoroutineHost` is a
  plugin-level component that outlives any single expedition).

**Cleanup** (`EnemyCleanup.cs`; `CleanupCheckIntervalSeconds`/`CleanupAfterSeconds`, default 30 s / 300 s):

- Every spawned `EnemyAgent` (from all three triggers) is registered with `EnemyCleanup.Track` the
  moment `EnemyAllocator.Current.SpawnEnemy` returns it. `EnemyCleanup.AliveCount` prunes dead/null
  entries as it counts, and is what `SpawnAround` checks against `MaxTotalEnemies` before spawning.
- A second infinite coroutine, also stopped in `Pre_Build`, wakes every `CleanupCheckIntervalSeconds`
  and recycles any tracked dummy that's both older than `CleanupAfterSeconds` and currently hidden
  (same `Visibility` check) by calling `agent.m_replicator.Despawn()` - the same `SNet_Replicator`
  method the game's own networked spawn/despawn lifecycle uses, so it's removed cleanly for every
  client, not just locally destroyed.

`LevelPatches` needs its class-level `[HarmonyPatch]`: `PatchAll` skips classes without one.

The spawn API and patch points were taken from ExtraEnemyCustomization's `SpawnEnemyAbility` and
TheArchive, and checked against the interop assemblies in the DEV profile.

## Layout

```
src/GTFO.CrashTestDummies/        the plugin project (Harmony patches, game calls)
  Shared/                         generated from GTFO.EZ's src/GTFO.EZ/Shared by its tools/publish-shared.ps1 -
                                  never edit here: the Insert/Delete infinite ammo/health toggles
src/GTFO.CrashTestDummies.Core/   pure spawn logic, no game dependency: spot picking, enemy-type
                                  selection, once-per-level gate; ships alongside the plugin DLL
tests/                            GTFO.CrashTestDummies.Core.Tests - NUnit, plain `dotnet test`, no game needed
package/                     what ships to Thunderstore / gets zipped (manifest name "GtfoCrashTestDummies")
  icon.png                   256x256, resized from reference/logo-1254x1254.png
reference/                   source art
build.ps1                    builds into the profile and packages dist/GTFO.CrashTestDummies-<version>.zip
```

## Building

Requires BepInExPack_GTFO in a Thunderstore Mod Manager profile that has been launched once, so
`BepInEx/interop` exists (this project compiles against the game's interop assemblies).

```powershell
.\build.ps1                      # Thunderstore Mod Manager profile "Easy Mode DEV"
.\build.ps1 -Profile MyProfile
```

## Testing

```powershell
dotnet test tests/GTFO.CrashTestDummies.Core.Tests/GTFO.CrashTestDummies.Core.Tests.csproj
```

No game install or BepInEx profile needed. This covers `GTFO.CrashTestDummies.Core`: spot
picking (distance band from an arbitrary origin, spacing, count cap - the same `SpawnPicker` logic
underlies all three spawn triggers, just with different origins and band/count settings), enemy-type
selection (weakest name match, Big Strikers skipped, disabled/no-prefab/zero-health blocks excluded,
fallback when nothing matches), and the once-per-level host-only gate. The visibility check
(`Visibility.cs`), population tracking (`EnemyCleanup.cs`) and ambient/cleanup coroutines live in the
game-dependent project since they need `PlayerAgent`, `FPSCamera` and `Physics.Linecast` - they're
untested by `dotnet test` and only verifiable in-game.

The Harmony patches and game calls in `src/GTFO.CrashTestDummies/` only run inside the game, so
check those in-game after `build.ps1`: launch any rundown in the profile. `LogOutput.log` should show
`GTFO.CrashTestDummies v3.0.0 loaded`, then on landing the chosen dummy type and
`spawning 100x 'Striker_Hibernate' (Hibernate).` Open an alarm-locked security door and it should log
`alarm door opened - ambushing the far side near ...`, then `spawning 20x 'Striker_Hibernate' (Hibernate).`
About a minute later it should log another `spawning Nx 'Striker_Hibernate' (Hibernate).` (N between 50
and 100) from the ambient trickle, and after dummies have sat unseen for 5+ minutes, occasional
`recycled N forgotten dummies (M still alive).` lines from cleanup. Worth confirming visually too: no
dummy should ever appear to pop in or out while you're looking at the spot it occupies.

## Versioning

Major-only bumps (v1, v2, ...), written as `1.0.0` in `manifest.json`.

## Publishing to Thunderstore

`dist/GTFO.CrashTestDummies-<version>.zip` is ready to upload as-is.
