using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HST.Game
{
    // this is a mediator that fires and syncs events between different components.  i.e.
    // something might need to know that a card was played, but to reduce coupling between
    // things that manage cards (Deck, Hero, Hands, etc) and the receiver of the event, they
    // can use GlobalGameEvent as a message passing broker to get the actions across.
    class GlobalGameEvent
    {
        static GlobalGameEvent _singleton = null;
        GlobalGameEvent() { }

        static private GlobalGameEvent Instance
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new GlobalGameEvent();
                }
                return _singleton;
            }
        }

        // when a card gets played, it often requires user input to complete (i.e. place a 
        // minion, or choose a target).  This makes it an asynchronous operation, so events
        // work well to drive this multi-step action.

        // KAI: these arguments getting passed around seem kind of arbitrary and inconsistent.  i.e. Game vs. Hero vs. whatever...
        public event Action<Hero, AbstractCard> CardPlayBegun = delegate { };
        public event Action CardPlayEnded = delegate { };
        public event Action FriendlyMinionNeeded = delegate { };
        public event Action EnemyMinionNeeded = delegate { };

        public event Action<Game, Minion> MinionDeath = delegate { };
        public event Action<Game> NewTurn = delegate { };

        public void FireCardPlayStarted(Hero h, AbstractCard c) { CardPlayBegun(h, c); }
        public void FireCardPlayCompleted() { CardPlayEnded(); }
        public void FireMinionDeath(Game g, Minion m) { MinionDeath(g, m); }
        public void FireNewTurn(Game g) { NewTurn(g);  }
    }
}
