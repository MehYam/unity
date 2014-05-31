using UnityEngine;
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
        IActorBehavior retval = null;
        _behaviors.TryGetValue(key, out retval);
        return retval;
    }

    void ChaseAttackFlee(Ammo[] ammo)
    {
        var retval = new SequencedBehavior();
        retval.AddBehavior(ActorBehaviorFactory.Instance.followPlayer, new RateLimiter(5));
    }
    readonly Dictionary<string, IActorBehavior> _behaviors = new Dictionary<string, IActorBehavior>();
    EnemyActorBehaviors()
    {
        var game = Main.Instance.gameState;

        _behaviors["GREENK_BEHAVIOR"] = ActorBehaviorFactory.Instance.followPlayer;

        _behaviors["MOTH_BEHAVIOR"] = ActorBehaviorFactory.Instance.CreateAutofire(
            new RateLimiter(2),
            new Ammo(game.GetVehicle("BULLET"), 0),
            new Ammo(game.GetVehicle("CANNONROUND0"), 1)
        );

        // moth:
        // - chase
        // - aim and shoot
        // - roam
        //_behaviors["MOTH_BEHAVIOR"] = ChaseAttackFlee();
    }
}
