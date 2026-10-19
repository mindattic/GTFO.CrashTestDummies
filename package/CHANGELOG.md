# Changelog

## v3

- Ambient scatter: every `AmbientIntervalSeconds` (default 60s), the host scatters a random
  `AmbientCountMin`-`AmbientCountMax` (default 50-100) more dummies between `AmbientSkipFirstMeters`
  and `AmbientMaxDistanceMeters` (default 10-100 m) of wherever the local player currently is. Toggle
  with `Ambient.AmbientSpawnEnabled`.
- Population cap, not a lifetime budget: `General.MaxTotalEnemies` (default 300) now caps how many of
  this mod's dummies may be alive *at once*, shared across all three spawn triggers. As dummies die or
  get recycled, room opens back up for more - it never permanently maxes out and goes quiet for the
  rest of the expedition.
- Cleanup: every `CleanupCheckIntervalSeconds` (default 30s), any dummy that's been alive longer than
  `CleanupAfterSeconds` (default 300s) and is currently out of sight gets quietly despawned (via the
  network replicator, same as a normal enemy removal), freeing its slot in the population cap. A dummy
  is never touched while a player might be looking at it. Toggle with `Cleanup.CleanupEnabled`.
- Never-seen guarantee, now everywhere: a shared out-of-sight check (`General.OutOfSightHalfAngleDegrees`,
  default 70°) - outside every player's view cone, or blocked by a line-of-sight check even if inside
  it - now gates all three spawn triggers (previously only the ambient scatter had it) and the cleanup
  despawn. Combined with cleanup, nothing this mod does should ever visibly pop in or out. The host
  can't tell where other players and bots are looking, so for them anything in their line of sight
  counts as seen, whichever way they face.
- The level-start scatter is guaranteed to run, and claim its share of the population cap, before the
  ambient scatter or cleanup loop start - so the initial 100 is never crowded out regardless of how
  aggressively `Ambient.*` is tuned.
- Nothing outlives its level: the ambient scatter, cleanup and any door ambush still spawning all stop
  the moment you leave the level (success, fail, abort to lobby or a checkpoint reload), and the
  ambient scatter and cleanup start again when you're back in after a checkpoint.

## v2

- Alarm door ambush: when a security door locked behind an alarm scan (chained puzzle) finishes
  opening, the host scatters `AmbushCount` (default 20) more sleeping dummies between
  `AmbushSkipFirstMeters` (10 m) and `AmbushMaxDistanceMeters` (100 m) of the far side of that
  doorway - same spot-picking as the level-start scatter, centered on the door instead of the drop
  point. Uses the existing spacing, enemy-type and sleep/aggressive settings. Toggle with
  `Ambush.AmbushOnAlarmDoors`.

## v1

- Initial release: on entering an expedition, the host scatters `SpawnCount` (default 100) sleeping
  Strikers between 10 m and 100 m from the drop point, at the level's own enemy spawn
  spots. Count, distance band, spacing, enemy type, delay and sleep/aggressive mode
  are configurable.
