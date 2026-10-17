using System.Numerics;

namespace GTFO.CrashTestDummies.Core;

public static class SpawnPicker
{
    /// <summary>
    /// Greedily picks up to <paramref name="count"/> of <paramref name="candidates"/> (already in walking order):
    /// each must be between <paramref name="skipMeters"/> and <paramref name="maxMeters"/> from
    /// <paramref name="origin"/> and at least <paramref name="minSpacing"/> from every earlier pick.
    /// Returns the indices of the picks.
    /// </summary>
    public static List<int> Pick(
        IReadOnlyList<Vector3> candidates,
        Vector3 origin,
        float skipMeters,
        float maxMeters,
        float minSpacing,
        int count)
    {
        var picked = new List<int>();
        var placed = new List<Vector3>();
        var spacingSq = minSpacing * minSpacing;

        for (var i = 0; i < candidates.Count && picked.Count < count; i++)
        {
            var c = candidates[i];
            var fromOrigin = Vector3.Distance(c, origin);
            if (fromOrigin < skipMeters || fromOrigin > maxMeters)
                continue;

            if (placed.Any(p => Vector3.DistanceSquared(c, p) < spacingSq))
                continue;

            picked.Add(i);
            placed.Add(c);
        }

        return picked;
    }
}
