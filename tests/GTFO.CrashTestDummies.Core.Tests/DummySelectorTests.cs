using GTFO.CrashTestDummies.Core;
using NUnit.Framework;

namespace GTFO.CrashTestDummies.Core.Tests;

public class DummySelectorTests
{
    private static EnemyCandidate Enemy(uint id, string name, float healthMax, bool enabled = true, bool hasPrefab = true)
        => new(id, name, enabled, hasPrefab, healthMax);

    private static readonly EnemyCandidate Striker = Enemy(13, "Striker_Wave", 20f);
    private static readonly EnemyCandidate BigStriker = Enemy(16, "Striker_Big_Wave", 120f);
    private static readonly EnemyCandidate Shooter = Enemy(11, "Shooter_Wave", 30f);
    private static readonly EnemyCandidate Baby = Enemy(38, "Baby", 5f);
    private static readonly EnemyCandidate Tank = Enemy(29, "Tank", 1000f);

    [Test]
    public void PicksWeakestNameMatch_SoBigStrikersAreSkipped()
    {
        var result = DummySelector.Select(new[] { BigStriker, Striker, Shooter }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
        Assert.That(result.FilterUnmatched, Is.False);
    }

    [Test]
    public void FilterMatchesAnywhereInName_IgnoringCase()
    {
        var result = DummySelector.Select(new[] { Shooter, BigStriker, Striker }, "sTrIkEr");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [Test]
    public void FilterWins_EvenOverAWeakerUnmatchedEnemy()
    {
        var result = DummySelector.Select(new[] { Baby, Striker }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [Test]
    public void NoNameMatch_FallsBackToWeakestOverall_AndSaysSo()
    {
        var result = DummySelector.Select(new[] { Tank, Shooter, Baby }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Baby));
        Assert.That(result.FilterUnmatched, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void NoFilter_PicksWeakestOverall_WithoutReportingAMiss(string? filter)
    {
        var result = DummySelector.Select(new[] { Tank, Striker, Baby }, filter);

        Assert.That(result.Chosen, Is.EqualTo(Baby));
        Assert.That(result.FilterUnmatched, Is.False);
    }

    [Test]
    public void SkipsDisabledBlocks()
    {
        var disabledStriker = Enemy(99, "Striker_Test", 1f, enabled: false);

        var result = DummySelector.Select(new[] { disabledStriker, Striker }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [Test]
    public void SkipsBlocksWithoutAPrefab()
    {
        var noPrefabStriker = Enemy(99, "Striker_Test", 1f, hasPrefab: false);

        var result = DummySelector.Select(new[] { noPrefabStriker, Striker }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [TestCase(0f)]
    [TestCase(-10f)]
    public void SkipsBlocksWithNoHealth(float healthMax)
    {
        var broken = Enemy(99, "Striker_Broken", healthMax);

        var result = DummySelector.Select(new[] { broken, Striker }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [Test]
    public void OnlyUnspawnableMatches_CountAsNoMatch()
    {
        var disabledStriker = Enemy(99, "Striker_Test", 1f, enabled: false);

        var result = DummySelector.Select(new[] { disabledStriker, Shooter }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Shooter));
        Assert.That(result.FilterUnmatched, Is.True);
    }

    [Test]
    public void NullNames_AreNeverNameMatches()
    {
        var unnamed = new EnemyCandidate(99, null, true, true, 1f);

        var result = DummySelector.Select(new[] { unnamed, Striker }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(Striker));
    }

    [Test]
    public void TiesGoToTheFirstCandidate()
    {
        var strikerA = Enemy(1, "Striker_A", 20f);
        var strikerB = Enemy(2, "Striker_B", 20f);

        var result = DummySelector.Select(new[] { strikerA, strikerB }, "Striker");

        Assert.That(result.Chosen, Is.EqualTo(strikerA));
    }

    [Test]
    public void NothingSpawnable_ChoosesNothing()
    {
        var result = DummySelector.Select(new[] { Enemy(1, "Striker", 20f, enabled: false) }, "Striker");

        Assert.That(result.Chosen, Is.Null);
    }

    [Test]
    public void StrikerSleeperFilter_PicksThePlainStrikerSleeper()
    {
        var strikerSleeper = Enemy(20, "Striker_Hibernate", 20f);
        var bigSleeper = Enemy(21, "Striker_Big_Hibernate", 120f);

        var result = DummySelector.Select(new[] { bigSleeper, Striker, strikerSleeper }, "Striker_Hibernate");

        Assert.That(result.Chosen, Is.EqualTo(strikerSleeper));
    }

    [Test]
    public void NoCandidates_ChoosesNothing()
    {
        var result = DummySelector.Select(Array.Empty<EnemyCandidate>(), "Striker");

        Assert.That(result.Chosen, Is.Null);
    }
}
