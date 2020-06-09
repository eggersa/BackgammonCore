using Backgammon.Game.Agents;
using Backgammon.Game.Common;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backgammon.Game
{
    class Program
    {
        private static Random rnd = new Random();

        static Program()
        {
            ConsoleErrorWriterDecorator.SetToConsole();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        static void Main(string[] args)
        {
            // RunGameInteractive(game, new ExpectimaxBackgammonAgent(game));

            try
            {
                int iterations = 10;
                bool[] result = new bool[iterations];

                Parallel.For(0, iterations, new Action<int>((i) =>
                {
                    result[i] = RunGameSilent(new ExpectimaxBackgammonAgent(), new RandomBackgammonAgent());
                }));

                double winLoseRatio = (double)result.Select(r => r ? 1 : 0).Sum() / iterations;
                Console.WriteLine($"Successrate is {winLoseRatio * 100:0.00} %");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
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

            if (point > 24 || point < 1)
            {
                Console.Error.Write("Move is not valid. ");
                return ReadMove(dice);
            }

            return new Move(--point, dice);
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
