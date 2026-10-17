namespace GTFO.CrashTestDummies.Core;

/// <summary>The game-independent facts about one enemy data block that dummy selection needs.</summary>
public sealed record EnemyCandidate(uint Id, string? Name, bool Enabled, bool HasPrefab, float HealthMax)
{
    /// <summary>Disabled blocks, blocks without a built prefab, and zero-health blocks can't be spawned as dummies.</summary>
    public bool IsSpawnable => Enabled && HasPrefab && HealthMax > 0f;
}

/// <param name="Chosen">The enemy to spawn, or null if nothing is spawnable.</param>
/// <param name="FilterUnmatched">A filter was given but no spawnable enemy matched it, so the weakest overall was used.</param>
public sealed record DummySelection(EnemyCandidate? Chosen, bool FilterUnmatched);

public static class DummySelector
{
    /// <summary>
    /// Of the spawnable enemies whose name contains <paramref name="nameFilter"/> (case-insensitive),
    /// picks the one with the lowest max health, so heavier variants sharing the name (e.g. Big Strikers)
    /// are skipped. With no filter, or no match, picks the weakest spawnable enemy overall.
    /// Ties go to whichever came first.
    /// </summary>
    public static DummySelection Select(IEnumerable<EnemyCandidate> candidates, string? nameFilter)
    {
        var spawnable = candidates.Where(c => c.IsSpawnable).ToList();

        var filterUnmatched = false;
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var named = spawnable
                .Where(c => c.Name != null && c.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (named.Count > 0)
                spawnable = named;
            else
                filterUnmatched = true;
        }

        EnemyCandidate? weakest = null;
        foreach (var candidate in spawnable)
        {
            if (weakest == null || candidate.HealthMax < weakest.HealthMax)
                weakest = candidate;
        }

        return new DummySelection(weakest, filterUnmatched);
    }

    /// <summary>
    /// Every spawnable enemy whose name contains <paramref name="nameFilter"/> (case-insensitive) and whose max
    /// health is at most <paramref name="maxHealth"/>. If none qualify, falls back to <see cref="Select"/>'s single pick.
    /// </summary>
    public static (List<EnemyCandidate> Chosen, bool FilterUnmatched) SelectAll(
        IEnumerable<EnemyCandidate> candidates, string? nameFilter, float maxHealth)
    {
        var list = candidates.ToList();
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var matches = list
                .Where(c => c.IsSpawnable && c.HealthMax <= maxHealth
                            && c.Name != null && c.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count > 0)
                return (matches, false);
        }

        var single = Select(list, null).Chosen;
        return (single != null ? new List<EnemyCandidate> { single } : new List<EnemyCandidate>(),
            !string.IsNullOrWhiteSpace(nameFilter));
    }
}
