namespace GTFO.CrashTestDummies.Core;

/// <summary>Lets the dummies spawn at most once per generated level, and only on the host.</summary>
public sealed class SpawnGate
{
    private bool _consumed;

    /// <summary>Call when a new level starts generating.</summary>
    public void Reset() => _consumed = false;

    /// <summary>
    /// True exactly once per level when the mod is enabled and this client is the host. A call that's
    /// refused because the mod is off or this isn't the host doesn't use up the level's spawn.
    /// </summary>
    public bool TryBegin(bool modEnabled, bool isHost)
    {
        if (!modEnabled || !isHost || _consumed)
            return false;

        _consumed = true;
        return true;
    }
}
