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

        static public GlobalGameEvent Instance
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

        //KAI: minimax will complicate this... instead of GlobalGameEvent singleton, have
        // Game own it as a public member
        public event Action<Game, AbstractCard> CardPlayComplete = delegate { };

        public void FireCardPlayComplete(Game game, AbstractCard card) { CardPlayComplete(game, card); }
    }
}
