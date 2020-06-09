using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Backgammon.Game
{
    class Program
    {
        private static Random rnd = new Random();

        static Program()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        static void Main(string[] args)
        {
            var game = Backgammon.Setup();
            var roll = RollDice();

            PrintGameOnConsole(game);


            //int counter = 0;
            //Ply ply;
            //do
            //{
            //    Console.WriteLine(game);

            //    do
            //    {
            //        // (_, ply) = Expectimax(game, roll, 2);
            //        ply = FindRandomMove(game, roll);
            //        roll = RollDice();
            //    } while (ply == null);

            //    // Console.WriteLine();

            //    counter++;
            //    Console.WriteLine("counter: " + counter);
            //} while (!game.ApplyMove(ply));

            //Console.WriteLine(game);
            //Console.WriteLine($"Game finished with {counter} moves");

            Console.ReadKey(true);
        }

        private static void PrintGameOnConsole(Backgammon game)
        {
            PrintColor("|c13d|14|c15d|16|c17d|18|  |c19d|20|c21d|22|c23d|24|\n");

            var minboardreverse = ArrayHelper.FastArrayCopy(game.MinPlayer.Board);
            minboardreverse = minboardreverse.Reverse().ToArray();

            var minboardtop = new short[12];
            Array.Copy(minboardreverse, 12, minboardtop, 0, 12);
            var maxboardtop = new short[12];
            Array.Copy(game.MaxPlayer.Board, 12, maxboardtop, 0, 12);
            
            PrintGameTopOnConsole(maxboardtop, minboardtop);

            Console.WriteLine();

            var maxboardbottom = ArrayHelper.FastArrayCopy(game.MaxPlayer.Board, 12);
            var minboardbottom = ArrayHelper.FastArrayCopy(minboardreverse, 12);
            
            PrintGameBottomOnConsole(maxboardbottom, minboardbottom);

            PrintColor("|12|c11d|10|c09d|08|c07d|  |06|c05d|04|c03d|02|c01d|");
        }

        private static void PrintGameTopOnConsole(short[] maxboardtop, short[] minboardtop)
        {
            while (Math.Max(minboardtop.Max(), maxboardtop.Max()) > 0)
            {
                for (int i = 0; i < maxboardtop.Length; i++)
                {
                    if (i == 6) Console.Write("   "); // middle bar
                    if (maxboardtop[i] + minboardtop[i] > 0)
                    {
                        if (minboardtop[i] > 0)
                        {
                            minboardtop[i]--;
                            PrintColor($"  rx");
                        }
                        else
                        {
                            maxboardtop[i]--;
                            PrintColor($"  go");
                        }
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                }
                Console.WriteLine();
            }
        }

        private static void PrintGameBottomOnConsole(short[] maxboardbottom, short[] minboardbottom)
        {
            int max;
            while ((max = Math.Max(minboardbottom.Max(), maxboardbottom.Max())) > 0)
            {
                for (int i = 11; i >= 0; i--)
                {
                    if (i == 5) Console.Write("   "); // middle bar
                    if (maxboardbottom[i] + minboardbottom[i] == max)
                    {
                        if (minboardbottom[i] == max)
                        {
                            minboardbottom[i]--;
                            PrintColor($"  rx");
                        }
                        else
                        {
                            maxboardbottom[i]--;
                            PrintColor($"  go");
                        }
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                }
                Console.WriteLine();
            }
        }

        private static void PrintColor(string str)
        {
            var restoreColor = Console.ForegroundColor;
            foreach (char ch in str)
            {
                if (ch == 'r')
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (ch == 'g')
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (ch == 'c')
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (ch == 'd') // default color
                {
                    Console.ForegroundColor = restoreColor;
                }
                else
                {
                    Console.Write(ch);
                }
            }
            Console.ForegroundColor = restoreColor;
        }

        private static int CountMoves(Tuple<short, short> initial)
        {
            var game = Backgammon.Setup();
            int counter = 0;
            Ply ply;
            do
            {
                do
                {
                    (_, ply) = Expectimax(game, initial, 2);
                    // ply = FindRandomMove(game, initial);
                    initial = RollDice();
                } while (ply == null);
                counter++;
                Console.WriteLine(game);
            } while (!game.ApplyMove(ply));

            return counter;
        }

        private static Ply FindRandomMove(Backgammon state, Tuple<short, short> roll)
        {
            var moves = state.GetPossibleMoves(roll);
            if (!moves.Any())
            {
                return null;
            }

            var selection = rnd.Next(0, moves.Count());
            return moves[selection];
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
