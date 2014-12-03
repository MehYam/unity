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

        if (mob.actorType is TankHullType)
        {
            AttachTankAI(mob);
        }
        if (mob.behavior == null && !hasCustomMonoBehavior)
        {
            Debug.LogWarning(string.Format("no AI found for {0}, substituting a default one", mob.actorType.name));
            mob.behavior = AttackAndFlee(3, 2, 2, mob.actorType.weapons);
        }
    }
    void AttachTankAI(Actor tank)
    {
        var tankHelper = new TankSpawnHelper(tank.gameObject);

        var bf = ActorBehaviorFactory.Instance;
        var hullBehavior = new CompositeBehavior(
            bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight),
            bf.CreateRoam(true),
            bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, tankHelper.hull.weapons)
        );

        var turretFireBehavior = new TimedSequenceBehavior();
        turretFireBehavior.Add(bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, tankHelper.turret.weapons), new Rate(3, 3));
        turretFireBehavior.Add(ActorBehaviorFactory.NULL, new Rate(3, 0.75f));

        var turretBehavior = new CompositeBehavior();
        turretBehavior.Add(bf.facePlayer);
        turretBehavior.Add(turretFireBehavior);

        var hullActor = tankHelper.hullGO.GetComponent<Actor>();
        var turretActor = tankHelper.turretGO.GetComponent<Actor>();
        hullActor.behavior = hullBehavior;
        turretActor.behavior = turretBehavior;

        hullActor.maxRotationalVelocity = Consts.MAX_MOB_HULL_ROTATION_DEG_PER_SEC;
        turretActor.maxRotationalVelocity = Consts.MAX_MOB_TURRET_ROTATION_DEG_PER_SEC;
    }
    IActorBehavior Get(ActorType vehicle)
    {
        Func<ActorType, IActorBehavior> retval = null;
        _behaviorFactory.TryGetValue(vehicle.name, out retval);
        return retval == null ? null : new FaceplantMitigation(retval(vehicle));
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
            case "BOSS1":
                actor.gameObject.AddComponent<Boss1AI>();
                return true;
        }
        return false;
    }
    static IActorBehavior AttackAndFlee(float followTime, float attackTime, float roamTime, ActorType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();
        retval.Add(bf.followPlayer, new Rate(followTime, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.facePlayer),
            new Rate(attackTime, 1)
        );
        retval.Add(bf.CreateRoam(false), new Rate(roamTime, 1));
        return retval;
    }
    static IActorBehavior TheCount(ActorType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new Rate(6, 0.5f));
        retval.Add(ActorBehaviorFactory.NULL, new Rate(2, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.thrustAway),
            new Rate(3, 0.5f));
        return new CompositeBehavior(bf.facePlayer, retval);
    }
    public IActorBehavior ChargeWeaponAI(ActorType.Weapon weapon)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();
        var charge = new ChargeWeaponController(Consts.CollisionLayer.MOB_AMMO, weapon);

        // follow
        retval.Add(bf.followPlayer, new Rate(4, 0.5f));

        // stop and charge
        retval.Add(new CompositeBehavior(bf.facePlayer, new GoHomeYouAreDrunkBehavior(), (Action<Actor>)charge.Charge), 
            new Rate(weapon.attrs.chargeSeconds, 0.75f));

        // discarge
        retval.Add(charge.Discharge, new Rate(1));

        return retval;
    }
    static IActorBehavior ShieldWeaponAI(ActorType vehicle)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new CompositeBehavior(bf.followPlayer);
        var weapon = vehicle.weapons[0];

        var shield = new ShieldWeaponController(Consts.CollisionLayer.MOB, weapon);
        var sequence = new TimedSequenceBehavior();

        sequence.Add((IActorBehavior)null, new Rate(5, 0.8f));
        sequence.Add(shield.Start, new Rate(0.25f));
        sequence.Add(shield.OnFrame, new Rate(3, 0.5f));
        sequence.Add(shield.Discharge, new Rate(1));

        retval.Add(sequence);
        return retval;
    }
    static IActorBehavior Buzz()
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new Rate(1, 0.5f));
        retval.Add(bf.followPlayer, new Rate(3, 0.5f));

        return retval;
    }
    /// <summary>
    /// AI registry
    /// </summary>
    readonly Dictionary<string, Func<ActorType, IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<ActorType, IActorBehavior>>();
    MobAI()
    {
        var bf = ActorBehaviorFactory.Instance;

        _behaviorFactory["GREENK0"] = (vehicle) =>
        {
            return Util.CoinFlip() ? bf.followPlayer : AttackAndFlee(4, 1, 1, vehicle.weapons);
        };
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
            return ChargeWeaponAI(vehicle.weapons[0]);
        };
        _behaviorFactory["MOTH"] = (vehicle) =>
        {
            return AttackAndFlee(3, 2, 2, vehicle.weapons);
        };
        _behaviorFactory["OSPREY"] = (vehicle) =>
        {
            var retval = new TimedSequenceBehavior();
            retval.Add(bf.followPlayer, new Rate(2, 0.75f));
            retval.Add(bf.CreateTurret(Consts.CollisionLayer.MOB_AMMO, vehicle.weapons), new Rate(2, 0.5f));
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
                new Rate(2, 0.25f)
            );
            sequence.Add(bf.facePlayer, new Rate(2));
            return sequence;
        };
    }

    sealed class FaceplantMitigation : IActorBehavior
    {
        readonly IActorBehavior _containedBehavior;
        public FaceplantMitigation(IActorBehavior behavior)
        {
            _containedBehavior = behavior;
        }
        public void FixedUpdate(Actor actor)
        {
            if ((Time.fixedTime - actor.lastFaceplantTime) < Consts.FACEPLANT_MITIGATION_DURATION)
            {
                ActorBehaviorFactory.Instance.thrustAway.FixedUpdate(actor);
            }
            else
            {
                _containedBehavior.FixedUpdate(actor);
            }
        }
    }
}

