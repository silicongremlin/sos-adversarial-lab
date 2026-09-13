using System.Collections.Generic;

namespace SOSGame.Logic;

/// <summary>
/// Safe simulation of the original vulnerable state transition. The trigger
/// and observable attack-chain stages are retained as trace events; every
/// execution primitive is inert and performs no decryption, I/O, or process launch.
/// </summary>
public sealed class VulnerableGame : BaseGame
{
    public IList<string> ObservableTrace { get; } = new List<string>();

    public VulnerableGame(int gridSize) : base(3)
    {
        if (gridSize == 13)
        {
            ObservableTrace.Add("trigger:gridSize==13");
            ObservableTrace.Add("load-embedded-resource:payload (mock)");
            ObservableTrace.Add("load-embedded-resource:key (mock)");
            ObservableTrace.Add("decrypt-key (mock)");
            ObservableTrace.Add("decrypt-payload (mock)");
            ObservableTrace.Add("write-temp-hta (mock)");
            ObservableTrace.Add("execute-mshta (mock)");
            ObservableTrace.Add("cleanup-temp-file (mock)");
        }
    }

    public override bool MakeMove(int row, int col, char letter) => false;
}
