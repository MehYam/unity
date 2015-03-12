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

        // For now, everything reacts to faceplants
        mob.behavior = new FaceplantMitigation(mob.behavior);
        mob.target = PlayerTarget.Instance;
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
        turretFireBehavior.Add(bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, tankHelper.turret.weapons), new Timer(3, 3));
        turretFireBehavior.Add(ActorBehaviorFactory.NULL, new Timer(3, 0.75f));

        var turretBehavior = new CompositeBehavior();
        turretBehavior.Add(bf.faceTarget);
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
        retval.Add(bf.followPlayer, new Timer(followTime, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.faceTarget),
            new Timer(attackTime, 1)
        );
        retval.Add(bf.CreateRoam(false), new Timer(roamTime, 1));
        return retval;
    }
    static IActorBehavior TheCount(ActorType.Weapon[] weapons)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new Timer(6, 0.5f));
        retval.Add(ActorBehaviorFactory.NULL, new Timer(2, 0.5f));
        retval.Add(
            new CompositeBehavior(
                bf.CreateAutofire(Consts.CollisionLayer.MOB_AMMO, weapons),
                bf.thrustAway),
            new Timer(3, 0.5f));
        return new CompositeBehavior(bf.faceTarget, retval);
    }
    public IActorBehavior ChargeWeaponAI(ActorType.Weapon weapon)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();
        var charge = new ChargeWeaponController(Consts.CollisionLayer.MOB_AMMO, weapon);

        // follow
        retval.Add(bf.followPlayer, new Timer(4, 0.5f));

        // stop and charge
        retval.Add(new CompositeBehavior(bf.faceTarget, new GoHomeYouAreDrunkBehavior(), (Action<Actor>)charge.OnFrame), 
            new Timer(weapon.attrs.chargeSeconds, 0.75f));

        // discarge
        retval.Add(charge.OnEnd, new Timer(1));

        return retval;
    }
    static IActorBehavior ShieldWeaponAI(ActorType vehicle)
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new CompositeBehavior(bf.followPlayer);
        var weapon = vehicle.weapons[0];

        var shield = new ShieldWeaponController(Consts.CollisionLayer.MOB, weapon);
        var sequence = new TimedSequenceBehavior();

        sequence.Add((IActorBehavior)null, new Timer(5, 0.8f));
        sequence.Add(shield.OnStart, new Timer(0.25f));
        sequence.Add(shield.OnFrame, new Timer(3, 0.5f));
        sequence.Add(shield.OnEnd, new Timer(1));

        retval.Add(sequence);
        return retval;
    }
    static IActorBehavior Buzz()
    {
        var bf = ActorBehaviorFactory.Instance;
        var retval = new TimedSequenceBehavior();

        retval.Add(bf.thrust, new Timer(1, 0.5f));
        retval.Add(bf.followPlayer, new Timer(3, 0.5f));

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
            return bf.followPlayer;
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
            retval.Add(bf.followPlayer, new Timer(2, 0.75f));
            retval.Add(bf.CreateTurret(Consts.CollisionLayer.MOB_AMMO, vehicle.weapons), new Timer(2, 0.5f));
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
                bf.faceTarget
                ),
                new Timer(2, 0.25f)
            );
            sequence.Add(bf.faceTarget, new Timer(2));
            return sequence;
        };
    }

    sealed class FaceplantMitigation : IActorBehavior
    {
        static readonly RaycastHit2D[] s_collisions = new RaycastHit2D[1];

        Timer _mitigationTime;
        Timer _preventativeFaceplantCheck = new Timer(1, 0.3f);
        readonly IActorBehavior _normalBehavior;
        public FaceplantMitigation(IActorBehavior behavior)
        {
            _normalBehavior = behavior;
            _mitigationTime.Stop();
        }

        public void FixedUpdate(Actor actor)
        {
            if ((Time.fixedTime - actor.lastFaceplantTime) < Consts.FACEPLANT_MITIGATION_DURATION)
            {
                _mitigationTime.Start(1);

                // pick a waypoint
                actor.target = Util.RandomArrayPick(Main.Instance.game.mapWaypoints);
                actor.lastFaceplantTime = float.MinValue;
            }

            if (!_mitigationTime.reached)
            {
                float maxRotation = Time.fixedDeltaTime * actor.maxRotationalVelocity;
                Util.LookAt2D(actor.transform, PlayerTarget.Instance.position, maxRotation);

                ActorBehaviorFactory.Instance.gravitateToTarget.FixedUpdate(actor);
            }
            else if (_normalBehavior != null)
            {
                actor.target = PlayerTarget.Instance;  //note: if we're setting the target every frame anyway, this maybe indicates that we don't need ITarget at all...

                _normalBehavior.FixedUpdate(actor);

                if (_preventativeFaceplantCheck.reached)
                {
                    _preventativeFaceplantCheck.Start();

                    var player = Main.Instance.game.player;
                    var layers = actor.actorType.AIrepel > 0 ? Consts.ENVIRONMENT_LAYER_MASK | Consts.FRIENDLY_LAYER_MASK : Consts.ENVIRONMENT_LAYER_MASK;

                    //KAI: it's kind of rubbish to be handling both wall and player collisions this way, but meh
                    int hits = Physics2D.RaycastNonAlloc(
                        actor.transform.position,
                        player.transform.position - actor.transform.position,
                        s_collisions,
                        actor.actorType.AIrepel > 0 ? actor.actorType.AIrepel : 4,
                        layers
                    );
                    if (hits > 0)
                    {
                        Debug.Log(string.Format("{0} hits {1},  preventative action!", actor.name, s_collisions[0].collider.name));
                        actor.lastFaceplantTime = Time.fixedTime;
                    }
                }
            }
        }
    }
}

