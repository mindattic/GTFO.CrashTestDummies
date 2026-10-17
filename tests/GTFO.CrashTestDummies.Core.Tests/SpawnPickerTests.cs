using System.Numerics;
using GTFO.CrashTestDummies.Core;
using NUnit.Framework;

namespace GTFO.CrashTestDummies.Core.Tests;

public class SpawnPickerTests
{
    private static Vector3 At(float x) => new(x, 0f, 0f);

    [Test]
    public void SkipsPointsInsideTheDropZone()
    {
        var picks = SpawnPicker.Pick(new[] { At(2), At(9.9f), At(10), At(50) }, Vector3.Zero, 10f, 100f, 0f, 10);

        Assert.That(picks, Is.EqualTo(new[] { 2, 3 }));
    }

    [Test]
    public void SkipsPointsPastTheMaxDistance()
    {
        var picks = SpawnPicker.Pick(new[] { At(20), At(100), At(100.1f) }, Vector3.Zero, 10f, 100f, 0f, 10);

        Assert.That(picks, Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void KeepsMinSpacingFromEarlierPicks()
    {
        var picks = SpawnPicker.Pick(new[] { At(20), At(22), At(23), At(30) }, Vector3.Zero, 10f, 100f, 3f, 10);

        Assert.That(picks, Is.EqualTo(new[] { 0, 2, 3 }));
    }

    [Test]
    public void StopsAtCount()
    {
        var picks = SpawnPicker.Pick(new[] { At(20), At(30), At(40) }, Vector3.Zero, 10f, 100f, 3f, 2);

        Assert.That(picks, Is.EqualTo(new[] { 0, 1 }));
    }

    [Test]
    public void DistanceIncludesHeight()
    {
        var picks = SpawnPicker.Pick(new[] { new Vector3(5f, 9f, 0f) }, Vector3.Zero, 10f, 100f, 0f, 10);

        Assert.That(picks, Is.EqualTo(new[] { 0 }));
    }
}
