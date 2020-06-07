namespace BackgammonCore
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
    }
}

