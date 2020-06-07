using System;
using System.Diagnostics;

namespace BackgammonCore
{
    class Program
    {


        static void Main(string[] args)
        {
            var game = Backgammon.Start();

            var sw = new Stopwatch();
            sw.Start();


            var a = new Tuple<int, int>[21];



            int counter = 0;
            for (int i = 0; i < 1; i++)
            {
                foreach (var succ in game.Expand(5, 6, true))
                {
                    counter++;
                }
            }

            sw.Stop();
            Console.WriteLine($"Elapsed milliseconds: {sw.ElapsedMilliseconds}");
            Console.WriteLine($"Instance count: {counter}");
        }
    }
}
