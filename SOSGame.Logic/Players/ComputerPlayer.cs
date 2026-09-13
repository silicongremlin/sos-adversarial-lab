using System;
using SOSGame.Logic;
using SOSGame.Logic.Players;

namespace SOSGame.Logic.Players
{

    // Complete an SOS if possible.
    // Otherwise block an opponent SOS.
    // Otherwise pick a random empty cell with a random letter.
    public class ComputerPlayer : IGamePlayer
    {
        public char DefaultLetter { get; }

        public char Letter => DefaultLetter;

        private readonly Random _rng = new Random();

        public ComputerPlayer(char defaultLetter)
        {
            DefaultLetter = defaultLetter;
        }

        public (int Row, int Col) GetNextMove(BaseGame game)
        {
            var (r, c, _) = GetNextMoveWithLetter(game);
            return (r, c);
        }

        public (int Row, int Col, char Letter) GetNextMoveWithLetter(BaseGame game)
        {
            if (FindBest(game, out var best)) return best;   // win
            if (FindBlock(game, out var block)) return block;  // block

            // Random fallback
            int size = game.GetBoard().GetLength(0);
            while (true)
            {
                int r = _rng.Next(size);
                int c = _rng.Next(size);
                if (game.GetBoard()[r, c] == '\0')
                {
                    char l = _rng.Next(2) == 0 ? 'S' : 'O';
                    return (r, c, l);
                }
            }
        }

        private bool FindBest(BaseGame game, out (int Row, int Col, char Letter) best)
        {
            char[,] board = game.GetBoard();
            int n = board.GetLength(0);

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (board[r, c] != '\0') continue;

                    foreach (char l in new[] { 'S', 'O' })
                    {
                        if (game.PreviewSOS(r, c, l) > 0)
                        {
                            best = (r, c, l);
                            return true;
                        }
                    }
                }
            }

            best = default;
            return false;
        }

        private bool FindBlock(BaseGame game, out (int Row, int Col, char Letter) move)
        {
            game.SwitchTurn();                 // look from opponent point of view
            bool ok = FindBest(game, out move);
            game.SwitchTurn();
            return ok;
        }
    }
}
