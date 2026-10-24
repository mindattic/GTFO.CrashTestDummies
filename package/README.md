# GTFO.CrashTestDummies

A testing tool that keeps a steady population of sleeping Strikers alive around you, so you always
have something to stealth or shoot while trying out other mods - and you'll never see one appear or
disappear. Three triggers feed the same population, each scattered in its own 10-100 m band around
its own origin (not one band covering the whole level):

- **On drop**: 100 sleeping Strikers are scattered 10-100 m from where you land, at the level's own
  enemy spawn spots. The elevator drop area itself stays clear. This always runs first and claims its
  share of the population before anything else does.
- **On each alarm door**: the moment a security door locked behind an alarm scan finishes opening,
  20 more are scattered 10-100 m past the far side of *that* doorway - wherever in the level it is.
- **Ambient trickle**: every 60 seconds, 50-100 more are scattered 10-100 m from wherever you
  currently are.

Every spawn - all three triggers - only ever picks a spot no player can currently see: out of the
host's view or behind a wall, and behind a wall for everyone else (the host can't tell which way
other players and bots are facing), so nothing ever visibly pops in.

**Population cap, not a one-time budget.** Up to 300 of this mod's dummies may be alive at once
(combined across all three triggers). It's a ceiling on the current count, not a lifetime total:
every 30 seconds, any dummy that's been alive for 5+ minutes, is still untouched (unhurt and not
fighting) and is currently out of sight gets quietly recycled, freeing its slot - so the population
keeps refilling with fresh dummies instead of permanently maxing out and going quiet for the rest of
the run.

## Config

`BepInEx/config/MindAttic.GTFO.CrashTestDummies.cfg`:

| Setting | Default | Meaning |
|---|---|---|
| `0. Mod.Enabled` | `true` | Turn the whole mod on or off. When false, the mod loads nothing: no patches, no hotkeys, no dummies. It's the file's first entry, so the mod can be switched off where a mod manager profile doesn't list it. Takes effect on restart. Replaces the old `General.Enabled`; a saved `false` there carries over. |
| `General.MaxTotalEnemies` | `300` | Population cap: how many of this mod's dummies may be alive at once, combined across all triggers. |
| `General.OutOfSightHalfAngleDegrees` | `70` | Used by every trigger and by cleanup: a spot is hidden from the host if it's outside this many degrees of the centre of their view, or something blocks line of sight to it. Other players and bots count as looking every way, so only a blocked line of sight hides a spot from them. |
| `Spawning.SpawnCount` | `100` | How many enemies the on-drop scatter spawns. |
| `Spawning.SkipFirstMeters` | `10` | Nothing spawns closer than this to the drop point. |
| `Spawning.MaxDistanceMeters` | `100` | Nothing spawns farther than this from the drop point. |
| `Spawning.MinSpacingMeters` | `3` | Minimum gap between any two enemies (shared by every trigger). |
| `Spawning.EnemyNameFilter` | `Striker_Hibernate` | Case-insensitive match against enemy data block names; the weakest match is spawned (shared by every trigger). If nothing matches, the enemy with the lowest max health is used. |
| `Spawning.SpawnDelaySeconds` | `2` | Wait after you spawn in before the on-drop scatter runs. |
| `Spawning.SpawnAsSleepers` | `true` | `true` spawns them asleep; `false` spawns them awake and aggressive (shared by every trigger). |
| `Ambush.AmbushOnAlarmDoors` | `true` | When an alarm-locked security door finishes opening, scatter extra dummies past it. |
| `Ambush.AmbushCount` | `20` | How many enemies to scatter on the far side of an alarm door when it opens. |
| `Ambush.AmbushSkipFirstMeters` | `10` | Nothing spawns closer than this to the far side of the doorway. |
| `Ambush.AmbushMaxDistanceMeters` | `100` | Nothing spawns farther than this from the far side of the doorway. |
| `Ambient.AmbientSpawnEnabled` | `true` | Master switch for the ambient trickle. |
| `Ambient.AmbientIntervalSeconds` | `60` | How often, in seconds, the ambient trickle runs (at least 1). |
| `Ambient.AmbientCountMin` / `AmbientCountMax` | `50` / `100` | A random count in this range is scattered each time the ambient trickle runs. |
| `Ambient.AmbientSkipFirstMeters` | `10` | Nothing spawns closer than this to you. |
| `Ambient.AmbientMaxDistanceMeters` | `100` | Nothing spawns farther than this from you. |
| `Cleanup.CleanupEnabled` | `true` | Master switch for recycling forgotten dummies. |
| `Cleanup.CleanupCheckIntervalSeconds` | `30` | How often, in seconds, to check for dummies to recycle (at least 1). |
| `Cleanup.CleanupAfterSeconds` | `300` | A dummy is only recycled once it's been alive this long *and* is currently out of sight. One that's been hurt or is awake and fighting is never recycled (so with `SpawnAsSleepers` off, nothing is). |

Only the host spawns or recycles enemies.
