using Xunit;
using SOSGame.Logic;

namespace SOSGame.Tests;

public class VulnerableGameTests
{
    [Fact]
    public void Trigger_RecordsCompleteInertAttackChain()
    {
        var game = new VulnerableGame(13);
        Assert.Equal(new[]
        {
            "trigger:gridSize==13",
            "load-embedded-resource:payload (mock)",
            "load-embedded-resource:key (mock)",
            "decrypt-key (mock)",
            "decrypt-payload (mock)",
            "write-temp-hta (mock)",
            "execute-mshta (mock)",
            "cleanup-temp-file (mock)"
        }, game.ObservableTrace);
    }

    [Fact]
    public void NonTrigger_DoesNotRecordChain()
    {
        var game = new VulnerableGame(12);
        Assert.Empty(game.ObservableTrace);
    }

    [Fact]
    public void InertState_RejectsMovesAndDoesNotChangeBoard()
    {
        var game = new VulnerableGame(13);
        Assert.False(game.MakeMove(0, 0, 'S'));
        Assert.Equal(0, game.GetTotalMoves());
    }
}
