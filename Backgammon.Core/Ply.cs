using System;
using System.Collections.Generic;
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
        private readonly SortedList<short, Move> moves = new SortedList<short, Move>(2);

        public Ply() { }

        public Ply(Move a, Move b)
        {
            moves.Add(a.Pips, a);
            moves.Add(b.Pips, b);
        }

        public IEnumerable<Move> GetMoves()
        {
            return new List<Move>(moves.Values);
        }

        public void AddMove(Move move)
        {
            moves.Add(move.Pips, move);
        }

        public void AddMove(short playerIndex, short pips)
        {
            AddMove(new Move(playerIndex, pips));
        }

        public bool Same(Ply other)
        {
            if (other == null || other.moves.Count != moves.Count)
            {
                return false;
            }

            for (short i = 0; i < moves.Count; i++)
            {
                if (!moves.Values[i].Equals(other.moves.Values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (!moves.Any())
            {
                return 0;
            }
            if (moves.Count == 1)
            {
                return moves.Values[0].GetHashCode();
            }
            else if (moves.Count == 2)
            {
                return moves.Values[0].GetHashCode() ^ moves.Values[1].GetHashCode();
            }
            else
            {
                throw new NotImplementedException("GetHashCode");
            }
        }

        public override string ToString()
        {
            return string.Join("; ", moves.Select(m => m.ToString()));
        }
    }
}

