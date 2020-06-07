using System.Collections.Generic;

namespace BackgammonCore
{
    public class MoveGroup
    {
        private List<Move> moves = new List<Move>();

        public IEnumerable<Move> GetMoves()
        {
            return new List<Move>(moves);
        }

        public void AddMove(Move move)
        {
            moves.Add(move);
        }

        public void AddMove(short playerIndex, short pips)
        {
            moves.Add(new Move(playerIndex, pips));
        }
    }
}

