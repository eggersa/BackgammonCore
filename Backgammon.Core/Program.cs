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
            RunGameInteractive(game);

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

        private static void RunGameInteractive(Backgammon game)
        {
            var roll = DiceCup.Roll();

            // TODO: Determine starting player

            BackgammonPrinter.Print(game);
            Ply ply;
            bool valid = true;
            do
            {
                if (!valid)
                {
                    Console.Clear();
                    BackgammonPrinter.Print(game);
                    Console.Error.WriteLine("One or more moves are not valid.");
                }
                ply = ReadPly(roll);
                if(ply == null)
                {
                    break;
                }
            } while (!(valid = game.ExecutePly(ply, true)));

            Console.Clear();

            BackgammonPrinter.Print(game);
        }

        private static Ply ReadPly(DiceRoll roll)
        {
            var moveOne = ReadMove(roll.One);
            if(moveOne == null)
            {
                return null;
            }

            var moveTwo = ReadMove(roll.Two);
            if(moveTwo == null)
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
                Console.Error.Write("Input is not recognized. ");
                return ReadMove(dice);
            }

            if (point > 24 || point < 1)
            {
                Console.Error.Write("Move is not valid. ");
                return ReadMove(dice);
            }

            return new Move(--point, dice);
        }

        //private static int CountMoves(Tuple<short, short> initial)
        //{
        //    var game = Backgammon.Setup();
        //    int counter = 0;
        //    Ply ply;
        //    do
        //    {
        //        do
        //        {
        //            (_, ply) = Expectimax(game, initial, 2);
        //            // ply = FindRandomMove(game, initial);
        //            initial = RollDice();
        //        } while (ply == null);
        //        counter++;
        //        Console.WriteLine(game);
        //    } while (!game.ExecutePly(ply));

        //    return counter;
        //}

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
