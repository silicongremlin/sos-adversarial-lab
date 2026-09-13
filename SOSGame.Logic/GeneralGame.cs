using System;

namespace SOSGame.Logic
{
    public class GeneralGame : BaseGame
    {
        // Callback for displaying banner messages.
        private readonly Action<string>? displayBanner;

        public GeneralGame(int size, Action<string>? displayBannerCallback = null)
            : base(size)
        {
            displayBanner = displayBannerCallback;
        }

        public override bool MakeMove(int row, int col, char letter)
        {
            if (!IsValidMove(row, col, letter))
                return false;

            PlaceLetter(row, col, letter);

            int newSOS = CountNewSOS(row, col);
            if (newSOS > 0)
            {
                if (isPlayerOneTurn)
                    playerOneScore += newSOS;
                else
                    playerTwoScore += newSOS;
                // current player keeps turn on SOS
            }
            else
            {
                SwitchTurn();
            }

            if (IsGameOver())
            {
                string winner = (playerOneScore > playerTwoScore)
                    ? "Nice Job Player Red"
                    : (playerTwoScore > playerOneScore)
                        ? "Sweet Action Player Blue"
                        : "Oh No! There was a tie and no one";
                displayBanner?.Invoke($" {winner} wins! Game Over!");
            }

            return true;
        }

        private int CountNewSOS(int row, int col)
        {
            int totalSOS = 0;

            int[,] directions = new int[,]
            {
                { 0, 1 },   // right
                { 1, 0 },   // down
                { 1, 1 },   // diagonal down-right
                { 1, -1 }   // diagonal down-left
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int dr = directions[i, 0];
                int dc = directions[i, 1];

                // New cell is the middle 'O'
                totalSOS += CheckTriplet(row - dr, col - dc, row, col, row + dr, col + dc);

                // New cell is the first 'S'
                totalSOS += CheckTriplet(row, col, row + dr, col + dc, row + 2 * dr, col + 2 * dc);

                // New cell is the last 'S'
                totalSOS += CheckTriplet(row - 2 * dr, col - 2 * dc, row - dr, col - dc, row, col);
            }

            return totalSOS;
        }

        private int CheckTriplet(int r1, int c1, int r2, int c2, int r3, int c3)
        {
            return base.TryCreateSOSLine(r1, c1, r2, c2, r3, c3);
        }
    }
}
