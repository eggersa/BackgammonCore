using Backgammon.Game.Common;
using System;
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
            ConsoleErrorWriterDecorator.SetToConsole();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-CH");
        }

        static void Main(string[] args)
        {
            var game = Backgammon.Setup();
            RunGameInteractive(game);
        }

        private static void RunGameInteractive(Backgammon game)
        {
            var roll = DiceCup.Roll();
            BackgammonPrinter.Print(game);
            Console.WriteLine($"==== {roll} ====");
            Console.WriteLine();
            Ply ply;
            while (true)
            {
                while(true)
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
                RunGameInteractive(game);
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

            if (point > 24 || point < 1)
            {
                Console.Error.Write("Move is not valid. ");
                return ReadMove(dice);
            }

            return new Move(--point, dice);
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
