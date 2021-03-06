﻿using System;
using System.Linq;

namespace Backgammon.Game.Agents
{
    /// <summary>
    /// Backgammon agent that choses random moves.
    /// </summary>
    public class RandomBackgammonAgent : IBackgammonAgent
    {
        private readonly static Random rnd = new Random();

        public string Name => "Random Agent";

        public Ply NextPly(DiceRoll roll, Backgammon game)
        {
            var moves = game.GetPossiblePlies(roll);
            if (moves.Any())
            {
                return moves[rnd.Next(0, moves.Count())];
            }

            return Ply.ZeroPly;
        }
    }
}
