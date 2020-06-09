using System.Linq;

namespace Backgammon.Game
{
    public class Player
    {
        public Player()
        {
            Board = new short[24];
            Board[23] = 2;
            Board[12] = 5;
            Board[7] = 3;
            Board[5] = 5;
        }

        public Player(short[] board)
        {
            Board = board;
        }

        public short[] Board { get; set; }

        public string Name { get; set; }

        public short Bar { get; set; }

        /// <summary>
        /// Returns true if a checker is on the board; otherwise false.
        /// </summary>
        /// <returns></returns>
        public bool HasCheckersOnBar()
        {
            return Bar > 0;
        }

        /// <summary>
        /// Accumulates each checker multiplied by its position (includes bar).
        /// </summary>
        /// <returns>Total number of remaining pips.</returns>
        public int GetRemainingPips()
        {
            int pips = Bar * 25;
            for (int i = 0; i < Board.Count(); i++)
            {
                pips += (i + 1) * Board[i];
            }
            return pips;
        }

        /// <summary>
        /// Determines the total amount of checkers remaining on the board (includes bar).
        /// </summary>
        /// <returns>The total amount of checkers on the board.</returns>
        public int GetRemainingCheckers()
        {
            int sum = 0;
            for (int i = 0; i < Board.Count(); i++)
            {
                sum += Board[i];
            }
            return sum + Bar;
        }

        /// <summary>
        /// Checks if the player is finished.
        /// </summary>
        /// <returns>True if the player has no checkers left; otherwise false.</returns>
        public bool IsFinished()
        {
            return GetRemainingCheckers() + Bar == 0;
        }

        public Player Clone()
        {
            return new Player()
            {
                Bar = Bar,
                Board = ArrayHelper.FastArrayCopy(Board)
            };
        }

        public override string ToString()
        {
            return string.Join(" ", Board) + " | " + Bar;
        }
    }
}
