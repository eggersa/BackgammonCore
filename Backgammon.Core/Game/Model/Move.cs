namespace Backgammon.Game
{
    /// <summary>
    /// Specified a move of one checker from a source point to a target point.
    /// </summary>
    public class Move
    {
        /// <summary>
        /// Gets the source point from where to move a checkers.
        /// </summary>
        public short Source { get; private set; }

        /// <summary>
        /// Gets the number of pips to move the checker in the
        /// direction of the home board.
        /// The other direction is not allowed.
        /// </summary>
        public short Dice { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Move"/> class.
        /// </summary>
        /// <param name="source">The source point.</param>
        /// <param name="dice">The dice value specifying the target point.</param>
        public Move(short source, short dice)
        {
            Source = source;
            Dice = dice;
        }

        public override int GetHashCode()
        {
            int hCode = Dice ^ Source;
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
                return Dice == otherMove.Dice && Source == otherMove.Source;
            }

            return false;
        }

        public override string ToString()
        {
            if(Source - Dice < 0)
            {
                return $"Bear off {Source + 1}";
            }
            return $"From {Source + 1} to {Source - Dice + 1}";
        }
    }
}

