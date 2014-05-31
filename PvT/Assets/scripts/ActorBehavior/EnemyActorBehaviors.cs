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

    readonly Dictionary<string, IActorBehavior> _behaviors = new Dictionary<string, IActorBehavior>();
    EnemyActorBehaviors()
    {
        _behaviors["GREENK_BEHAVIOR"] = ActorBehaviorFactory.Instance.followPlayer;
        _behaviors["MOTH_BEHAVIOR"] = null;
    }
}
