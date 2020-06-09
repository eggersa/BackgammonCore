namespace Backgammon.Game
{
    public class Move
    {
        public short Checker { get; private set; }

        public short Pips { get; private set; }

        public Move(short playerIndex, short pips)
        {
            Checker = playerIndex;
            Pips = pips;
        }

        public override int GetHashCode()
        {
            int hCode = Pips ^ Checker;
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
                return Pips == otherMove.Pips && Checker == otherMove.Checker;
            }

            return false;
        }

        public override string ToString()
        {
            if(Checker - Pips < 0)
            {
                return $"Bear off from point {Checker + 1}";
            }
            return $"Move checker from point {Checker + 1} to point {Checker - Pips + 1}";
        }
    }
}

