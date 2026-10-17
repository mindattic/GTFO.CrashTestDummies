using BepInEx.Configuration;

namespace GTFO.CrashTestDummies;

internal static class Settings
{
    public static ConfigEntry<bool> ModEnabled = null!;
    public static ConfigEntry<int> SpawnCount = null!;
    public static ConfigEntry<float> SkipFirstMeters = null!;
    public static ConfigEntry<float> MaxDistanceMeters = null!;
    public static ConfigEntry<float> MinSpacingMeters = null!;
    public static ConfigEntry<string> EnemyNameFilter = null!;
    public static ConfigEntry<float> SpawnDelaySeconds = null!;
    public static ConfigEntry<bool> SpawnAsSleepers = null!;

    public static void Bind(ConfigFile config)
    {
        ModEnabled = config.Bind(
            "General", "Enabled", true,
            "Master switch. Disable to stop dummies from spawning without uninstalling the mod.");

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
    }
}
