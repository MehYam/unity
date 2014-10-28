﻿using UnityEngine;
using System.Collections;

using PvT.Util;

/// <summary>
/// This class mirrors ChargeWeaponController.  There may be some way to consolidate the two.
/// </summary>
public sealed class ShieldWeaponController 
{
    readonly Consts.CollisionLayer _layer;
    readonly WorldObjectType.Weapon _weapon;
    readonly RateLimiter _limiter;
    public ShieldWeaponController(Consts.CollisionLayer layer, WorldObjectType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
        _limiter = new RateLimiter(weapon.rate);
    }

    GameObject _shield;
    Vector2 _localShieldPos;
    public void Start(Actor actor)
    {
        if (_shield == null && _limiter.reached)
        {
            // create the GameObject
            var vehicle = Main.Instance.game.loader.GetVehicle(_weapon.vehicleName);
            _shield = vehicle.Spawn(Consts.SortingLayer.AMMO, false);
            _shield.layer = (int)_layer;

            _shield.GetComponent<Actor>().collisionDamage = _weapon.damage;

            // init the Actor
            var shieldActor = _shield.GetComponent<Actor>();
            shieldActor.health = actor.worldObjectType.health * Consts.SHIELD_HEALTH_MULTIPLIER;
            shieldActor.SetExpiry(Actor.EXPIRY_INFINITE);
            shieldActor.explodesOnDeath = false;
            shieldActor.showsHealthBar = false;

            shieldActor.behavior = ActorBehaviorFactory.Instance.CreateFadeWithHealthAndExpiry(actor.worldObjectType.health);

            // position the shield
            Util.PrepareLaunch(actor.transform, _shield.transform, _weapon.offset, _weapon.angle);
            _shield.transform.parent = actor.transform;
            _localShieldPos = _shield.transform.localPosition;

            GlobalGameEvent.Instance.FireAmmoSpawned(shieldActor);
        }
    }

    public void OnFrame(Actor actor)
    {
        if (_shield == null && _limiter.reached)
        {
            Start(actor);
        }
        if (_shield != null)
        {
            _shield.transform.localPosition = _localShieldPos;
        }
    }

    const float LAUNCH_LIFE = 1.5f;
    public void Discharge(Actor actor)
    {
        if (_shield != null)
        {
            // release shield
            var shieldActor = _shield.GetComponent<Actor>();
            shieldActor.SetExpiry(LAUNCH_LIFE);

            _shield.transform.parent = Main.Instance.AmmoParent.transform;
            _shield.AddComponent<Rigidbody2D>();
            _shield.rigidbody2D.drag = 1;
            _shield.rigidbody2D.mass = 250;

            var shieldVelocity = actor.rigidbody2D.velocity;

            shieldVelocity += Util.GetLookAtVector(_shield.transform.rotation.eulerAngles.z, Consts.SHIELD_BOOST);

            _shield.rigidbody2D.velocity = shieldVelocity;
            _shield = null;

            _limiter.Start();
        }
    }
}
