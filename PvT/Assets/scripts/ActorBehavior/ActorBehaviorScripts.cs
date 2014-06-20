using UnityEngine;
using System;
using System.Collections.Generic;

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
        retval.Add(bf.CreateRoam(new RateLimiter(roamTime / 2)), new RateLimiter(roamTime, roamTime));
        return retval;
    }
    readonly Dictionary<string, Func<IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<IActorBehavior>>();
    ActorBehaviorScripts()
    {
        //var game = Main.Instance.game;
        var bf = ActorBehaviorFactory.Instance;

        _behaviorFactory["GREENK_BEHAVIOR"] = () =>
        {
            return bf.followPlayer;
        };
        _behaviorFactory["MOTH_BEHAVIOR"] = () =>
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
        _behaviorFactory["TANK"] = () =>
        {
            return null;
        };

        // moth:
        // - chase
        // - aim and shoot
        // - roam
        //_behaviors["MOTH_BEHAVIOR"] = ChaseAttackFlee();
    }
}

sealed class TankPatrolBehavior : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
    }
}
