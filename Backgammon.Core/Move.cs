namespace Backgammon.Game
{
    public class Move
    {
        public short PlayerIndex { get; private set; }

        public short Pips { get; private set; }

        public Move(short playerIndex, short pips)
        {
            PlayerIndex = playerIndex;
            Pips = pips;
        }

        public override int GetHashCode()
        {
            int hCode = Pips ^ PlayerIndex;
            return hCode.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            if(obj is Move otherMove)
            {
                return Pips == otherMove.Pips && PlayerIndex == otherMove.PlayerIndex;
            }

            return false;
        }

        public override string ToString()
        {
            return $"Move checker from point {PlayerIndex + 1} to point {PlayerIndex - Pips + 1}.";
        }
    }
}

