using Backgammon.Game.Agents;
using Backgammon.Game.Common;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backgammon.Game
{
    class Program
    {
        private const bool Interactive = false;

        static Program()
        {
            ConsoleErrorWriterDecorator.SetToConsole();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        static void Main(string[] args)
        {
            try
            {
                if (Interactive)
                {
                    RunGameInteractive(Backgammon.Setup(), new ExpectimaxBackgammonAgent());
                }
                else
                {
                    MeasureAgentVsAgent(new ExpectimaxBackgammonAgent(), new RandomBackgammonAgent(), 10);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static void MeasureAgentVsAgent(IBackgammonAgent player, IBackgammonAgent adversary, int iterations = 20)
        {
            Console.WriteLine($"Running {player.Name} against {adversary.Name} with {iterations} iterations...");

            bool[] result = new bool[iterations];
            int elapsedMilliseconds = 0;

            Parallel.For(0, iterations, new Action<int>((i) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                result[i] = RunGameSilent(player, adversary);
                sw.Stop();
                Interlocked.Add(ref elapsedMilliseconds, (int)sw.ElapsedMilliseconds);
                Console.WriteLine($"{(result[i] ? player.Name : adversary.Name)} has won iteration!");
            }));

            Console.WriteLine();
            Console.WriteLine($"Total elapsed time: {(double)elapsedMilliseconds / 1000:0.000} ms");
            Console.WriteLine($"Avg. time per iteration: {(double)elapsedMilliseconds / iterations / 1000:0.000} s");
            Console.WriteLine($"Successrate for {player.Name}: {(double)result.Select(r => r ? 1 : 0).Sum() / iterations * 100:0.00} %");
        }

        private static bool RunGameSilent(IBackgammonAgent player, IBackgammonAgent adverary)
        {
            DiceRoll roll;
            Ply ply;
            var game = Backgammon.Setup();

            while (true)
            {
                roll = DiceCup.Roll();
                ply = player.NextPly(roll, game);
                game.ExecutePly(ply);

                if (game.IsTerminal())
                {
                    return true;
                }

                roll = DiceCup.Roll();
                ply = adverary.NextPly(roll, game);
                game.ExecutePly(ply);

                if (game.IsTerminal())
                {
                    return false;
                }
            }
        }

        private static void RunGameInteractive(Backgammon game, IBackgammonAgent adversary)
        {
            var roll = DiceCup.Roll();
            BackgammonPrinter.Print(game);
            Console.WriteLine($"==== {roll} ====");
            Console.WriteLine();
            Ply ply;
            while (true)
            {
                while (true)
                {
                    ply = ReadPly(roll);
                    if (ply == null || game.ValidatePly(ply, roll))
                    {
                        break;
                    }
                    Console.Error.WriteLine("One or more moves are invalid.");
                }

                if (ply == null)
                {
                    break;
                }

                game.ExecutePly(ply);
                Console.Clear();

                if (game.MinToMove())
                {
                    // Let adversary agent play
                    roll = DiceCup.Roll();
                    BackgammonPrinter.Print(game);
                    Console.WriteLine($"==== {roll} ====");
                    Console.WriteLine();
                    Console.WriteLine("Agent is thinking... ");
                    ply = adversary.NextPly(roll, game);
                    game.ExecutePly(ply);
                    Console.Clear();
                }

                RunGameInteractive(game, adversary);
            }

            Console.Clear();
            BackgammonPrinter.Print(game);
        }

        private static Ply ReadPly(DiceRoll roll)
        {
            var moveOne = ReadMove(roll.One);
            if (moveOne == null)
            {
                return null;
            }

            var moveTwo = ReadMove(roll.Two);
            if (moveTwo == null)
            {
                return null;
            }

            return new Ply(moveOne, moveTwo);
        }

        private static Move ReadMove(short dice)
        {
            Console.Write($"Enter move (<pips:number>) for dice {dice} or q to quit: ");
            var command = Console.ReadLine();
            command = command.Replace(" ", "");
            if (command == "q")
            {
                return null;
            }

            if (!short.TryParse(command, out short point))
            {
                Console.Error.WriteLine("Input is not recognized. ");
                return ReadMove(dice);
            }

            // 25 for ckecker on bar
            if (point > 25 || point < 1)
            {
                Console.Error.Write("Move is not valid. ");
                return ReadMove(dice);
            }

            return new Move(--point, dice);
        }
    }
}
