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
    static IActorBehavior TheCount(WorldObjectType.Weapon[] weapons)
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
    static IActorBehavior ChargeWeaponAI(VehicleType vehicle)
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
    static IActorBehavior ShieldWeaponAI(VehicleType vehicle)
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
    static IActorBehavior Hopper(VehicleType vehicle)
    {
        var bf = ActorBehaviorFactory.Instance;
        var sequence = new TimedSequenceBehavior();

        sequence.Add(bf.facePlayer, new RateLimiter(1, 0));


        return sequence;
    }

    /// <summary>
    /// AI registry
    /// </summary>
    readonly Dictionary<string, Func<VehicleType, IActorBehavior>> _behaviorFactory = new Dictionary<string, Func<VehicleType, IActorBehavior>>();
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
        _behaviorFactory["FLY"] = (vehicle) =>
        {
            return new HopBehavior();
        };
    }
}

//var angle = Util.GetLookAtAngle(actor.transform.position, Main.Instance.game.player.transform.position);

/// <summary>
/// This implements a pseudo-3D hopping.  Would it be better to use real 3D?
/// </summary>
sealed class HopBehavior : IActorBehavior
{
    static readonly float GRAVITY = 1.5f;
    static readonly float JUMP_VELOCITY = 0.75f;

    float _verticalVelocity = 0;
    float _height = 0;

    Vector3 _originalScale = Vector3.zero;
    public void FixedUpdate(Actor actor)
    {
        if (_height <= 0)
        {
            // jump!
            if (_originalScale == Vector3.zero)
            {
                _originalScale = actor.transform.localScale;
            }
            var target = Main.Instance.game.player.transform.position;

            _verticalVelocity = JUMP_VELOCITY;

            var lookAt = Util.GetLookAtVector(actor.transform.position, target);
            actor.rigidbody2D.velocity = lookAt * actor.maxSpeed;
        }

        // tween the actor
        _height += Time.deltaTime * _verticalVelocity;
        _verticalVelocity -= Time.deltaTime * GRAVITY;

        actor.transform.localScale = (1 + _height) * _originalScale;

        var dropShadow = actor.GetComponentInChildren<DropShadow>();
        if (dropShadow != null)
        {
            dropShadow.distanceModifier = _height;
        }
    }
}
