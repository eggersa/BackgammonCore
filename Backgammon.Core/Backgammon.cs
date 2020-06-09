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
        public static readonly DiceRoll[] DicePairs;
        public bool actualGame = false;

        /// <summary>
        /// Initializes static field and properties of the class.
        /// </summary>
        static Backgammon()
        {
            // Precompute all possible dice combinations. The order of the dice can be ignored.
            // Therefore we only end up with 21 combinations instead of 36 given by 6 x 6.
            DicePairs = new DiceRoll[21];
            int counter = 0;
            for (int i = 1; i <= 6; i++)
            {
                // Start with i to ignore duplicates by means of different order.
                for (int j = i; j <= 6; j++)
                {
                    DicePairs[counter++] = new DiceRoll((short)i, (short)j);
                }
            }
        }

        private Backgammon(PlayerState maxPlayer, PlayerState minPlayer, bool maxToMove, Ply move)
        {
            this.MaxPlayer = maxPlayer;
            this.MinPlayer = minPlayer;
            this.maxToMove = maxToMove;
            LastMove = move;
        }

        public PlayerState MaxPlayer { get; private set; }

        public PlayerState MinPlayer { get; private set; }

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

        public PlayerState GetCurrentPlayer()
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
            var bckg = new Backgammon(new PlayerState() { Name = "Max" }, new PlayerState() { Name = "Min" }, true, null);
            bckg.actualGame = true;
            return bckg;
        }

        public bool ValidatePly(Ply ply, DiceRoll roll)
        {
            var moves = GetPossiblePlies(roll).ToList();
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
            PlayerState player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

            PlayerState maxClone = null, minClone = null;
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
                    ApplyHitOnOpponent(opponent, move.Checker, move.Pips);
                }

                MoveCheckerOnPlayer(player, move.Checker, move.Pips);
            }

            LastMove = ply;

            // update players
            MaxPlayer.Board = (maxToMove ? player : opponent).Board;
            MinPlayer.Board = (maxToMove ? opponent : player).Board;

            maxToMove = !maxToMove; // switch current player

            return true;
        }

        public Ply[] GetPossiblePlies(DiceRoll roll)
        {
            PlayerState player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

            var biggestBar = 0;

            // Keep track of plies to ignore duplicates by means of different move order
            var expansion = new HashSet<Ply>();
            foreach (var ply in GetPossiblePlies(player, opponent, roll.One, roll.Two))
            {
                // Ensure that the player plays moves as many checkers as possible from the bar.
                if (ply.CountBarMovements > biggestBar)
                {
                    biggestBar = ply.CountBarMovements;
                    expansion.Clear(); // All previous moves are invalid in this case
                }
                else if (ply.CountBarMovements < biggestBar)
                {
                    continue;
                }

                expansion.Add(ply);
            }

            if (!expansion.Any()) // In certain cases only one of two dice can be played.
            {
                foreach (var ply in GetPossiblePlies(player, opponent, roll.Two, roll.One))
                {
                    // Ensure that the player plays moves as many checkers as possible from the bar.
                    if (ply.CountBarMovements > biggestBar)
                    {
                        biggestBar = ply.CountBarMovements;
                        expansion.Clear(); // All previous moves are invalid in this case
                    }
                    else if (ply.CountBarMovements < biggestBar)
                    {
                        continue;
                    }

                    expansion.Add(ply);
                }
            }

            return expansion.ToArray();
        }

        private Ply[] GetPossiblePlies(PlayerState player, PlayerState opponent, short diceOne, short diceTwo)
        {
            // Keep track of plies to ignore duplicates by means of different move order
            var expansion = new HashSet<Ply>();

            var moves = GetPossibleMoves(player, opponent, diceOne);
            foreach (var firstMove in moves)
            {
                var playerClone = player.Clone();
                var opponentClone = opponent.Clone();
                ExecuteMove(playerClone, opponentClone, firstMove);

                if (playerClone.IsFinished())
                {
                    expansion.Add(new Ply(firstMove));
                    continue;
                }

                foreach (var secondMove in GetPossibleMoves(playerClone, opponentClone, diceTwo))
                {
                    expansion.Add(new Ply(firstMove, secondMove));
                }
            }

            return expansion.ToArray();
        }

        private Move[] GetPossibleMoves(PlayerState player, PlayerState opponent, short dice)
        {
            var moves = new List<Move>();
            foreach (var checker in FindPlayerPoints(player))
            {
                if (IsTargetOpen(opponent, checker, dice))
                {
                    moves.Add(new Move(checker, dice));
                }
            }
            return moves.ToArray();
        }

        private void ExecuteMove(PlayerState player, PlayerState opponent, Move move)
        {
            MoveCheckerOnPlayer(player, move.Checker, move.Pips);
            if (IsTargetBlot(opponent, move.Checker, move.Pips))
            {
                ApplyHitOnOpponent(opponent, move.Checker, move.Pips);
            }
        }

        private bool IsTargetOpen(PlayerState opponent, short source, short dice)
        {
            return GetNumOpponentCheckersOnTarget(opponent.Board, source, dice) < 2;
        }

        private bool IsTargetBlot(PlayerState opponent, short source, short dice)
        {
            return GetNumOpponentCheckersOnTarget(opponent.Board, source, dice) == 1;
        }

        private static void ApplyHitOnOpponent(PlayerState opponent, short source, short dice)
        {
            opponent.Board[23 - (source - dice)]--; // remove opponent checker from his board
            opponent.Bar++; // and put it on his bar
        }

        /// <summary>
        /// Returns a set of all possible successor states.
        /// </summary>
        /// <param name="diceOne">First dice value.</param>
        /// <param name="diceTwo">Second dice value.</param>
        public IEnumerable<Backgammon> Expand(DiceRoll roll)
        {
            PlayerState player = maxToMove ? MaxPlayer : MinPlayer,
                   opponent = maxToMove ? MinPlayer : MaxPlayer;

            // Holds all our successors
            var possibleMoves = GetPossiblePlies(roll);
            var successors = new List<Backgammon>(possibleMoves.Length);
            foreach (var move in possibleMoves)
            {
                var bckg = new Backgammon(player.Clone(), opponent.Clone(), true, null);
                bckg.ExecutePly(move); // apply move for current player
                successors.Add(bckg);
            }

            return successors;
        }

        /// <summary>
        /// Moves a checker forward by the specified distance (pips).
        /// </summary>
        /// <param name="player">Array holding the checkers.</param>
        /// <param name="checkerIndex">Index of checker to move.</param>
        /// <param name="pips">Difference in pips between source and target point.</param>
        /// <returns>A new checkers array with the move applied.</returns>
        private static void MoveCheckerOnPlayer(PlayerState player, short source, short dice)
        {
            if (source == 24)
            {
                player.Bar--;
            }
            else
            {
                player.Board[source]--; // remove checker from source point
            }

            if (source - dice > 0) // check for bear-off
            {
                player.Board[source - dice]++; // put checker on new point (left to right!)
            }
        }

        private short[] FindPlayerPoints(PlayerState player)
        {
            var board = player.Board; // impacts perfomance
            int index = 0;
            short[] occupied = new short[15];
            for (short i = 0; i < board.Length; i++)
            {
                if (board[i] > 0)
                {
                    occupied[index++] = i;
                }
            }
            if (player.Bar > 0)
            {
                occupied[index++] = 24;
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

