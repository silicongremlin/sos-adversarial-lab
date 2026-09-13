using System;
using System.Collections.Generic;

namespace SOSGame.Logic
{
    public abstract class BaseGame
    {
        protected char[,] board;
        protected int gridSize;
        protected int playerOneScore = 0;
        protected int playerTwoScore = 0;
        protected bool isPlayerOneTurn = true;
        protected int totalMovesMade = 0;

        // Collection for SOS lines used in dynamic UI updates.
        protected List<SOSLine> Lines { get; } = new List<SOSLine>();

        // Property to freeze the game state for input (used in SimpleGame).
        public bool IsFrozen { get; set; } = false;

        public BaseGame(int size)
        {
            gridSize = size;
            board = new char[size, size];
        }

        public abstract bool MakeMove(int row, int col, char letter);

        public char[,] GetBoard() => board;
        public bool IsPlayerOneTurn() => isPlayerOneTurn;
        public void SwitchTurn() => isPlayerOneTurn = !isPlayerOneTurn;
        public int GetPlayerOneScore() => playerOneScore;
        public int GetPlayerTwoScore() => playerTwoScore;
        public int GetTotalMoves() => totalMovesMade;

        public bool IsGameOver()
        {
            return totalMovesMade >= gridSize * gridSize;
        }

        protected bool IsValidMove(int row, int col, char letter)
        {
            return row >= 0 && col >= 0 &&
                   row < gridSize && col < gridSize &&
                   board[row, col] == '\0' &&
                   (letter == 'S' || letter == 'O');
        }

        public void PlaceLetter(int row, int col, char letter)
        {
            board[row, col] = letter;
            totalMovesMade++;
        }

        // Method for checking all 8 directions around the newly placed cell (row, col) and returns the total number of SOS formations found.
        protected int CountNewSOS(int row, int col, bool stopAfterFirst = false)
        {
            int totalSOS = 0;

            // 8 directions: up, down, left, right, and 4 diagonals.
            int[,] directions = new int[,]
            {
                { -1,  0 }, {  1,  0 },
                {  0, -1 }, {  0,  1 },
                { -1, -1 }, {  1,  1 },
                { -1,  1 }, {  1, -1 }
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int dr = directions[i, 0];
                int dc = directions[i, 1];

                // First Patterm: New cell is the middle letter 'O'
                totalSOS += CheckTriplet(row - dr, col - dc, row, col, row + dr, col + dc);

                // Second Pattern: New cell is the first letter 'S'
                totalSOS += CheckTriplet(row, col, row + dr, col + dc, row + 2 * dr, col + 2 * dc);

                // Third Pattern: New cell is the last letter 'S'
                totalSOS += CheckTriplet(row - 2 * dr, col - 2 * dc, row - dr, col - dc, row, col);

                if (stopAfterFirst && totalSOS > 0)
                    return totalSOS;
            }

            return totalSOS;
        }

        public int PreviewSOS(int row, int col, char letter)
        {
            if (board[row, col] != '\0') return 0;
            board[row, col] = letter;
            int result = CountNewSOS(row, col, stopAfterFirst: true);
            board[row, col] = '\0';
            return result;
        }


        // Checks if the three board cells form "S-O-S". If they do, adds a line to the Lines list.
        // Returns 1 if an SOS is found; otherwise, returns 0.
        protected int CheckTriplet(int r1, int c1, int r2, int c2, int r3, int c3)
        {
            if (!IsInBounds(r1, c1) || !IsInBounds(r2, c2) || !IsInBounds(r3, c3))
                return 0;

            if (board[r1, c1] == 'S' && board[r2, c2] == 'O' && board[r3, c3] == 'S')
            {
                string color = isPlayerOneTurn ? "Red" : "Blue";
                Lines.Add(new SOSLine(r1, c1, r3, c3, color));
                return 1;
            }

            return 0;
        }

        protected bool IsInBounds(int r, int c)
        {
            return r >= 0 && c >= 0 && r < gridSize && c < gridSize;
        }

        // Checks if the cells at (r1,c1), (r2,c2), (r3,c3) form "SOS" and adds an SOS line if true.
        protected int TryCreateSOSLine(int r1, int c1, int r2, int c2, int r3, int c3)
        {
            if (!IsInBounds(r1, c1) || !IsInBounds(r2, c2) || !IsInBounds(r3, c3))
                return 0;

            if (board[r1, c1] == 'S' && board[r2, c2] == 'O' && board[r3, c3] == 'S')
            {
                string color = isPlayerOneTurn ? "Red" : "Blue";
                Lines.Add(new SOSLine(r1, c1, r3, c3, color));
                return 1;
            }
            return 0;
        }

        // Expose SOS lines for UI updates.
        public List<SOSLine> GetSOSLines() => Lines;

        // This class is used to manage the game state and logic for the SOS game.
        public IList<(int Row, int Col)> GetEmptyCells()
        {
            var empties = new List<(int, int)>();
            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    if (board[r, c] == '\0')
                        empties.Add((r, c));
                }
            }
            return empties;
        }


        public bool WouldScore(char letter, int r, int c)
        {
            if (!IsInBounds(r, c) || board[r, c] != '\0' || (letter != 'S' && letter != 'O'))
                return false;

            // Temporarily place the letter and count SOS
            board[r, c] = letter;
            int found = CountNewSOS(r, c);
            board[r, c] = '\0';         // revert
            return found > 0;
        }
    }

    // Visually display a line drawn through an SOS formation.

    public class SOSLine
    {
        public int StartRow { get; }
        public int StartCol { get; }
        public int EndRow { get; }
        public int EndCol { get; }
        public string PlayerColor { get; }

        public SOSLine(int startRow, int startCol, int endRow, int endCol, string playerColor)
        {
            StartRow = startRow;
            StartCol = startCol;
            EndRow = endRow;
            EndCol = endCol;
            PlayerColor = playerColor;
        }
    }
}