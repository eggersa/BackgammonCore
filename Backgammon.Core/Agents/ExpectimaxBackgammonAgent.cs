using System;

namespace Backgammon.Game.Agents
{
    public class ExpectimaxBackgammonAgent : IBackgammonAgent
    {
        private readonly Backgammon game;

        public ExpectimaxBackgammonAgent(Backgammon game)
        {
            this.game = game;
        }

        public Ply NextPly(DiceRoll roll)
        {
            (double _, Ply ply) = Expectimax(game, new Tuple<short, short>(roll.One, roll.Two), 2);
            return ply;
        }

        private static (double score, Ply bestMove) Expectimax(Backgammon state, Tuple<short, short> roll, int depth, bool chance = false)
        {
            if (state.IsTerminal() || depth == 0)
            {
                // Return the heuristic value of node
                return (state.Utility(), state.LastMove);
            }

            double bestScore = 0;
            Ply bestMove = null;

            if (chance)
            {
                var pairs = Backgammon.DicePairs;
                foreach (var pair in pairs)
                {
                    foreach (var child in state.Expand(pair))
                    {
                        (double score, _) = Expectimax(child, pair, depth, false); // chance nodes do not account for depth
                        if (pair.Item1 == pair.Item2)
                        {
                            bestScore += 1 / 36 * score;
                        }
                        else
                        {
                            bestScore += 2 / 36 * score;
                        }
                    }
                }
            }
            else if (state.MaxToMove())
            {
                // Return value of minimum-valued child state
                bestScore = double.NegativeInfinity;
                foreach (var child in state.Expand(roll))
                {
                    (double score, _) = Expectimax(child, roll, depth - 1, true);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = child.LastMove;
                    }
                }
            }
            else if (state.MinToMove())
            {
                // Return value of minimum-valued child state
                bestScore = double.PositiveInfinity;
                foreach (var child in state.Expand(roll))
                {
                    (double score, Ply move) = Expectimax(child, roll, depth - 1, true);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = child.LastMove;
                    }
                }
            }

            return (bestScore, bestMove);
        }
    }
}
