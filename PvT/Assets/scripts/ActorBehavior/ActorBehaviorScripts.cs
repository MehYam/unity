using UnityEngine;
using System;
using System.Collections.Generic;

using PvT.DOM;

/// <summary>
/// This is used to map AI from the vehicles.json config file to actual C# objects.  There's also
/// AI coded directly here that's too complex to be defined in the configs.
/// </summary>
public sealed class ActorBehaviorScripts
{
    static ActorBehaviorScripts _instance;
    static public ActorBehaviorScripts Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ActorBehaviorScripts();
            }
            return _instance;
        }
    }
    public IActorBehavior Get(string key)
    {
        Func<IActorBehavior> retval = null;
        _behaviorFactory.TryGetValue(key, out retval);
        return retval != null ? retval() : null;
    }

    IActorBehavior AttackAndFlee(float followTime, float attackTime, float roamTime)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new SequencedBehavior();
        retval.Add(bf.followPlayer, new RateLimiter(followTime, followTime / 2));
        retval.Add(
            new CompositeBehavior(bf.CreateAutofire(new RateLimiter(1), Consts.Layer.MOB_AMMO),
                                  bf.facePlayer),
            new RateLimiter(attackTime, attackTime)
        );
        retval.Add(bf.CreateRoam(Consts.MAX_MOB_ROTATION_DEG_PER_SEC, false), new RateLimiter(roamTime, roamTime));
        return retval;
    }
    readonly Dictionary<string, Func<IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<IActorBehavior>>();
    ActorBehaviorScripts()
    {
        //var game = Main.Instance.game;
        var bf = ActorBehaviorFactory.Instance;

        _behaviorFactory["GREENK"] = () =>
        {
            return bf.followPlayer;
        };
        _behaviorFactory["MOTH"] = () =>
        {
            return AttackAndFlee(3, 2, 2);
        };
        _behaviorFactory["OSPREY"] = () =>
        {
            var retval = new SequencedBehavior();
            retval.Add(bf.followPlayer, new RateLimiter(2, 2));
            retval.Add(bf.CreateTurret(new RateLimiter(1), Consts.Layer.MOB_AMMO), new RateLimiter(2, 1));
            return retval;
        };
        _behaviorFactory["BEE"] = () =>
        {
            return AttackAndFlee(3, 1, 3);
        };
        _behaviorFactory["BAT"] = () =>
        {
            return null;
        };
    }
}

/// <summary>
/// Given a tank Actor, this wraps a behavior, and applies it to the turret instead of the tank
/// </summary>
sealed class TurretBehavior : IActorBehavior
{
    public TurretBehavior()
    {
    }

    public void FixedUpdate(Actor actor)
    {
        throw new NotImplementedException();
    }
}
