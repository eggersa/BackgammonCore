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
        public static readonly Tuple<short, short>[] DicePairs;
        public bool actualGame = false;

        /// <summary>
        /// Initializes static field and properties of the class.
        /// </summary>
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
            this.MaxPlayer = maxPlayer;
            this.MinPlayer = minPlayer;
            this.maxToMove = maxToMove;
            LastMove = move;
        }

        public Player MaxPlayer { get; private set; }

        public Player MinPlayer { get; private set; }

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
            return MaxPlayer.IsFinished() || MinPlayer.IsFinished();
        }

        public Player GetCurrentPlayer()
        {
            return maxToMove ? MaxPlayer : MinPlayer;
        }

        /// <summary>
        /// Returns the initial state of the game. The initial arrangement of checkers is: 
        /// two on each player's twenty-four point, five on each player's thirteen point, 
        /// three on each player's eight point, and five on each player's six point.
        /// </summary>
        public static Backgammon Setup()
        {
            var bckg = new Backgammon(new Player() { Name = "Max" }, new Player() { Name = "Min" }, true, null);
            bckg.actualGame = true;
            return bckg;
        }

        public bool ValidatePly(Ply ply, DiceRoll roll)
        {
            var moves = GetPossibleMoves(new Tuple<short, short>(roll.One, roll.Two)).ToList();
            return moves.Contains(ply, PlyEqualityComparer.Instance);
        }

        /// <summary>
        /// Executes the given ply on the current game state.
        /// </summary>
        /// <param name="ply"></param>
        /// <param name="rollbackOnError">If false (default), an exception is thrown if an error (invalid state) occures.</param>
        /// <returns>True if the ply has been successfully executed.
        /// Return false if an has error occured (invalid move) and the state has been roll back.</returns>
        public bool ExecutePly(Ply ply, bool rollbackOnError = false)
        {
            Player player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

            Player maxClone = null, minClone = null;
            if (rollbackOnError)
            {
                maxClone = MaxPlayer.Clone();
                minClone = MinPlayer.Clone();
            }

            foreach (var move in ply.GetMoves())
            {
                var nTargets = GetNumOpponentCheckersOnTarget(opponent.Board, move.Checker, move.Pips);
                if (nTargets > 1) // check if target is open
                {
                    if (rollbackOnError)
                    {
                        // restore previous state
                        MaxPlayer = maxClone;
                        MinPlayer = minClone;

                        return false;
                    }
                    throw new InvalidOperationException($"Move not allowed: {move}");
                }
                else if (nTargets == 1)
                {
                    HitOpponent(move.Pips, opponent, move.Checker);
                }

                player.Board = MoveChecker(player.Board, move.Checker, move.Pips);
            }

            LastMove = ply;

            // update players
            MaxPlayer.Board = (maxToMove ? player : opponent).Board;
            MinPlayer.Board = (maxToMove ? opponent : player).Board;
            
            maxToMove = !maxToMove; // switch current player

            return true;
        }

        [Obsolete("Use GetPossibleMoves(DiceRoll) instead.")]
        public Ply[] GetPossibleMoves(Tuple<short, short> roll)
        {
            return GetPossibleMoves(new DiceRoll(roll.Item1, roll.Item2));
        }

        // TODO: Might not retur all moves in an endgame
        public Ply[] GetPossibleMoves(DiceRoll roll)
        {
            short diceOne = roll.One, diceTwo = roll.Two;

            Player player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

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
            Player player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

            // Holds all our successors
            var possibleMoves = GetPossibleMoves(roll);
            var successors = new List<Backgammon>(possibleMoves.Length);
            foreach (var move in possibleMoves)
            {
                var bckg = new Backgammon(player.Clone(), opponent.Clone(), true, null);
                bckg.ExecutePly(move); // apply move for current player
                successors.Add(bckg);
            }

            return successors;
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
                if (MaxPlayer.Board[i] > 0 && MinPlayer.Board[23 - i] > 0)
                {
                    var rev = MinPlayer.Board.Reverse();

                    Debug.WriteLine($"Invalid move: {LastMove}");
                    Debug.WriteLine(string.Join(' ', MaxPlayer));
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

            return $"{move}Max {MaxPlayer}\nMin {MinPlayer}";
        }
    }
}

