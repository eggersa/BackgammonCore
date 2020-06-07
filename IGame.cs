using System;
using System.Collections.Generic;
using System.Text;

namespace BackgammonCore
{
    public interface IGame
    {
        /// <summary>
        /// Returns true, if the max player is to move; false otherwise.
        /// </summary>
        bool MaxToMove();

        /// <summary>
        /// Returns true, if the min player is to move; false otherwise.
        /// </summary>
        bool MinToMove();

        /// <summary>
        /// Returns the initial state of the game.
        /// </summary>
        IGame Start();

        /// <summary>
        /// Returns the move leating to this state or null if the game state is initial.
        /// </summary>
        int GetLastMove();

        /// <summary>
        /// Do a move on the current state.
        /// </summary>
        /// <param name="move">The move to do.</param>
        void DoMove(int move);

        /// <summary>
        /// Returns a set of possible moves.
        /// </summary>
        object GetPossibleMoves();

        /// <summary>
        /// Returns a set of successor states.
        /// </summary>
        /// <returns></returns>
        IGame[] Expand();

        /// <summary>
        /// Determines if the current state is terminal.
        /// </summary>
        /// <returns>True if the current state is terminal; false otherwise.</returns>
        bool IsTerminal();

        /// <summary>
        /// Returns the evaluated value related to the max player for the current state. T
        /// he evaluation is exact for terminal nodes. For other nodes, the value is statically
        /// evaluated using a heuristic approcimation.
        /// </summary>
        /// <returns></returns>
        double utility();
    }
}
