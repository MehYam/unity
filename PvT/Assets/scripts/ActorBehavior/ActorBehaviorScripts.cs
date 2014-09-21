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
    public IActorBehavior Get(VehicleType vehicle)
    {
        Func<VehicleType, IActorBehavior> retval = null;
        if (!_behaviorFactory.TryGetValue(vehicle.name, out retval))
        {
            Debug.LogWarning(string.Format("no AI found for {0}, substituting a default one", vehicle.name));
            return AttackAndFlee(3, 2, 2, vehicle.weapons);
        }
        return retval != null ? retval(vehicle) : null;
    }
    static IActorBehavior AttackAndFlee(float followTime, float attackTime, float roamTime, WorldObjectType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new SequencedBehavior();
        retval.Add(bf.followPlayer, new RateLimiter(followTime, followTime / 2));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.Layer.MOB_AMMO, weapons),
                bf.facePlayer),
            new RateLimiter(attackTime, attackTime)
        );
        retval.Add(bf.CreateRoam(Consts.MAX_MOB_ROTATION_DEG_PER_SEC, false), new RateLimiter(roamTime, roamTime));
        return retval;
    }
    readonly Dictionary<string, Func<VehicleType, IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<VehicleType, IActorBehavior>>();
    ActorBehaviorScripts()
    {
        var bf = ActorBehaviorFactory.Instance;

        _behaviorFactory["GREENK"] = (vehicle) =>
        {
            return bf.followPlayer;
        };
        _behaviorFactory["MOTH"] = (vehicle) =>
        {
            return AttackAndFlee(3, 2, 2, vehicle.weapons);
        };
        _behaviorFactory["OSPREY"] = (vehicle) =>
        {
            var retval = new SequencedBehavior();
            retval.Add(bf.followPlayer, new RateLimiter(2, 2));
            retval.Add(bf.CreateTurret(Consts.Layer.MOB_AMMO, vehicle.weapons), new RateLimiter(2, 1));
            return retval;
        };
        _behaviorFactory["BEE"] = (vehicle) =>
        {
            return AttackAndFlee(3, 1, 3, vehicle.weapons);
        };
        _behaviorFactory["BAT"] = (vehicle) =>
        {
            return AttackAndFlee(3, 1, 3, vehicle.weapons);
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
