using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Backgammon.Game
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

        private readonly Player maxPlayer, minPlayer;
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

        private Backgammon(Player maxPlayer, Player minPlayer, bool maxToMove, Ply move)
        {
            this.maxPlayer = maxPlayer;
            this.minPlayer = minPlayer;
            this.maxToMove = maxToMove;
            LastMove = move;
        }

        public Player MaxPlayer
        {
            get { return maxPlayer; }
        }

        public Player MinPlayer
        {
            get { return maxPlayer; }
        }

        public Ply LastMove { get; private set; }

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
            return player.IsFinished();
        }

        public double Utility()
        {
            if (MaxToMove())
            {
                return maxPlayer.Evaluate();
            }
            return minPlayer.Evaluate();
        }

        public bool ApplyMove(Ply ply)
        {
            Player player = maxToMove ? maxPlayer : minPlayer,
                   opponent = maxToMove ? minPlayer : maxPlayer;

            foreach (var move in ply.GetMoves())
            {
                var nTargets = GetNumOpponentCheckersOnTarget(opponent.Board, move.Checker, move.Pips);
                if (nTargets > 1)
                {
                    throw new ArgumentException($"Move not allowed: {move}"); // TODO: Rollback
                }
                else if (nTargets == 1)
                {
                    HitOpponent(move.Pips, opponent, move.Checker);
                }

                player.Board = MoveChecker(player.Board, move.Checker, move.Pips);
            }

            LastMove = ply;

            return player.IsFinished();
        }

        /// <summary>
        /// Returns the initial state of the game. The initial arrangement of checkers is: 
        /// two on each player's twenty-four point, five on each player's thirteen point, 
        /// three on each player's eight point, and five on each player's six point.
        /// </summary>
        public static Backgammon Setup()
        {
            return new Backgammon(new Player(), new Player(), true, null);
        }

        public Ply[] GetPossibleMoves(Tuple<short, short> roll)
        {
            short diceOne = roll.Item1, diceTwo = roll.Item2;

            Player player = maxToMove ? maxPlayer : minPlayer,
                   opponent = maxToMove ? minPlayer : maxPlayer;

            // Keep track of plies to ignore duplicates by means of different move order
            var expansion = new HashSet<Ply>();

            foreach (var firstCheckerToMove in FindOccupiedPoints(player.Board))
            {
                Player opponentAfterFirstMove = opponent.Clone();

                short nTarget = GetNumOpponentCheckersOnTarget(opponentAfterFirstMove.Board, firstCheckerToMove, diceOne);
                if (nTarget <= 1)
                {
                    if (nTarget == 1 /* blot */)
                    {
                        HitOpponent(diceOne, opponentAfterFirstMove, firstCheckerToMove);
                    }
                }
                else
                {
                    // target point is already occupied by opponent
                    continue;
                }

                short[] playerAfterFirstMove = MoveChecker(player.Board, firstCheckerToMove, diceOne);

                var ply = new Ply();
                ply.AddMove(firstCheckerToMove, diceOne);

                var occupiedPointsAfterFirstMove = FindOccupiedPoints(playerAfterFirstMove);
                if (occupiedPointsAfterFirstMove.Length == 0) // check if game is finished after first move
                {
                    return new Ply[] { ply }; // no need to find additional moves (pruning)
                }

                foreach (var secondCheckerToMove in occupiedPointsAfterFirstMove)
                {
                    Player opponentAfterSecondMove = opponentAfterFirstMove.Clone();

                    nTarget = GetNumOpponentCheckersOnTarget(opponentAfterSecondMove.Board, secondCheckerToMove, diceTwo);
                    if (nTarget <= 1)
                    {
                        if (nTarget == 1 /* blot */)
                        {
                            HitOpponent(diceTwo, opponentAfterSecondMove, secondCheckerToMove);
                        }
                    }
                    else
                    {
                        // target point is already occupied by opponent
                        continue;
                    }

                    expansion.Add(new Ply(new Move(firstCheckerToMove, diceOne), new Move(secondCheckerToMove, diceTwo)));
                }
            }

            return expansion.ToArray();
        }

        /// <summary>
        /// Returns a set of all possible successor states.
        /// </summary>
        /// <param name="diceOne">First dice value.</param>
        /// <param name="diceTwo">Second dice value.</param>
        public IEnumerable<Backgammon> Expand(Tuple<short, short> roll)
        {
            Player player = maxToMove ? maxPlayer : minPlayer,
                   opponent = maxToMove ? minPlayer : maxPlayer;

            // Holds all our successors
            var possibleMoves = GetPossibleMoves(roll);
            var successors = new List<Backgammon>(possibleMoves.Length);
            foreach (var move in possibleMoves)
            {
                var bckg = new Backgammon(player.Clone(), opponent.Clone(), true, null);
                bckg.ApplyMove(move); // apply move for current player
                bckg.maxToMove = !bckg.maxToMove; // now switch player
                successors.Add(bckg);
            }

            return successors;

            ////var perror = new Ply(new Move(9, 2), new Move(5, 4));
            //foreach (var firstCheckerToMove in FindOccupiedPoints(playerBoard))
            //{
            //    Player opponentAfterFirstMove = opponent.Clone();

            //    short nTarget = GetNumOpponentCheckersOnTarget(opponentAfterFirstMove.Board, firstCheckerToMove, diceOne);
            //    if (nTarget <= 1)
            //    {
            //        if (nTarget == 1 /* blot */)
            //        {
            //            HitOpponent(diceOne, opponentAfterFirstMove, firstCheckerToMove);
            //        }
            //    }
            //    else
            //    {
            //        // target point is already occupied by opponent
            //        continue;
            //    }

            //    short[] playerAfterFirstMove = MoveChecker(playerBoard, firstCheckerToMove, diceOne);

            //    foreach (var secondCheckerToMove in FindOccupiedPoints(playerAfterFirstMove))
            //    {
            //        Player opponentAfterSecondMove = opponentAfterFirstMove.Clone();

            //        nTarget = GetNumOpponentCheckersOnTarget(opponentAfterSecondMove.Board, secondCheckerToMove, diceTwo);
            //        if (nTarget <= 1)
            //        {
            //            if (nTarget == 1 /* blot */)
            //            {
            //                HitOpponent(diceTwo, opponentAfterSecondMove, secondCheckerToMove);
            //            }
            //        }
            //        else
            //        {
            //            // target point is already occupied by opponent
            //            continue;
            //        }

            //        var ply = new Ply(new Move(firstCheckerToMove, diceOne), new Move(secondCheckerToMove, diceTwo));

            //        if (expansion.Add(ply))
            //        {
            //            short[] playerAfterSecondMove = MoveChecker(playerAfterFirstMove, secondCheckerToMove, diceTwo);

            //            if (MaxToMove())
            //            {
            //                successors.Add(new Backgammon(new Player(playerAfterSecondMove), opponentAfterSecondMove, false, ply));
            //            }
            //            else
            //            {
            //                successors.Add(new Backgammon(opponentAfterSecondMove, new Player(playerAfterSecondMove), true, ply));
            //            }
            //        }

            //        // successors.Last().VerifyState();
            //    }
            //}


        }

        private static void HitOpponent(short pips, Player opponent, short checker)
        {
            opponent.Board[23 - (checker - pips)]--; // remove opponent checker from his board

            // TODO: Uncomment

            // opponent.Bar++; // and put it on his bar
        }

        /// <summary>
        /// Moves a checker forward by the specified distance (pips).
        /// </summary>
        /// <param name="player">Array holding the checkers.</param>
        /// <param name="checkerIndex">Index of checker to move.</param>
        /// <param name="pips">Difference in pips between source and target point.</param>
        /// <returns>A new checkers array with the move applied.</returns>
        private static short[] MoveChecker(short[] player, short checkerIndex, short pips)
        {
            var playerCopy = ArrayHelper.FastArrayCopy(player);

            playerCopy[checkerIndex]--; // remove checker from source point
            if (checkerIndex - pips > 0) // check for bear-off
            {
                playerCopy[checkerIndex - pips]++; // put checker on new point (left to right!)
            }

            return playerCopy;
        }

        private short[] FindOccupiedPoints(short[] player)
        {
            int index = 0;
            short[] occupied = new short[15];
            for (short i = 0; i < player.Length; i++)
            {
                if (player[i] > 0)
                {
                    occupied[index++] = i;
                }
            }

            return ArrayHelper.FastArrayCopy(occupied, index);
        }

        private short GetNumOpponentCheckersOnTarget(short[] opponent, short playerIndex, short pips)
        {
            if (playerIndex - pips < 0)
            {
                return 0;
            }

            return opponent[23 - (playerIndex - pips)]; // opponent is reversed
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
                if (maxPlayer.Board[i] > 0 && minPlayer.Board[23 - i] > 0)
                {
                    var rev = minPlayer.Board.Reverse();

                    Debug.WriteLine($"Invalid move: {LastMove}");
                    Debug.WriteLine(string.Join(' ', maxPlayer));
                    Debug.WriteLine(string.Join(' ', rev));
                }
            }
        }

        public override string ToString()
        {
            string move = string.Empty;
            if (LastMove != null)
            {
                move = LastMove.ToString() + "\n";
            }

            return $"{move}Max {maxPlayer}\nMin {minPlayer}";
        }
    }
}

