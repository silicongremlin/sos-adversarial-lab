using Xunit;
using SOSGame.Logic;

namespace SOSGame.Tests
{
    public class SimpleGameTests
    {
        // Parameterized test for forming SOS in vertical, horizontal, and diagonal.
        [Theory]
        [InlineData(0, 2, 1, 2, 2, 2)]  // vertical
        [InlineData(2, 0, 2, 1, 2, 2)]  // horizontal
        [InlineData(0, 0, 1, 1, 2, 2)]  // diagonal
        public void MakeMove_FormsSOS_ReturnsTrue(
            int r1, int c1, int r2, int c2, int r3, int c3)
        {
            var game = new SimpleGame(5);
            game.MakeMove(r1, c1, 'S');
            game.MakeMove(r2, c2, 'O');
            bool result = game.MakeMove(r3, c3, 'S');
            Assert.True(result);
        }

        // Parameterized test for invalid inputs (invalid letter or out of bounds).
        [Theory]
        [InlineData(0, 0, 'X')]   // invalid letter
        [InlineData(-1, 0, 'S')]  // out of bounds (row < 0)
        [InlineData(0, 5, 'O')]   // out of bounds (col >= size)
        public void MakeMove_InvalidInput_ReturnsFalse(int row, int col, char letter)
        {
            var game = new SimpleGame(5);
            bool result = game.MakeMove(row, col, letter);
            Assert.False(result);
        }

        // Test for attempting a move on an already occupied cell.
        [Fact]
        public void MakeMove_ToOccupiedCell_ReturnsFalse()
        {
            var game = new SimpleGame(5);
            game.MakeMove(0, 0, 'S');
            bool result = game.MakeMove(0, 0, 'O');
            Assert.False(result);
        }
    }
}
