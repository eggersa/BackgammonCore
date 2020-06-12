using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Backgammon.Game
{
    /// <summary>
    /// The backgammon class implements the game rules by means of evaluatiing 
    /// all possible moves for the current game state.
    /// 
    /// The object of the game is move all your checkers into your own home board and then bear them off.
    /// The first player to bear off all of their checkers wins the game.
    /// For more details <see cref="https://www.bkgm.com/rules.html"/>.
    /// </summary>
    public class Backgammon
    {
        // total number of points on the board
        private const int NumPoints = 24;

        // true if the max player is to move
        private bool maxToMove = true;

        /// <summary>
        /// Gets all possible dice combinations for a pair of dice ignoring their order.
        /// </summary>
        public static readonly DiceRoll[] DicePairs;

        /// <summary>
        /// Initializes static fields and properties of the class.
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
            MaxPlayer = maxPlayer;
            MinPlayer = minPlayer;
            this.maxToMove = maxToMove;

            LastMove = move;
        }

        /// <summary>
        /// Gets the maximizing player.
        /// </summary>
        public PlayerState MaxPlayer { get; private set; }

        /// <summary>
        /// Gets the minimxing player.
        /// </summary>
        public PlayerState MinPlayer { get; private set; }

        /// <summary>
        /// Gets the last move that lead to the current state or null of the state is initial.
        /// </summary>
        public Ply LastMove { get; private set; }

        /// <summary>
        /// Checks if the maximizing player can move.
        /// </summary>
        /// <returns>True if the maximizing player can move; otherwise false.</returns>
        public bool MaxToMove()
        {
            return maxToMove;
        }

        /// <summary>
        /// Checks if the minimizing player can move.
        /// </summary>
        /// <returns>True if the minimizing player can move; otherwise false.</returns>
        public bool MinToMove()
        {
            return !maxToMove;
        }

        /// <summary>
        /// Checks if the game is finished.
        /// </summary>
        /// <returns>True if the game is finished; otherwise false.</returns>
        public bool IsTerminal()
        {
            return MaxPlayer.IsFinished() || MinPlayer.IsFinished();
        }

        /// <summary>
        /// Checks which player can move.
        /// </summary>
        /// <returns>Returns the player that has to execute a move.</returns>
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
            return new Backgammon(new PlayerState() { Name = "Max" }, new PlayerState() { Name = "Min" }, true, null);
        }

        /// <summary>
        /// Determines all possible plies for the current state with the given dice roll.
        /// </summary>
        /// <param name="roll">The dice roll.</param>
        /// <returns>An array containing all possible plies that can be executed.</returns>
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

        /// <summary>
        /// Checks if the given ply is valid for the given dice roll.
        /// </summary>
        /// <param name="ply">The ply to check.</param>
        /// <param name="roll">The dice roll to check.</param>
        /// <returns>True if the ply is valid; otherwise false.</returns>
        public bool ValidatePly(Ply ply, DiceRoll roll)
        {
            var moves = GetPossiblePlies(roll).ToList();
            return moves.Contains(ply, PlyEqualityComparer.Instance);
        }

        /// <summary>
        /// Executes the given ply on the current game state.
        /// </summary>
        /// <param name="ply">The ply to execute.</param>
        /// <param name="rollbackOnError">If false (default), an exception is thrown if an error (invalid state) occures.</param>
        /// <returns>True if the ply has been successfully executed or false if an has error occured (invalid move) 
        /// and the state has been roll back.</returns>
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
        /// Determines all possible plies for the current state with respect to the dice order.
        /// </summary>
        /// <param name="player">The player state.</param>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="diceOne">The dice that constitues the first move.</param>
        /// <param name="diceTwo">The dice that constitutes the second move.</param>
        /// <returns>An array containg all possible plies that can be executed with the given order of dice.</returns>
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

        /// <summary>
        /// Determines all possible moves for a specific dice.
        /// </summary>
        /// <param name="player">The player state.</param>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="dice">The dice.</param>
        /// <returns>An array containing all possible moves for the specified dice.</returns>
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

        /// <summary>
        /// Executes the given move.
        /// </summary>
        /// <param name="player">The player state.</param>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="move">The move to execute.</param>
        private void ExecuteMove(PlayerState player, PlayerState opponent, Move move)
        {
            MoveCheckerOnPlayer(player, move.Checker, move.Pips);
            if (IsTargetBlot(opponent, move.Checker, move.Pips))
            {
                ApplyHitOnOpponent(opponent, move.Checker, move.Pips);
            }
        }

        /// <summary>
        /// Checks if the target point is open. A target is open if there are no more
        /// than one enemy checker on the point.
        /// </summary>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="source">The index of the source point.</param>
        /// <param name="dice">The dice specifying the target point.</param>
        /// <returns>True if the target is open; otherwise false.</returns>
        private bool IsTargetOpen(PlayerState opponent, short source, short dice)
        {
            return GetNumOpponentCheckersOnTarget(opponent.Board, source, dice) < 2;
        }

        /// <summary>
        /// Checks if the target point is a blot open. A target is a blot if there
        /// is exactily one enemy checker on the point.
        /// </summary>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="source">The index of the source point.</param>
        /// <param name="dice">The dice specifying the target point.</param>
        /// <returns>True if the target is a blot; otherwise false.</returns>
        private bool IsTargetBlot(PlayerState opponent, short source, short dice)
        {
            return GetNumOpponentCheckersOnTarget(opponent.Board, source, dice) == 1;
        }

        /// <summary>
        /// Hits the opponent on the target point by removing his checker and putting it on the board.
        /// </summary>
        /// <param name="opponent">The opponent state.</param>
        /// <param name="source">The index of the source point.</param>
        /// <param name="dice">The dice specifying the target point.</param>
        private static void ApplyHitOnOpponent(PlayerState opponent, short source, short dice)
        {
            opponent.Board[23 - (source - dice)]--; // remove opponent checker from his board
            opponent.Bar++; // and put it on his bar
        }

        /// <summary>
        /// Moves a checker forward to the specified point by updating the players board state.
        /// </summary>
        /// <param name="opponent">The player state.</param>
        /// <param name="source">The index of the source point.</param>
        /// <param name="dice">The dice specifying the target point.</param>
        /// <returns>A new checkers array with the move applied.</returns>
        private static void MoveCheckerOnPlayer(PlayerState player, short source, short dice)
        {
            if (source == 24)
            {
                player.Bar -= 1;
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

        /// <summary>
        /// Finds all points that are occupied by at least one checker of the given player.
        /// </summary>
        /// <param name="player">The player to check for.</param>
        /// <returns>An array containing all indexes to points that contain at least one checker.
        /// The index refers to the players board state.</returns>
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

        /// <summary>
        /// Gets the number of opponent checkers on the specified target.
        /// </summary>
        /// <param name="opponent">The player state.</param>
        /// <param name="source">The index of the source point.</param>
        /// <param name="dice">The dice specifying the target point.</param>
        /// <returns>An amount of checkers.</returns>
        private short GetNumOpponentCheckersOnTarget(short[] opponent, short source, short dice)
        {
            if (source - dice < 0)
            {
                return 0;
            }

            return opponent[23 - (source - dice)]; // opponent is reversed
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

        /// <summary>
        /// Builds a string that represents the board state.
        /// </summary>
        /// <returns>A string representing the board state.</returns>
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

