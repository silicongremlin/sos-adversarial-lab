using System;

namespace SOSGame.Logic
{
    public class SimpleGame : BaseGame
    {
        private readonly Action<string>? displayBanner;

        public SimpleGame(int size, Action<string>? displayBannerCallback = null)
            : base(size)
        {
            this.displayBanner = displayBannerCallback;

            this.IsFrozen = false;
        }

        public override bool MakeMove(int row, int col, char letter)
        {
            // If the game is frozen (an SOS has been formed), do not allow further moves.
            if (this.IsFrozen)
                return false;

            if (!IsValidMove(row, col, letter))
                return false;

            PlaceLetter(row, col, letter);

            // Stop after the first SOS.
            int newSOS = CountNewSOS(row, col, stopAfterFirst: true);
            string moveMessage = "";

            if (newSOS > 0)
            {
                // Update the player's score.
                if (isPlayerOneTurn)
                    playerOneScore += newSOS;
                else
                    playerTwoScore += newSOS;


                string winner = (playerOneScore > playerTwoScore) ? "Nice job player Red" :
                                (playerTwoScore > playerOneScore) ? "Sweet Action player Blue" : "Tie";
                string gameOverMessage = $" {winner} wins! Game Over!";
                string finalMessage = moveMessage.Length > 0 ? $"{moveMessage} {gameOverMessage}" : gameOverMessage;
                displayBanner?.Invoke(finalMessage);


                // Freeze the game so no further moves are accepted.
                this.IsFrozen = true;
            }

            if (IsGameOver())
            {
                string endMessage = "This round ended as a draw! Want to try again?";
                System.Diagnostics.Debug.WriteLine(endMessage);
                displayBanner?.Invoke(endMessage);
            }
            else
            {
                // Only switch turn if the game is not frozen.
                if (!this.IsFrozen)
                    SwitchTurn();
            }

            return true;
        }
    }
}
