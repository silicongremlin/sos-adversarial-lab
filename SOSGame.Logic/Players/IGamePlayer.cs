using SOSGame.Logic;

namespace SOSGame.Logic.Players
{
    public interface IGamePlayer
    {
        (int Row, int Col) GetNextMove(BaseGame game);

        (int Row, int Col, char Letter) GetNextMoveWithLetter(BaseGame game);

        char DefaultLetter { get; }
    }
}
