using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private bool maxToMove = true;

        public short[] maxPlayer, minPlayer;

        public static readonly Tuple<short, short>[] DicePairs;

        static Backgammon()
        {
            // Precompute all possible dice combinations. The order of the dice can be ignored.
            // Therefore we only end up with 21 combinations instead of 36 given by 6 x 6.
            DicePairs = new Tuple<short, short>[21];
            int counter = 0;
            for (int i = 1; i <= 6; i++)
            {
                // Start with i to ignore duplicates by means of different order.
                for (int j = i; j <= 6; j++)
                {
                    DicePairs[counter++] = new Tuple<short, short>((short)i, (short)j);
                }
            }
        }

        private Backgammon(short[] maxPlayer, short[] minPlayer, bool maxToMove, MoveGroup move)
        {
            this.maxPlayer = maxPlayer;
            this.minPlayer = minPlayer;
            this.maxToMove = maxToMove;
            LastMove = move;
        }

        public MoveGroup LastMove { get; private set; }

        public bool MaxToMove()
        {
            return maxToMove;
        }

        public bool MinToMove()
        {
            return !maxToMove;
        }

        public bool IsTerminal()
        {
            var player = maxToMove ? maxPlayer : minPlayer;

            // Checks if the player has no more checkers on the board
            for (int i = 0; i < NumPoints; i++)
            {
                if (player[i] > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public double Utility()
        {
            return 0;
        }

        /// <summary>
        /// Returns the initial state of the game. The initial arrangement of checkers is: 
        /// two on each player's twenty-four point, five on each player's thirteen point, 
        /// three on each player's eight point, and five on each player's six point.
        /// </summary>
        public static Backgammon Start()
        {
            short[] player = new short[NumPoints];

            player[23] = 2;
            player[12] = 5;
            player[7] = 3;
            player[5] = 5;

            return new Backgammon(player, (short[])player.Clone(), true);
        }

        /// <summary>
        /// Returns a set of all possible successor states.
        /// </summary>
        /// <param name="diceOne">First dice value.</param>
        /// <param name="diceTwo">Second dice value.</param>
        public IEnumerable<Backgammon> Expand(short diceOne, short diceTwo)
        {
            short[] player = maxToMove ? maxPlayer : minPlayer;
            short[] opponent = maxToMove ? minPlayer : maxPlayer;

            // Holds the index for each checker on a point.
            // There is a maximum of 15 checkers for each player.
            var checkersList = new List<short>(15);
            for (short point = 0; point < NumPoints; point++)
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
                    checkersList.Add(point);
                }
            }

            // Use array instead of list for better performance.
            var checkers = checkersList.ToArray();
            
            // Holds all our successors
            var successors = new List<Backgammon>(300);

            // Iterate over each possible combination of two checkers.
            for (int i = 0; i < checkers.Length; i++)
            {
                for (int j = 0; j < checkers.Length; j++)
                {
                    // Each i and j reference a checker inside the checkers array (c)

                    var playerCopy = ArrayHelper.FastArrayCopy(player);
                    var checkersCopy = ArrayHelper.FastArrayCopy(checkers);
                    var moveGroup = new MoveGroup();
                    moveGroup.AddMove(checkersCopy[i], diceOne);
                    MoveChecker(diceOne, i, playerCopy, ref checkersCopy); // Move first checker
                    moveGroup.AddMove(checkersCopy[j], diceTwo);
                    MoveChecker(diceTwo, j, playerCopy, ref checkersCopy); // Move second checker

                    if (MaxToMove())
                    {
                        successors.Add(new Backgammon(playerCopy, opponent, false, moveGroup));
                    }
                    else
                    {
                        successors.Add(new Backgammon(opponent, playerCopy, true, moveGroup));
                    }

                    successors.Last().VerifyState();
                }
            }

            return successors;
        }

        private static void MoveChecker(short pips, int checker, short[] player, ref short[] checkers)
        {
            player[checkers[checker]]--; // remove checker from source point
            checkers[checker] -= pips; // update new index of checker

            // If new position is outside board...
            if (checkers[checker] < 0)
            {
                // ... then bear off.
                checkers = ArrayHelper.RemoveAt(checkers, checker);
            }
            else
            {
                // ... otherwise place checker at new position
                player[checkers[checker]]++;
            }
        }

        /// <summary>
        /// Verifies the current state. Emits an error message if the state is invalid.
        /// </summary>
        [Conditional("DEBUG")]
        private void VerifyState()
        {
            // Check for each point if the point is occupied by no or one player.
            for (int i = 0; i < NumPoints; i++)
            {
                // Return false if a point is occupied by both players.
                if (maxPlayer[i] > 0 && minPlayer[23 - i] > 0)
                {
                    Debug.Fail("Point is occupied by both players.");
                }
            }
        }
    }
}

