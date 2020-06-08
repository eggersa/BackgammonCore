using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Backgammon.Game
{
    class Program
    {
        private static Random rnd = new Random(4);

        static Program()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        static void Main(string[] args)
        {
            var game = Backgammon.Start();


            var roll = RollDice(5, 6);

            var sw = new Stopwatch();
            sw.Start();

            int iterations = 1;
            for (int i = 0; i < iterations; i++)
            {
                game.Expand(roll);
            }

            sw.Stop();
            Console.WriteLine($"Expand: {(double)sw.ElapsedMilliseconds / iterations:0.00} ms.");

            Console.ReadKey(true);
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

        private static Tuple<short, short> RollDice()
        {
            return new Tuple<short, short>((short)(rnd.Next(6) + 1), (short)(rnd.Next(6) + 1));
        }

        private static Tuple<short, short> RollDice(short a, short b)
        {
            return new Tuple<short, short>(a, b);
        }
    }
}
