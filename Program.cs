using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BackgammonCore
{
    class Program
    {
        private static Random rnd = new Random(4);

        static void Main(string[] args)
        {
            var game = Backgammon.Start();


            var roll = RollDice();



            //var sw = new Stopwatch();
            //sw.Start();
            //sw.Stop();
            //Console.WriteLine($"Elapsed milliseconds: {sw.ElapsedMilliseconds}");
        }


        private static (double score, MoveGroup bestMove) Expectimax(Backgammon state, Tuple<short, short> roll, int depth, bool chance = false)
        {

            if (state.IsTerminal() || depth == 0)
            {
                // Return the heuristic value of node
                return (state.Utility(), state.LastMove);
            }

            double bestScore = 0;
            MoveGroup bestMove = null;

            if (chance)
            {
                double weightedAverage = 0;
                var pairs = Backgammon.DicePairs;
                foreach (var pair in pairs)
                {
                    foreach (var child in state.Expand(pair.Item1, pair.Item2))
                    {
                        (double score, _) = Expectimax(child, pair, depth - 1, false);

                        if(pair.Item1 == pair.Item2)
                        {
                            weightedAverage += 1 / 36 * score;
                        }
                        else
                        {
                            weightedAverage += 2 / 36 * score;
                        }
                    }
                }

                bestScore = sum / pairs.Length;
            }
            else if (state.MinToMove())
            {
                // Return value of minimum-valued child state
                bestScore = double.PositiveInfinity;
                foreach (var child in state.Expand(0, 0))
                {
                    (double score, MoveGroup move) = Expectimax(child, null, depth - 1);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }

                return (bestScore, bestMove);
            }
            else if (state.MaxToMove())
            {
                // Return value of minimum-valued child state
                bestScore = double.NegativeInfinity;
                foreach (var child in state.Expand(0, 0))
                {
                    (double score, MoveGroup move) = Expectimax(child, null, depth - 1);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
            }


        //var combinations = Backgammon.DiceCombinations;

        //var successors = state.Expand(roll.Item1, roll.Item2);
        //for (int i = 0; i < successors.Count(); i++)
        //{


        //    foreach (var succ in successors.ElementAt(i).Expand(0, 0))
        //    {

        //    }
        //}



        //// Iterate over each possible move for the current roll
        //for (int i = 0; i < combinations.Length; i++)
        //{

        //}
    }

    private static Tuple<short, short> RollDice()
    {
        return new Tuple<short, short>((short)(rnd.Next(6) + 1), (short)(rnd.Next(6) + 1));
    }
}
}
