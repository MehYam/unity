using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This is used to map AI from the vehicles.json config file to actual C# objects.  There's also
/// AI coded directly here that's too complex to be defined in the configs.
/// </summary>
public sealed class EnemyActorBehaviors
{
    static EnemyActorBehaviors _instance;
    static public EnemyActorBehaviors Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EnemyActorBehaviors();
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
        retval.AddBehavior(ActorBehaviorFactory.Instance.followPlayer, new RateLimiter(5));
    }
    readonly Dictionary<string, Func<IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<IActorBehavior>>();
    EnemyActorBehaviors()
    {
        var game = Main.Instance.gameState;

        _behaviorFactory["GREENK_BEHAVIOR"] = () =>
        {
            return ActorBehaviorFactory.Instance.followPlayer;
        };
        _behaviorFactory["MOTH_BEHAVIOR"] = () =>
        {
            return ActorBehaviorFactory.Instance.CreateAutofire(new RateLimiter(1));
        };

        // moth:
        // - chase
        // - aim and shoot
        // - roam
        //_behaviors["MOTH_BEHAVIOR"] = ChaseAttackFlee();
    }
}
