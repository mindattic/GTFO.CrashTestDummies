using BepInEx.Configuration;

namespace GTFO.CrashTestDummies;

internal static class Settings
{
    public static ConfigEntry<bool> ModEnabled = null!;
    public static ConfigEntry<int> MaxTotalEnemies = null!;
    public static ConfigEntry<float> OutOfSightHalfAngleDegrees = null!;
    public static ConfigEntry<int> SpawnCount = null!;
    public static ConfigEntry<float> SkipFirstMeters = null!;
    public static ConfigEntry<float> MaxDistanceMeters = null!;
    public static ConfigEntry<float> MinSpacingMeters = null!;
    public static ConfigEntry<string> EnemyNameFilter = null!;
    public static ConfigEntry<float> SpawnDelaySeconds = null!;
    public static ConfigEntry<bool> SpawnAsSleepers = null!;
    public static ConfigEntry<bool> AmbushOnAlarmDoors = null!;
    public static ConfigEntry<int> AmbushCount = null!;
    public static ConfigEntry<float> AmbushSkipFirstMeters = null!;
    public static ConfigEntry<float> AmbushMaxDistanceMeters = null!;
    public static ConfigEntry<bool> AmbientSpawnEnabled = null!;
    public static ConfigEntry<float> AmbientIntervalSeconds = null!;
    public static ConfigEntry<int> AmbientCountMin = null!;
    public static ConfigEntry<int> AmbientCountMax = null!;
    public static ConfigEntry<float> AmbientSkipFirstMeters = null!;
    public static ConfigEntry<float> AmbientMaxDistanceMeters = null!;
    public static ConfigEntry<bool> CleanupEnabled = null!;
    public static ConfigEntry<float> CleanupCheckIntervalSeconds = null!;
    public static ConfigEntry<float> CleanupAfterSeconds = null!;

    public static void Bind(ConfigFile config)
    {
        ModEnabled = config.Bind(
            "General", "Enabled", true,
            "Master switch. Disable to stop dummies from spawning without uninstalling the mod.");

        MaxTotalEnemies = config.Bind(
            "General", "MaxTotalEnemies", 300,
            "Population cap: how many of this mod's dummies may be alive at once, combined across the " +
            "drop-point scatter, alarm-door ambushes and the ambient scatter. It's a ceiling on the " +
            "current count, not a lifetime total - as dummies die or get recycled by the Cleanup settings, " +
            "room opens up for more to spawn.");

        OutOfSightHalfAngleDegrees = config.Bind(
            "General", "OutOfSightHalfAngleDegrees", 70f,
            "Used by every spawn trigger and by cleanup: a spot is treated as hidden from a player if it " +
            "falls outside this many degrees of the centre of their view, or something blocks the line of " +
            "sight to it. The bigger this is, the more conservative (and more likely to find nowhere to " +
            "spawn in a small room). Only the host's own view has a direction; other players and bots " +
            "count as looking every way, so only a blocked line of sight hides a spot from them.");

        SpawnCount = config.Bind(
            "Spawning", "SpawnCount", 100,
            "How many enemies to scatter between SkipFirstMeters and MaxDistanceMeters, at the level's own enemy spawn " +
            "spots and random walkable points in each room.");

        SkipFirstMeters = config.Bind(
            "Spawning", "SkipFirstMeters", 10f,
            "No dummy is placed closer than this to where you step off the elevator.");

        MaxDistanceMeters = config.Bind(
            "Spawning", "MaxDistanceMeters", 100f,
            "No dummy is placed farther than this from where you step off the elevator.");

        MinSpacingMeters = config.Bind(
            "Spawning", "MinSpacingMeters", 3f,
            "Minimum distance between any two enemies, just enough to keep them from overlapping.");

        EnemyNameFilter = config.Bind(
            "Spawning", "EnemyNameFilter", "Striker_Hibernate",
            "Case-insensitive substring match against the game's enemy data block names; of the matches, the one with the " +
            "lowest max health is spawned. Default is the plain sleeping Striker. " +
            "If nothing matches, the mod falls back to whichever enemy has the lowest max health.");

        SpawnDelaySeconds = config.Bind(
            "Spawning", "SpawnDelaySeconds", 2f,
            "Delay after you spawn into the level before dummies are created, to let the level finish settling.");

        SpawnAsSleepers = config.Bind(
            "Spawning", "SpawnAsSleepers", true,
            "true: dummies spawn asleep (hibernating) and can be stealth-killed. false: dummies spawn awake and aggressive.");

        AmbushOnAlarmDoors = config.Bind(
            "Ambush", "AmbushOnAlarmDoors", true,
            "When a security door locked behind an alarm scan (chained puzzle) finishes opening, scatter extra " +
            "dummies on the far side of it, same as the level-start scatter but centered on that doorway.");

        AmbushCount = config.Bind(
            "Ambush", "AmbushCount", 20,
            "How many enemies to scatter on the far side of an alarm door when it opens.");

        AmbushSkipFirstMeters = config.Bind(
            "Ambush", "AmbushSkipFirstMeters", 10f,
            "No ambush dummy is placed closer than this to the far side of the doorway.");

        AmbushMaxDistanceMeters = config.Bind(
            "Ambush", "AmbushMaxDistanceMeters", 100f,
            "No ambush dummy is placed farther than this from the far side of the doorway.");

        AmbientSpawnEnabled = config.Bind(
            "Ambient", "AmbientSpawnEnabled", true,
            "Every AmbientIntervalSeconds, scatter a few more dummies near the local player, skipping any " +
            "spot currently in a player's view so they never visibly pop in.");

        AmbientIntervalSeconds = config.Bind(
            "Ambient", "AmbientIntervalSeconds", 60f,
            "How often, in seconds, to run the ambient scatter (at least 1).");

        AmbientCountMin = config.Bind(
            "Ambient", "AmbientCountMin", 50,
            "Minimum enemies to scatter each time the ambient scatter runs (inclusive).");

        AmbientCountMax = config.Bind(
            "Ambient", "AmbientCountMax", 100,
            "Maximum enemies to scatter each time the ambient scatter runs (inclusive). A random count " +
            "between AmbientCountMin and this is picked on every run.");

        AmbientSkipFirstMeters = config.Bind(
            "Ambient", "AmbientSkipFirstMeters", 10f,
            "No ambient dummy is placed closer than this to the local player.");

        AmbientMaxDistanceMeters = config.Bind(
            "Ambient", "AmbientMaxDistanceMeters", 100f,
            "No ambient dummy is placed farther than this from the local player.");

        CleanupEnabled = config.Bind(
            "Cleanup", "CleanupEnabled", true,
            "Recycle dummies that have sat alive, untouched and out of sight for a while, so the " +
            "population cap keeps refilling with fresh dummies instead of staying maxed out with old ones.");

        CleanupCheckIntervalSeconds = config.Bind(
            "Cleanup", "CleanupCheckIntervalSeconds", 30f,
            "How often, in seconds, to check for dummies to recycle (at least 1).");

        CleanupAfterSeconds = config.Bind(
            "Cleanup", "CleanupAfterSeconds", 300f,
            "A dummy is only eligible to be recycled once it has been alive this long (seconds) and is " +
            "currently out of sight - it's never removed while a player might be looking at it.");
    }
}
