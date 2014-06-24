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
    static public void ReleaseAll()
    {
        _singleton = null;
    }

    public event Action<TileMap, XRect> MapReady = delegate { };
    public event Action<GameObject> PlayerSpawned = delegate { };
    public event Action HerolingLaunched = delegate { };
    public event Action<Actor> HerolingAttached = delegate { };
    public event Action<Actor> HerolingDetached = delegate { };
    public event Action HerolingDestroyed = delegate { };

    public event Action<Actor> PossessionContact = delegate { };
    public event Action<Actor, float> HealthChange = delegate { };
    public event Action<Actor> ActorDeath = delegate { };

    public event Action GameOver = delegate { };

    public void FireMapReady(TileMap map, XRect bounds) { MapReady(map, bounds); }
    public void FirePlayerSpawned(GameObject player) { PlayerSpawned(player); }

    public void FireHerolingLaunched() { HerolingLaunched(); }
    public void FireHerolingAttached(Actor host) { HerolingAttached(host); }
    public void FireHerolingDetached(Actor host) { HerolingDetached(host); }
    public void FireHerolingDestroyed() { HerolingDestroyed(); }

    public void FirePossessionContact(Actor host) { PossessionContact(host); }
    public void FireHealthChange(Actor actor, float delta) { HealthChange(actor, delta); }
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }

    public void FireGameOver() { GameOver(); }

    // UI-centric
    public event Action<string> CenterPrint = delegate { };

    public void FireCenterPrint(string centerPrint) { CenterPrint(centerPrint); }
}
