using System;
using UnityEngine;

/// <summary>
/// GlobalGameEvent is a broker that allows events to be attached to and fired from disparate
/// components.  It allows these events to be subscribed to anonymously, and makes it easy
/// to clean up all held references with the ReleaseAllListeners() call.
/// </summary>
class GlobalEvent
{
    static GlobalEvent _singleton = null;
    GlobalEvent() { }

    static public GlobalEvent Instance
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = new GlobalEvent();
            }
            return _singleton;
        }
    }
    static public void ReleaseAllListeners()
    {
        _singleton = null;
    }

    /// <summary>
    /// Events
    /// </summary>
    public event Action MainReady = delegate { };
    public void FireMainReady() { MainReady(); }

    public event Action<Actor> ActorSpawned = delegate { };
    public void FireActorSpawned(Actor a) { ActorSpawned(a); }

    public event Action<Actor> ActorDeath = delegate { };
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }

    public event Action<string> DebugString = delegate { };
    public void FireDebugString(string text) { DebugString(text); }

    //public event Action<GameObject, XRect> MapReady = delegate { };
    //public event Action<IGame> GameReady = delegate { };
    //public event Action<MonoBehaviour> IntroOver = delegate { };
    //public event Action<MonoBehaviour> TutorialOver = delegate { };
    //public event Action GameOver = delegate { };
    //public event Action<Actor> PlayerSpawned = delegate { };
    //public event Action<Actor, ActorType.Weapon> AmmoSpawned = delegate { };
    //public event Action<GameObject> ExplosionSpawned = delegate { };
    //public event Action<Actor> MobSpawned = delegate { };
    //public event Action<Actor> MobDeath = delegate { };
    //public event Action<Actor, float> HealthChange = delegate { };
    //public event Action HerolingLaunched = delegate { };
    //public event Action<Actor> HerolingAttached = delegate { };
    //public event Action<Actor> HerolingDetached = delegate { };
    //public event Action<Actor> CollisionWithOverwhelmed = delegate { };
    //public event Action<Actor> PossessionInitiated = delegate { };
    //public event Action<Actor> PossessionComplete = delegate { };
    //public event Action DepossessionComplete = delegate { };

    //public event Action<Actor, float> WeaponCharge = delegate { };

    // UI and misc
    //public event Action TouchInputBegin = delegate { };
    //public event Action TouchInputEnd = delegate { };
    //public event Action<PlayerData> PlayerDataUpdated = delegate { };
    //public event Action<int, Vector2> GainingXP = delegate { };
    //public event Action<ActorType> TierUp = delegate { };
    //public event Action<int> LevelUp = delegate { };

    // C# events require binding directly to the event source.  GlobalGameEvent works around
    // this, but the cost is that for each event we have to implement its counterpart
    // Fire**** method.
    //public void FireAmmoSpawned(Actor a, ActorType.Weapon w) { AmmoSpawned(a, w); }
    //public void FirePlayerCollisionWithOverwhelmed(Actor host) { CollisionWithOverwhelmed(host); }
    //public void FireDepossessionComplete() { DepossessionComplete(); }
    //public void FireExplosionSpawned(GameObject a) { ExplosionSpawned(a); }
    //public void FireGainingXP(int xp, Vector2 where) { GainingXP(xp, where); }
    //public void FireGameOver() { GameOver(); }
    //public void FireGameReady(IGame game) { GameReady(game); }
    //public void FireHealthChange(Actor actor, float delta) { HealthChange(actor, delta); }
    //public void FireHerolingAttached(Actor host) { HerolingAttached(host); }
    //public void FireHerolingDetached(Actor host) { HerolingDetached(host); }
    //public void FireHerolingLaunched() { HerolingLaunched(); }
    //public void FireIntroOver(MonoBehaviour script) { IntroOver(script); }
    //public void FireLevelUp(int level) { LevelUp(level); }
    //public void FireMapReady(GameObject map, XRect bounds) { MapReady(map, bounds); }
    //public void FireMobDeath(Actor a) { MobDeath(a); }
    //public void FireMobSpawned(Actor a) { MobSpawned(a); }
    //public void FirePlayerDataUpdated(PlayerData pd) { PlayerDataUpdated(pd); }
    //public void FirePlayerSpawned(Actor player) { PlayerSpawned(player); }
    //public void FirePossessionComplete(Actor host) { PossessionComplete(host); }
    //public void FirePossessionInitiated(Actor host) { PossessionInitiated(host); }
    //public void FireTierUp(ActorType newType) { TierUp(newType); }
    //public void FireTouchInputBegin() { TouchInputBegin(); }
    //public void FireTouchInputEnd() { TouchInputEnd(); }
    //public void FireTutorialOver(MonoBehaviour script) { TutorialOver(script); }
    //public void FireWeaponCharge(Actor actor, float pct) { WeaponCharge(actor, pct); }
}
