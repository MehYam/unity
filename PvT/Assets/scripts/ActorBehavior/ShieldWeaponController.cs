using UnityEngine;
using System.Collections;

using PvT.Util;

/// <summary>
/// This class mirrors ChargeWeaponController.  There may be some way to consolidate the two.
/// </summary>
public sealed class ShieldWeaponController 
{
    readonly Consts.CollisionLayer _layer;
    readonly ActorType.Weapon _weapon;
    readonly Rate _limiter;
    public ShieldWeaponController(Consts.CollisionLayer layer, ActorType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
        _limiter = new Rate(weapon.attrs.rate);
        _limiter.Stop();
    }

    GameObject _shield;
    Vector2 _localShieldPos;
    public void Start(Actor actor)
    {
        if (_shield == null && _limiter.reached)
        {
            // create the GameObject
            var vehicle = Main.Instance.game.loader.GetActorType(_weapon.actorName);
            _shield = vehicle.Spawn(Consts.SortingLayer.AMMO, false);
            _shield.layer = (int)_layer;

            _shield.GetComponent<Actor>().collisionDamage = _weapon.attrs.damage;

            // init the Actor
            var shieldActor = _shield.GetComponent<Actor>();
            shieldActor.health = actor.attrs.maxHealth;
            shieldActor.SetExpiry(Actor.EXPIRY_INFINITE);
            shieldActor.explodesOnDeath = false;
            shieldActor.showsHealthBar = false;
            shieldActor.reflectsAmmo = true;

            shieldActor.behavior = ActorBehaviorFactory.Instance.CreateFadeWithHealthAndExpiry(actor.attrs.maxHealth);

            // position the shield
            Util.PrepareLaunch(actor.transform, _shield.transform, _weapon.offset, _weapon.angle);
            _shield.transform.parent = actor.transform;
            _localShieldPos = _shield.transform.localPosition;

            GlobalGameEvent.Instance.FireAmmoSpawned(shieldActor, _weapon);
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
            _limiter.Start();
        }
    }

    public void Discharge(Actor actor)
    {
        if (_shield != null)
        {
            // release shield
            var shieldActor = _shield.GetComponent<Actor>();
            shieldActor.SetExpiry(actor.actorType.weapons[0].attrs.ttl);

            _shield.transform.parent = null;
            _shield.AddComponent<Rigidbody2D>();
            _shield.rigidbody2D.drag = 1;
            _shield.rigidbody2D.mass = 50;

            var shieldVelocity = actor.rigidbody2D.velocity;

            shieldVelocity += Util.GetLookAtVector(_shield.transform.rotation.eulerAngles.z) * Consts.SHIELD_BOOST;

            _shield.rigidbody2D.velocity = shieldVelocity;
            _shield = null;

            _limiter.Start();
        }
    }
}
