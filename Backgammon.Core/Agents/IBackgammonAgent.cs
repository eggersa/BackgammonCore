namespace Backgammon.Game.Agents
{
    public interface IBackgammonAgent
    {
        Ply NextPly(DiceRoll roll, Backgammon game);
    }
}
