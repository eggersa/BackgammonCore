using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Backgammon.Game
{
    /// <summary>
    /// A ply is composed from several moves. This class encapsulates all moves for a ply.
    /// The order of moves does not impact the overall result of executing the ply.
    /// </summary>
    public class Ply
    {
        // Moves are orderd to easier compare two plies.
        private readonly List<Move> moves = new List<Move>(2);
        
        public static readonly Ply ZeroPly = new Ply();

        public Ply() { }

        public Ply(Move a)
        {
            AddMove(a);
        }

        public Ply(Move a, Move b)
        {
            AddMove(a);
            AddMove(b);
        }

        /// <summary>
        /// Gets the number of bar moves. A move is a bar move if his applications
        /// results in an enemy checker being hit.
        /// </summary>
        public int CountBarMovements { get; private set; }

        public IEnumerable<Move> GetMoves()
        {
            return new List<Move>(moves);
        }

        public void AddMove(Move move)
        {
            VerifyAcces();

            if (move.Source == 24)
            {
                CountBarMovements++;
            }

            if (moves.Any() && move.Dice < moves[0].Dice)
            {
                moves.Insert(0, move);
            }
            else
            {
                moves.Add(move);
            }
        }

        public void AddMove(short playerIndex, short pips)
        {
            AddMove(new Move(playerIndex, pips));
        }

        /// <summary>
        /// Checks if two plies are the same.
        /// </summary>
        /// <param name="other">The second ply.</param>
        /// <returns>True if the current ply is the same as the second ply; otherwise false.</returns>
        public bool Same(Ply other)
        {
            if (other == null || other.moves.Count != moves.Count)
            {
                return false;
            }

            for (short i = 0; i < moves.Count; i++)
            {
                if (!moves[i].Equals(other.moves[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the current ply contains only one move.
        /// </summary>
        /// <returns>True if the current ply contains only one move</returns>
        public bool IsSingleMove()
        {
            return moves.Count == 1;
        }

        public override int GetHashCode()
        {
            if (!moves.Any())
            {
                return 0;
            }
            if (moves.Count == 1)
            {
                return moves[0].GetHashCode();
            }
            else if (moves.Count == 2)
            {
                return moves[0].GetHashCode() ^ moves[1].GetHashCode();
            }
            else
            {
                throw new NotImplementedException("GetHashCode");
            }
        }

        public override string ToString()
        {
            if(this == ZeroPly)
            {
                return "No moves";
            }

            return string.Join("; ", moves.Select(m => m.ToString()));
        }

        [Conditional("DEBUG"), DebuggerStepThrough]
        private void VerifyAcces()
        {
            if (this == ZeroPly)
            {
                throw new InvalidOperationException("You can not modify to ZeroPly");
            }
        }
    }
}

