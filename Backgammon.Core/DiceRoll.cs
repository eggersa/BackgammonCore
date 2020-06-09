using System.Diagnostics;

namespace Backgammon.Game
{
    /// <summary>
    /// Encapuslates the values of rolling two dice.
    /// </summary>
    public class DiceRoll
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiceRoll"/> class.
        /// Only values from 1 to 6 are allowed.
        /// </summary>
        /// <param name="one">Value of the first dice.</param>
        /// <param name="one">Value of the second dice.</param>
        public DiceRoll(short one, short two)
        {
            AssertDiceValue(one);
            One = one;
            AssertDiceValue(two);
            Two = two;
        }

        /// <summary>
        /// Gets the value of the first dice;
        /// </summary>
        public short One { get; private set; }

        /// <summary>
        /// Gets the value of the second dice;
        /// </summary>
        public short Two { get; private set; }

        /// <summary>
        /// Checks if a double has been rolled e.g. both dice have the same value.
        /// </summary>
        /// <returns>Return true if a double was rolled; otherwise false.</returns>
        public bool IsDouble()
        {
            return One == Two;
        }

        public override string ToString()
        {
            return $"Roll {One} and {Two}";
        }

        [Conditional("DEBUG"), DebuggerStepThrough]
        private void AssertDiceValue(short value)
        {
            Debug.Assert(value > 0 && value < 7, "Dice value must be inside a range from 1 to 6");
        }
    }
}
