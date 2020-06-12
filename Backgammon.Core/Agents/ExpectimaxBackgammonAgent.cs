namespace Backgammon.Game.Agents
{
    public class ExpectimaxBackgammonAgent : IBackgammonAgent
    {
        public string Name => "Expectimax Agent";

        public Ply NextPly(DiceRoll roll, Backgammon game)
        {
            (double _, Ply ply) = Expectimax(game, roll, 2);
            return ply;
        }

        private (double score, Ply bestMove) Expectimax(Backgammon state, DiceRoll roll, int depth, bool chance = false)
        {
            if (state.IsTerminal() || depth == 0)
            {
                // Return the heuristic value of node
                return (Evaluate(state.GetCurrentPlayer()), state.LastPly);
            }

            double bestScore = 0;
            Ply bestMove = Ply.ZeroPly;

            if (chance)
            {
                var pairs = Backgammon.DicePairs;
                foreach (var pair in pairs)
                {
                    foreach (var child in state.Expand(pair))
                    {
                        (double score, _) = Expectimax(child, pair, depth, false); // chance nodes do not account for depth
                        if (pair.One == pair.Two)
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
                        bestMove = child.LastPly;
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
                        bestMove = child.LastPly;
                    }
                }
            }

            return (bestScore, bestMove);
        }

        /// <summary>
        /// Evaluates the players current position.
        /// </summary>
        /// <returns>Returns a value that determines the players current state.
        /// A lower value is better. A value of 0 means the player has won.
        /// </returns>
        private double Evaluate(PlayerState player)
        {
            return Evaluate(player, 1, 0.5, 1);
        }

        /// <summary>
        /// Evaluates the players current position.
        /// </summary>
        /// <param name="wCheckers">Weight for remaining checkers.</param>
        /// <param name="wPips">Weight for remaining pips.</param>
        /// <param name="wBar">Weight for checkers on bar.</param>
        /// <returns>Returns a value that determines the players current state.
        /// A bigger value is better. A value of 0 means the player has finished.
        /// </returns>
        private static double Evaluate(PlayerState player, double wCheckers, double wPips, double wBar)
        {
            return -(wCheckers * player.GetRemainingCheckers() + wPips * player.GetRemainingPips() + wBar * player.Bar);
        }
    }
}
