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
}
