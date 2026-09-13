using System.Linq;
using SOSGame.Logic;
using SOSGame.Logic.Players;
using Xunit;

namespace SOSGame.Tests
{
    public class ComputerPlayerTests
    {
        [Fact]
        public void ChoosesWinningMove_WhenAvailable()
        {
            var game = new SimpleGame(3);
            game.PlaceLetter(0, 0, 'S');
            game.PlaceLetter(0, 1, 'O');
            var cpu = new ComputerPlayer('S');

            var move = cpu.GetNextMove(game);
            Assert.Equal((0, 2), move);

            game.MakeMove(move.Row, move.Col, cpu.Letter);
            Assert.Equal(1, game.GetPlayerOneScore() + game.GetPlayerTwoScore());
        }

        [Fact]
        public void BlocksOpponent_WhenTheyCouldWinNext()
        {
            var game = new SimpleGame(3);
            // Opponent 'O' would form SOS at (1,1) between the two 'S's
            game.PlaceLetter(1, 0, 'S');
            game.PlaceLetter(1, 2, 'S');
            var cpu = new ComputerPlayer('S');

            // CPU chooses the block position
            var move = cpu.GetNextMove(game);
            Assert.Equal((1, 1), move);

            // Apply the CPU's move to the board
            game.MakeMove(move.Row, move.Col, cpu.Letter);

            // Now that the cell is occupied by 'S', the opponent cannot score there
            Assert.False(game.WouldScore('O', 1, 1));
        }

        [Fact]
        public void PicksRandom_WhenNoWinningOrBlocking()
        {
            var game = new SimpleGame(3);
            var cpu = new ComputerPlayer('S');

            var move = cpu.GetNextMove(game);
            var empties = game.GetEmptyCells();
            Assert.Contains(move, empties);
        }
    }
}
