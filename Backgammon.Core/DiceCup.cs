using System;

namespace Backgammon.Game
{
    /// <summary>
    /// Helper class to roll two dice.
    /// </summary>
    public static class DiceCup
    {
        private readonly static Random rnd = new Random();

        /// <summary>
        /// Rolls two dice by means of creating two random values between 1 and 6.
        /// </summary>
        /// <returns>A <see cref="DiceRoll"/> object with the outcome.</returns>
        public static DiceRoll Roll()
        {
            return new DiceRoll((short)rnd.Next(1, 7), (short)rnd.Next(1, 7));
        }
    }
}
