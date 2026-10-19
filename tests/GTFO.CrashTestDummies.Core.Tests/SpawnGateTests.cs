using GTFO.CrashTestDummies.Core;
using NUnit.Framework;

namespace GTFO.CrashTestDummies.Core.Tests;

public class SpawnGateTests
{
    [Test]
    public void OpensOncePerLevel()
    {
        var gate = new SpawnGate();

        Assert.That(gate.TryBegin(isHost: true), Is.True);
        Assert.That(gate.TryBegin(isHost: true), Is.False);
    }

    [Test]
    public void ReopensAfterANewLevelStarts()
    {
        var gate = new SpawnGate();
        gate.TryBegin(isHost: true);

        gate.Reset();

        Assert.That(gate.TryBegin(isHost: true), Is.True);
    }

    [Test]
    public void StaysShutForClients()
    {
        Assert.That(new SpawnGate().TryBegin(isHost: false), Is.False);
    }

    [Test]
    public void ARefusedAttempt_DoesNotUseUpTheLevelsSpawn()
    {
        var gate = new SpawnGate();

        gate.TryBegin(isHost: false);

        Assert.That(gate.TryBegin(isHost: true), Is.True);
    }

    [Test]
    public void ResetBeforeAnySpawn_IsHarmless()
    {
        var gate = new SpawnGate();

        gate.Reset();

        Assert.That(gate.TryBegin(isHost: true), Is.True);
    }
}
