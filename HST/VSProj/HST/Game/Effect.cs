using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    public interface IEffect
    {
        void Go(Game game, Hero hero);
    }

    public class Modifier
    {
        public readonly bool oneTurn;
        public readonly int amount;

        public Modifier(bool oneTurn, int amount) { this.oneTurn = oneTurn; this.amount = amount; }
    }
}
