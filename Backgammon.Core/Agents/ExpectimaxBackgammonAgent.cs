﻿using System;

namespace Backgammon.Game.Agents
{
    public class ExpectimaxBackgammonAgent : IBackgammonAgent
    {
        private double bestScore = double.NegativeInfinity;

        public Ply NextPly(DiceRoll roll, Backgammon game)
        {
            (double _, Ply ply) = Expectimax(game, new Tuple<short, short>(roll.One, roll.Two), 2);
            return ply;
        }

        private (double score, Ply bestMove) Expectimax(Backgammon state, Tuple<short, short> roll, int depth, bool chance = false)
        {
            if (state.IsTerminal() || depth == 0)
            {
                // Return the heuristic value of node
                return (Evaluate(state.GetCurrentPlayer()), state.LastMove);
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

        /// <summary>
        /// Evaluates the players current position.
        /// </summary>
        /// <returns>Returns a value that determines the players current state.
        /// A lower value is better. A value of 0 means the player has won.
        /// </returns>
        private double Evaluate(Player player)
        {
            return Evaluate(player, 0.5, 1, 1.5);
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
        private static double Evaluate(Player player, double wCheckers, double wPips, double wBar)
        {
            return -(wCheckers * player.GetRemainingCheckers() + wPips * player.GetRemainingPips() + wBar * player.Bar);
        }
    }
}
