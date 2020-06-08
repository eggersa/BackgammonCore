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
        
        public short[] Board { get; private set; } 

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

        /// <summary>
        /// Evaluates the players current position.
        /// </summary>
        /// <returns>Returns a value that determines the players current state.
        /// A lower value is better. A value of 0 means the player has won.
        /// </returns>
        public double Evaluate()
        {
            return Evaluate(0.5, 1, 1.5);
        }

        /// <summary>
        /// Evaluates the players current position.
        /// </summary>
        /// <param name="wCheckers">Weight for remaining checkers.</param>
        /// <param name="wPips">Weight for remaining pips.</param>
        /// <param name="wBar">Weight for checkers on bar.</param>
        /// <returns>Returns a value that determines the players current state.
        /// A lower value is better. A value of 0 means the player has won.
        /// </returns>
        public double Evaluate(double wCheckers, double wPips, double wBar)
        {
            return wCheckers * GetRemainingCheckers() + wPips * GetRemainingPips() + wBar * Bar; 
        }
    }
}
