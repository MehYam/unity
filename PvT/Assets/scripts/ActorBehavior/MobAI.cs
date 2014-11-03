using UnityEngine;
using System;
using System.Collections.Generic;

using PvT.DOM;
using PvT.Util;

/// <summary>
/// This is used to map AI from the vehicles.json config file to actual C# objects.  There's also
/// AI coded directly here that's too complex to be defined in the configs.
/// </summary>
public sealed class MobAI
{
    static MobAI _instance;
    static public MobAI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MobAI();
            }
            return _instance;
        }
    }
    public void AttachAI(Actor mob)
    {
        mob.behavior = Get(mob.actorType);

        var hasCustomMonoBehavior = AttachMonoBehavior(mob);
        if (mob.behavior == null && !hasCustomMonoBehavior)
        {
            Debug.LogWarning(string.Format("no AI found for {0}, substituting a default one", mob.actorType.name));
            mob.behavior = AttackAndFlee(3, 2, 2, mob.actorType.weapons);
        }
    }
    IActorBehavior Get(ActorType vehicle)
    {
        Func<ActorType, IActorBehavior> retval = null;
        _behaviorFactory.TryGetValue(vehicle.name, out retval);
        return retval == null ? null : retval(vehicle);
    }
    bool AttachMonoBehavior(Actor actor)
    {
        var vehicleName = actor.actorType.name;
        switch (vehicleName) {
            case "FLY":
            case "FLY2":
            case "FLY3":
                actor.gameObject.AddComponent<DeathHopperAI>();

                var spawner = actor.gameObject.AddComponent<DeathSpawnerAI>();
                spawner.toSpawnOnDeath = vehicleName + "_FRY";
                spawner.count = UnityEngine.Random.Range(3, 6);
                return true;
        }
        return false;
    }
    static IActorBehavior AttackAndFlee(float followTime, float attackTime, float roamTime, ActorType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();
        retval.Add(bf.followPlayer, new RateLimiter(followTime, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.facePlayer),
            new RateLimiter(attackTime, 1)
        );
        retval.Add(bf.CreateRoam(Consts.MAX_MOB_ROTATION_DEG_PER_SEC, false), new RateLimiter(roamTime, 1));
        return retval;
    }
    static IActorBehavior TheCount(ActorType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new RateLimiter(6, 0.5f));
        retval.Add(ActorBehaviorFactory.NULL, new RateLimiter(2, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.thrustAway),
            new RateLimiter(3, 0.5f));
        return new CompositeBehavior(bf.facePlayer, retval);
    }
    static IActorBehavior ChargeWeaponAI(ActorType vehicle)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();
        var weapon = vehicle.weapons[0];
        var charge = new ChargeWeaponController(Consts.CollisionLayer.MOB_AMMO, weapon);

        // follow
        retval.Add(bf.followPlayer, new RateLimiter(4, 0.5f));

        // stop and charge
        retval.Add(new CompositeBehavior(bf.facePlayer, new GoHomeYouAreDrunkBehavior(), (Action<Actor>)charge.Charge), 
            new RateLimiter(weapon.chargeSeconds, 0.75f));

        // discarge
        retval.Add(charge.Discharge, new RateLimiter(1));

        return retval;
    }
    static IActorBehavior ShieldWeaponAI(ActorType vehicle)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new CompositeBehavior(bf.followPlayer);
        var weapon = vehicle.weapons[0];

        var shield = new ShieldWeaponController(Consts.CollisionLayer.MOB_AMMO, weapon);
        var sequence = new TimedSequenceBehavior();

        sequence.Add((IActorBehavior)null, new RateLimiter(5, 0.8f));
        sequence.Add(shield.Start, new RateLimiter(0.25f));
        sequence.Add(shield.OnFrame, new RateLimiter(3, 0.5f));
        sequence.Add(shield.Discharge, new RateLimiter(1));

        retval.Add(sequence);
        return retval;
    }
    static IActorBehavior Buzz()
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new RateLimiter(1, 0.5f));
        retval.Add(bf.followPlayer, new RateLimiter(3, 0.5f));

        return retval;
    }
    /// <summary>
    /// AI registry
    /// </summary>
    readonly Dictionary<string, Func<ActorType, IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<ActorType, IActorBehavior>>();
    MobAI()
    {
        var bf = ActorBehaviorFactory.Instance;

        _behaviorFactory["GREENK"] = (vehicle) =>
        {
            return Util.CoinFlip(0.2f) ? ShieldWeaponAI(vehicle) : bf.followPlayer;
        };
        _behaviorFactory["GREENK2"] = (vehicle) =>
        {
            return ShieldWeaponAI(vehicle);
        };
        _behaviorFactory["BLUEK"] = (vehicle) =>
        {
            return ChargeWeaponAI(vehicle);
        };
        _behaviorFactory["MOTH"] = (vehicle) =>
        {
            return AttackAndFlee(3, 2, 2, vehicle.weapons);
        };
        _behaviorFactory["OSPREY"] = (vehicle) =>
        {
            var retval = new TimedSequenceBehavior();
            retval.Add(bf.followPlayer, new RateLimiter(2, 0.75f));
            retval.Add(bf.CreateTurret(Consts.CollisionLayer.MOB_AMMO, vehicle.weapons), new RateLimiter(2, 0.5f));
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
        _behaviorFactory["ESOX"] = _behaviorFactory["PIKE"] = _behaviorFactory["PIKE0"] = (vehicle) =>
        {
            return TheCount(vehicle.weapons);
        };
        _behaviorFactory["FLY_FRY"] = _behaviorFactory["FLY_FRY2"] = _behaviorFactory["FLY_FRY3"] = (vehicle) =>
        {
            return Buzz();
        };
        _behaviorFactory["LASERTURRET1"] = _behaviorFactory["LASERTURRET2"] = (vehicle) =>
        {
            var sequence = new TimedSequenceBehavior();
            sequence.Add(new CompositeBehavior( 
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, vehicle.weapons),
                bf.facePlayer
                ),
                new RateLimiter(2, 0.25f)
            );
            sequence.Add(bf.facePlayer, new RateLimiter(2));
            return sequence;
        };
    }
}

