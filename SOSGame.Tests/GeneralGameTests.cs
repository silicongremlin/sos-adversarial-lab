using Xunit;
using System;
using System.Collections.Generic;
using SOSGame.Logic;

namespace SOSGame.Tests
{
    public class GeneralGameTests
    {
        // Test for forming SOS in different orientations.
        [Theory]
        [InlineData(0, 2, 1, 2, 2, 2)]  // vertical
        [InlineData(2, 0, 2, 1, 2, 2)]  // horizontal
        [InlineData(0, 0, 1, 1, 2, 2)]  // diagonal
        public void MakeMove_FormsSOS_IncrementsScoreAndKeepsTurn(
            int r1, int c1, int r2, int c2, int r3, int c3)
        {
            int size = 5;
            int initialScore = 0;
            var messages = new List<string>();
            var game = new GeneralGame(size, msg => messages.Add(msg));

            // Place S, O
            game.MakeMove(r1, c1, 'S');
            game.MakeMove(r2, c2, 'O');
            // This move should form exactly one SOS
            bool result = game.MakeMove(r3, c3, 'S');

            Assert.True(result);
            Assert.Equal(initialScore + 1, game.GetPlayerOneScore() + game.GetPlayerTwoScore());
            // On SOS, the same player keeps the turn
            Assert.True(game.IsPlayerOneTurn());
        }

        // Test that a non‑SOS move switches turn
        [Fact]
        public void MakeMove_NoSOS_SwitchesTurn()
        {
            var game = new GeneralGame(3);
            bool initialTurn = game.IsPlayerOneTurn();
            // Place a move that cannot form SOS
            bool result = game.MakeMove(0, 0, 'S');

            Assert.True(result);
            Assert.NotEqual(initialTurn, game.IsPlayerOneTurn());
        }

        // Test for invalid inputs (invalid letter or out of bounds)
        [Theory]
        [InlineData(0, 0, 'X')]   // invalid letter
        [InlineData(-1, 0, 'S')]  // row < 0
        [InlineData(0, 3, 'O')]   // col >= size
        public void MakeMove_InvalidInput_ReturnsFalse(int row, int col, char letter)
        {
            var game = new GeneralGame(3);
            bool result = game.MakeMove(row, col, letter);
            Assert.False(result);
        }

        // Test for attempting a move on an already occupied cell.
        [Fact]
        public void MakeMove_ToOccupiedCell_ReturnsFalse()
        {
            var game = new GeneralGame(3);
            game.MakeMove(1, 1, 'S');
            bool result = game.MakeMove(1, 1, 'O');
            Assert.False(result);
        }

        // Test that the win banner is invoked when the board fills and scores determine a winner or tie
        [Fact]
        public void MakeMove_GameOver_InvokesBanner()
        {
            var size = 2; // 2x2 will fill in 4 moves
            var messages = new List<string>();
            var game = new GeneralGame(size, msg => messages.Add(msg));

            // Fill up without forming SOS (alternating S/O)
            game.MakeMove(0, 0, 'S');
            game.MakeMove(0, 1, 'O');
            game.MakeMove(1, 0, 'S');
            bool lastMove = game.MakeMove(1, 1, 'O');

            Assert.True(lastMove);
            Assert.True(game.IsGameOver());
            Assert.Single(messages);
            Assert.Contains("wins! Game Over!", messages[0]);
        }
    }
}
