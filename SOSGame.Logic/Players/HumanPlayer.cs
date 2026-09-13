using System;
using SOSGame.Logic;

namespace SOSGame.Logic.Players
{
    public class HumanPlayer : IGamePlayer
    {
        public char DefaultLetter { get; }

        public HumanPlayer(char defaultLetter)
        {
            if (defaultLetter != 'S' && defaultLetter != 'O')
                throw new ArgumentException("Letter must be 'S' or 'O'");

            DefaultLetter = defaultLetter;
        }

        public (int Row, int Col) GetNextMove(BaseGame _)
            => throw new InvalidOperationException("Human moves come from GUI");

        public (int Row, int Col, char Letter) GetNextMoveWithLetter(BaseGame _)
            => throw new InvalidOperationException("Human moves come from GUI");
    }
}
