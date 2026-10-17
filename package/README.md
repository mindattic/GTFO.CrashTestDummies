# GTFO.CrashTestDummies

A testing tool: when you drop into an expedition, 100 sleeping Strikers are scattered
through the level 10-100 m from where you land, at the level's own enemy spawn spots, so you always
have something to stealth or shoot while trying out other mods. The elevator drop area stays clear.

## Config

`BepInEx/config/MindAttic.GTFO.CrashTestDummies.cfg`:

| Setting | Default | Meaning |
|---|---|---|
| `General.Enabled` | `true` | Master on/off switch. |
| `Spawning.SpawnCount` | `100` | How many enemies to scatter. |
| `Spawning.SkipFirstMeters` | `10` | Nothing spawns closer than this to the drop point. |
| `Spawning.MaxDistanceMeters` | `100` | Nothing spawns farther than this from the drop point. |
| `Spawning.MinSpacingMeters` | `3` | Minimum gap between any two enemies. |
| `Spawning.EnemyNameFilter` | `Striker_Hibernate` | Case-insensitive match against enemy data block names; the weakest match is spawned. If nothing matches, the enemy with the lowest max health is used. |
| `Spawning.SpawnDelaySeconds` | `2` | Wait after you spawn in before the enemies appear. |
| `Spawning.SpawnAsSleepers` | `true` | `true` spawns them asleep; `false` spawns them awake and aggressive. |

Only the host spawns enemies, once per expedition.
