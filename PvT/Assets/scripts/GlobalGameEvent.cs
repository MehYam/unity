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

    // High-level scene state changes
    public event Action MainReady = delegate { };
    public event Action<GameObject, XRect> MapReady = delegate { };
    public event Action<IGame> GameReady = delegate { };
    public event Action<MonoBehaviour> IntroOver = delegate { };
    public event Action<MonoBehaviour> TutorialOver = delegate { };
    public event Action GameOver = delegate { };

    // In-game events
    public event Action<Actor> ActorSpawned = delegate { };
    public event Action<Actor> ActorDeath = delegate { };
    public event Action<Actor> PlayerSpawned = delegate { };
    public event Action<Actor, ActorType.Weapon> AmmoSpawned = delegate { };
    public event Action<GameObject> ExplosionSpawned = delegate { };
    public event Action<Actor> MobSpawned = delegate { };
    public event Action<Actor> MobDeath = delegate { };
    public event Action<Actor, float> HealthChange = delegate { };
    public event Action HerolingLaunched = delegate { };
    public event Action<Actor> HerolingAttached = delegate { };
    public event Action<Actor> HerolingDetached = delegate { };
    public event Action<Actor> CollisionWithOverwhelmed = delegate { };
    public event Action<Actor> PossessionInitiated = delegate { };
    public event Action<Actor> PossessionComplete = delegate { };
    public event Action DepossessionComplete = delegate { };

    public event Action<Actor, float> WeaponCharge = delegate { };

    // UI and misc
    public event Action TouchInputBegin = delegate { };
    public event Action TouchInputEnd = delegate { };
    public event Action<PlayerData> PlayerDataUpdated = delegate { };

    // C# events require binding directly to the event source.  GlobalGameEvent works around
    // this, but the cost is that for each event we have to implement its counterpart
    // Fire**** method.
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }
    public void FireActorSpawned(Actor a) { ActorSpawned(a); }
    public void FireAmmoSpawned(Actor a, ActorType.Weapon w) { AmmoSpawned(a, w); }
    public void FireCollisionWithOverwhelmed(Actor host) { CollisionWithOverwhelmed(host); }
    public void FireDepossessionComplete() { DepossessionComplete(); }
    public void FireExplosionSpawned(GameObject a) { ExplosionSpawned(a); }
    public void FireGameOver() { GameOver(); }
    public void FireGameReady(IGame game) { GameReady(game); }
    public void FireHealthChange(Actor actor, float delta) { HealthChange(actor, delta); }
    public void FireHerolingAttached(Actor host) { HerolingAttached(host); }
    public void FireHerolingDetached(Actor host) { HerolingDetached(host); }
    public void FireHerolingLaunched() { HerolingLaunched(); }
    public void FireIntroOver(MonoBehaviour script) { IntroOver(script); }
    public void FireMainReady() { MainReady(); }
    public void FireMapReady(GameObject map, XRect bounds) { MapReady(map, bounds); }
    public void FireMobDeath(Actor a) { MobDeath(a); }
    public void FireMobSpawned(Actor a) { MobSpawned(a); }
    public void FirePlayerDataUpdated(PlayerData pd) { PlayerDataUpdated(pd); }
    public void FirePlayerSpawned(Actor player) { PlayerSpawned(player); }
    public void FirePossessionComplete(Actor host) { PossessionComplete(host); }
    public void FirePossessionInitiated(Actor host) { PossessionInitiated(host); }
    public void FireTouchInputBegin() { TouchInputBegin(); }
    public void FireTouchInputEnd() { TouchInputEnd(); }
    public void FireTutorialOver(MonoBehaviour script) { TutorialOver(script); }
    public void FireWeaponCharge(Actor actor, float pct) { WeaponCharge(actor, pct); }
}
