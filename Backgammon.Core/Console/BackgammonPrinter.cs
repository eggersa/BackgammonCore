using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Backgammon.Game
{
    /// <summary>
    /// Helper class to pretty print the game state on the console window.
    /// </summary>
    public static class BackgammonPrinter
    {
        public static void Print(Backgammon game)
        {
            if(game.LastMove != null)
            {
                Console.WriteLine($"Previous player move is {game.LastMove}");
                Console.WriteLine();
            }

            PrintColor("|c13d|14|c15d|16|c17d|18|  |c19d|20|c21d|22|c23d|24|\n");

            var minboardreverse = ArrayHelper.FastArrayCopy(game.MinPlayer.Board);
            minboardreverse = minboardreverse.Reverse().ToArray();

            var minboardtop = new short[12];
            Array.Copy(minboardreverse, 12, minboardtop, 0, 12);
            var maxboardtop = new short[12];
            Array.Copy(game.MaxPlayer.Board, 12, maxboardtop, 0, 12);

            PrintGameTop(maxboardtop, minboardtop, game.MaxPlayer.Bar);

            Console.WriteLine();

            var maxboardbottom = ArrayHelper.FastArrayCopy(game.MaxPlayer.Board, 12);
            var minboardbottom = ArrayHelper.FastArrayCopy(minboardreverse, 12);

            PrintGameBottom(maxboardbottom, minboardbottom, game.MinPlayer.Bar);

            PrintColor("|12|c11d|10|c09d|08|c07d|  |06|c05d|04|c03d|02|c01d|");

            Console.WriteLine();
            Console.WriteLine();
        }

        private static void PrintGameTop(short[] maxboardtop, short[] minboardtop, short maxbar)
        {
            int conter = 0;
            while (Math.Max(minboardtop.Max(), maxboardtop.Max()) > 0)
            {
                
                for (int i = 0; i < maxboardtop.Length; i++)
                {
                    if (i == 6) Console.Write("   "); // middle bar
                    if (maxboardtop[i] + minboardtop[i] > 0)
                    {
                        // checker is outside of board after bearing off
                        if (maxboardtop[i] < 0) maxboardtop[i] = 0;
                        if (minboardtop[i] < 0) minboardtop[i] = 0;

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

                if (conter == 0)
                {
                    if (maxbar > 0)
                    {
                        Console.Write(" | ");
                    }
                    for (int i = 0; i < maxbar; i++)
                    {
                        PrintColor($" go");
                    }
                }

                conter++;
                Console.WriteLine();
            }
        }

        private static void PrintGameBottom(short[] maxboardbottom, short[] minboardbottom, short minbar)
        {
            int max;
            int counter = Math.Max(minboardbottom.Max(), maxboardbottom.Max()) - 1;
            while ((max = Math.Max(minboardbottom.Max(), maxboardbottom.Max())) > 0)
            {
                for (int i = 11; i >= 0; i--)
                {
                    // checker is outside of board after bearing off
                    if (maxboardbottom[i] < 0) maxboardbottom[i] = 0;
                    if (minboardbottom[i] < 0) minboardbottom[i] = 0;

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

                if (counter == 0)
                {
                    if (minbar > 0)
                    {
                        Console.Write(" | ");
                    }
                    for (int i = 0; i < minbar; i++)
                    {
                        PrintColor($"rx");
                    }
                }

                counter--;
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
    }
}
