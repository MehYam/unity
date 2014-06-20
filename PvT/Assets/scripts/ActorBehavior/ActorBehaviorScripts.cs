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

    void ChaseAttackFlee()
    {
        var retval = new SequencedBehavior();
        retval.Add(ActorBehaviorFactory.Instance.followPlayer, new RateLimiter(5));
    }
    readonly Dictionary<string, Func<IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<IActorBehavior>>();
    ActorBehaviorScripts()
    {
        //var game = Main.Instance.game;

        _behaviorFactory["GREENK_BEHAVIOR"] = () =>
        {
            return ActorBehaviorFactory.Instance.followPlayer;
        };
        _behaviorFactory["MOTH_BEHAVIOR"] = () =>
        {
            var retval = new SequencedBehavior();
            retval.Add(ActorBehaviorFactory.Instance.followPlayer, new RateLimiter(3, 2));
            retval.Add(
                new CompositeBehavior(ActorBehaviorFactory.Instance.CreateAutofire(new RateLimiter(1), Consts.Layer.MOB_AMMO),
                                      ActorBehaviorFactory.Instance.facePlayer),
                new RateLimiter(2,2)
            );
            retval.Add(ActorBehaviorFactory.Instance.CreatePatrol(new RateLimiter(2)), new RateLimiter(3,5));
            return retval;
        };
        _behaviorFactory["OSPREY"] = () =>
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
