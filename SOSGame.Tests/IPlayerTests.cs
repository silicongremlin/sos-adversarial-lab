using Xunit;
using SOSGame.Logic.Players;

namespace SOSGame.Tests
{
    public class IPlayerTests
    {
        // Combines HumanPlayer and ComputerPlayer interface compliance tests
        [Theory]
        [InlineData(false, 'S')]  // HumanPlayer with 'S'
        [InlineData(false, 'O')]  // HumanPlayer with 'O'
        [InlineData(true, 'S')]  // ComputerPlayer with 'S'
        [InlineData(true, 'O')]  // ComputerPlayer with 'O'
        public void Player_ImplementsInterface_AndStoresLetter(bool isComputer, char letter)
        {
            IGamePlayer player = isComputer
                ? new ComputerPlayer(letter)
                : new HumanPlayer(letter);

            Assert.IsAssignableFrom<IGamePlayer>(player);
            Assert.Equal(letter, player.DefaultLetter);
        }
    }
}
