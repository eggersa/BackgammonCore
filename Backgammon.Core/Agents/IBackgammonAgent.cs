namespace Backgammon.Game.Agents
{
    public interface IBackgammonAgent
    {
        string Name { get; }

        Ply NextPly(DiceRoll roll, Backgammon game);
    }
}
