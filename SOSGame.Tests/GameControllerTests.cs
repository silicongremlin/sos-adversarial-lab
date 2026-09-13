using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOSGame.Controller;
using SOSGame.Logic;
using SOSGame.Logic.Players;
using Xunit;


namespace SOSGame.Tests
{
    public class GameControllerTests
    {
        [Fact]
        public void Constructor_InvalidGridSize_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new GameController(2, "Simple"));
        }

        [Fact]
        public void TestTurnSwitching()
        {
            var controller = new GameController(5, "Simple");
            char first = controller.CurrentPlayer;
            bool ok = controller.MakeMove(0, 0, 'S');
            Assert.True(ok);
            Assert.NotEqual(first, controller.CurrentPlayer);
        }

        [Fact]
        public void OnMoveMade_Fires_For_Human_And_AI_Moves()
        {
            var controller = new GameController(
                3, "Simple",
                redIsCpu: false,
                blueIsCpu: true,
                redLetter: 'S',
                blueLetter: 'O');

            var moves = new List<(int r, int c, char l)>();
            controller.OnMoveMade += (r, c, l) => moves.Add((r, c, l));

            controller.MakeMove(0, 0, 'S');

            Assert.True(moves.Count >= 2);
            Assert.Equal((0, 0, 'S'), moves[0]);

            foreach (var mv in moves)
                Assert.NotEqual('\0', controller.GameLogic.GetBoard()[mv.r, mv.c]);
        }

        [Fact]
        public async Task TryAutoPlayNext_Completes_Full_CPUvsCPU_Game()
        {
            var controller = new GameController(
                3, "Simple",
                redIsCpu: true,
                blueIsCpu: true,
                redLetter: 'S',
                blueLetter: 'O');

            int movesMade = 0;
            controller.OnMoveMade += (_, _, _) => movesMade++;

            // Run the CPU vs CPU sequence
            await controller.TryAutoPlayNext();

            // Ensure at least one move was made
            Assert.True(movesMade > 0);

            // In SimpleGame mode, the game should freeze on first SOS
            var simpleGame = controller.GameLogic as SimpleGame;
            Assert.NotNull(simpleGame);
            Assert.True(simpleGame.IsFrozen);
        }
    }
}