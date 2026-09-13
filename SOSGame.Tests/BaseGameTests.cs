using Xunit;
using SOSGame.Logic;

namespace SOSGame.Tests
{
    public class BaseGameTests
    {
        // Returns a new instance of SimpleGame (as a BaseGame) with the specified size.
        private BaseGame GetGameInstance(int size = 5) => new SimpleGame(size);

        [Fact]
        // Verifies that a new game instance is created successfully.
        public void Game_Initializes_Correctly()
        {
            var game = GetGameInstance();
            Assert.NotNull(game);
        }

        [Fact]
        // Verifies that making a valid move updates the game board correctly.
        public void MakeMove_ValidMove_UpdatesGrid()
        {
            var game = GetGameInstance();
            bool result = game.MakeMove(2, 2, 'S');
            Assert.True(result);
            Assert.Equal('S', game.GetBoard()[2, 2]);
        }

        [Fact]
        // Verifies that attempting to make a move on an already occupied cell returns false.
        public void MakeMove_ToOccupiedCell_ReturnsFalse()
        {
            var game = GetGameInstance();
            game.MakeMove(0, 0, 'S');
            bool result = game.MakeMove(0, 0, 'O');
            Assert.False(result);
        }

        [Fact]
        // Verifies that the turn switches after a valid move is made.
        public void Turn_Switches_AfterMove()
        {
            var game = GetGameInstance();
            bool initialTurn = game.IsPlayerOneTurn();
            game.MakeMove(0, 0, 'S');
            Assert.NotEqual(initialTurn, game.IsPlayerOneTurn());
        }

        [Fact]
        // Verifies that calling SwitchTurn toggles the current player.
        public void SwitchTurn_TogglesTurn()
        {
            var game = GetGameInstance();
            bool before = game.IsPlayerOneTurn();
            game.SwitchTurn();
            bool after = game.IsPlayerOneTurn();
            Assert.NotEqual(before, after);
        }

        [Fact]
        // Verifies that GetBoard returns the current state of the game board.
        public void GetBoard_ReturnsBoardState()
        {
            var game = GetGameInstance();
            game.MakeMove(1, 1, 'S');
            var board = game.GetBoard();
            Assert.Equal('S', board[1, 1]);
        }

        [Fact]
        // Verifies that both players' scores are initially zero.
        public void PlayerScores_InitiallyZero()
        {
            var game = GetGameInstance();
            Assert.Equal(0, game.GetPlayerOneScore());
            Assert.Equal(0, game.GetPlayerTwoScore());
        }

        [Fact]
        public void GetEmptyCells_ReturnsAllBlanks()
        {
            var game = new SimpleGame(3);
            game.MakeMove(1, 1, 'S');
            var empties = game.GetEmptyCells();
            Assert.Equal(8, empties.Count);
            Assert.DoesNotContain((1, 1), empties);
        }

        [Fact]
        public void WouldScore_DetectsSOSCorrectly()
        {
            var game = new SimpleGame(3);
            // Setup so placing 'S' at (0,2) completes SOS
            game.PlaceLetter(0, 0, 'S');
            game.PlaceLetter(0, 1, 'O');
            Assert.True(game.WouldScore('S', 0, 2));
            Assert.False(game.WouldScore('O', 2, 2));
        }

    }
}
