using System;
using System.Collections.Generic;

namespace BackgammonCore
{
    /// <summary>
    /// The object of the game is move all your checkers into your own home board and then bear them off.
    /// The first player to bear off all of their checkers wins the game.
    /// For more details <see cref="https://www.bkgm.com/rules.html"/>.
    /// </summary>
    public class Backgammon
    {
        private const int NumPoints = 24;

        private int[] maxPlayer, minPlayer;

        public static readonly Tuple<int, int>[] DiceCombinations;

        static Backgammon()
        {
            // Precompute all possible dice combinations. The order of the dice can be ignored.
            // Therefore we only end up with 21 combinations instead of 36 given by 6 x 6.
            DiceCombinations = new Tuple<int, int>[21];
            int counter = 0;
            for (int i = 1; i <= 6; i++)
            {
                // Start with i to ignore duplicates by means of different order.
                for (int j = i; j <= 6; j++)
                {
                    DiceCombinations[counter++] = new Tuple<int, int>(i, j);
                }
            }
        }

        private Backgammon(int[] maxPlayer, int[] minPlayer)
        {
            this.maxPlayer = maxPlayer;
            this.minPlayer = minPlayer;
        }

        /// <summary>
        /// Returns the initial state of the game. The initial arrangement of checkers is: 
        /// two on each player's twenty-four point, five on each player's thirteen point, 
        /// three on each player's eight point, and five on each player's six point.
        /// </summary>
        public static Backgammon Start()
        {
            int[] player = new int[NumPoints];

            player[23] = 2;
            player[12] = 5;
            player[7] = 3;
            player[5] = 5;

            return new Backgammon(player, (int[])player.Clone());
        }

        /// <summary>
        /// Returns a set of all possible successor states.
        /// </summary>
        /// <param name="a">First dice value.</param>
        /// <param name="b">Second dice value.</param>
        public IEnumerable<Backgammon> Expand(int a, int b, bool max)
        {
            int[] player = max ? maxPlayer : minPlayer;
            int[] opponent = max ? minPlayer : maxPlayer;

            // Holds the index for each checker on a point.
            var checkers = new List<int>(15);

            for (int point = 0; point < NumPoints; point++)
            {
                int numCheckersOnPoint = player[point];

                // Continue to next point if no checker is at the current point
                if (numCheckersOnPoint == 0)
                {
                    continue;
                }

                // For each checker on the current point, store its index
                for (int k = 0; k < numCheckersOnPoint; k++)
                {
                    checkers.Add(point);
                }
            }

            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    var p = (int[])player.Clone();
                    var c = new List<int>(checkers);

                    MoveChecker(a, 0, p, c);
                    MoveChecker(b, 0, p, c);

                    if (max)
                        yield return new Backgammon(p, opponent);
                    else
                        yield return new Backgammon(opponent, p);
                }
            }
        }

        private static void MoveChecker(int pips, int checker, int[] player, List<int> checkers)
        {
            player[checkers[checker]]--;
            checkers[checker] -= pips;

            // If new position is outside board...
            if (checkers[checker] < 0)
            {
                // ... then bear off.
                checkers.RemoveAt(checker);
            }
            else
            {
                // ... otherwise place checker at new position
                player[checkers[checker]]++;
            }
        }

        /// <summary>
        /// Validates the current state.
        /// </summary>
        /// <returns>True if the state is valid; false otherwise.</returns>
        private bool Validate()
        {
            // Check for each point if the point is occupied by no or one player.
            for (int i = 0; i < NumPoints; i++)
            {
                // Return false if a point is occupied by both players.
                if (maxPlayer[i] > 0 && minPlayer[i] > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

