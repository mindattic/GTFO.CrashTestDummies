# GTFO.CrashTestDummies

Dev repo for **GTFO.CrashTestDummies**, a BepInEx IL2CPP plugin for GTFO that scatters 100 sleeping
Strikers through the level around your drop point, so you have targets to stealth
or shoot while testing other mods.

## How it works

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
- `LevelPatches` needs its class-level `[HarmonyPatch]`: `PatchAll` skips classes without one.

The spawn API and patch points were taken from ExtraEnemyCustomization's `SpawnEnemyAbility` and
TheArchive, and checked against the interop assemblies in the DEV profile.

## Layout

```
src/GTFO.CrashTestDummies/        the plugin project (Harmony patches, game calls)
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
picking (10-100 m band, spacing, count cap), enemy-type selection (weakest name match,
Big Strikers skipped, disabled/no-prefab/zero-health blocks excluded, fallback when nothing
matches), and the once-per-level host-only gate.

The Harmony patches and game calls in `src/GTFO.CrashTestDummies/` only run inside the game, so
check those in-game after `build.ps1`: launch any rundown in the profile. `LogOutput.log` should show
`GTFO.CrashTestDummies v1.0.0 loaded`, then on landing the chosen dummy type and
`spawning 100x 'Striker_Hibernate' (Hibernate).`

## Versioning

Major-only bumps (v1, v2, ...), written as `1.0.0` in `manifest.json`.

## Publishing to Thunderstore

`dist/GTFO.CrashTestDummies-<version>.zip` is ready to upload as-is.
