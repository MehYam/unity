using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

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

    public event Action<TileMap, XRect> MapReady = delegate { };
    public void FireMapReady(TileMap map, XRect bounds) { MapReady(map, bounds); }

    public event Action<GameObject> PlayerSpawned = delegate { };
    public void FirePlayerSpawned(GameObject player) { PlayerSpawned(player); }
}
