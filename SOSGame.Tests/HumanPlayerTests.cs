using System;
using SOSGame.Logic;
using SOSGame.Logic.Players;
using Xunit;

namespace SOSGame.Tests
{
    public class HumanPlayerTests
    {
        // Valid letters should be accepted and stored
        [Theory]
        [InlineData('S')]
        [InlineData('O')]
        public void Constructor_ValidLetters_SetsDefaultLetter(char letter)
        {
            var player = new HumanPlayer(letter);
            Assert.Equal(letter, player.DefaultLetter);
        }

        // Invalid letters should throw ArgumentException
        [Theory]
        [InlineData('X')]
        [InlineData('A')]
        [InlineData('1')]
        [InlineData('\0')]
        public void Constructor_InvalidLetter_ThrowsArgumentException(char letter)
        {
            var ex = Assert.Throws<ArgumentException>(() => new HumanPlayer(letter));
            Assert.Contains("Letter must be 'S' or 'O'", ex.Message);
        }

        // GetNextMove should always throw InvalidOperationException
        [Fact]
        public void GetNextMove_Always_ThrowsInvalidOperationException()
        {
            var player = new HumanPlayer('S');
            var dummyGame = new SimpleGame(3);
            Assert.Throws<InvalidOperationException>(() => player.GetNextMove(dummyGame));
        }

        // GetNextMoveWithLetter should always throw InvalidOperationException
        [Fact]
        public void GetNextMoveWithLetter_Always_ThrowsInvalidOperationException()
        {
            var player = new HumanPlayer('O');
            var dummyGame = new SimpleGame(3);
            Assert.Throws<InvalidOperationException>(() => player.GetNextMoveWithLetter(dummyGame));
        }
    }
}
