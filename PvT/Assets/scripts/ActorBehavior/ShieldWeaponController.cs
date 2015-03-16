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
    
    Timer _limiter;
    public ShieldWeaponController(Consts.CollisionLayer layer, ActorType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
        _limiter = new Timer(weapon.attrs.rate);
        _limiter.Stop();
    }

    GameObject _shield;
    public void OnStart(Actor actor)
    {
        if (_shield == null && _limiter.reached)
        {
            // create the GameObject
            var shieldType = Main.Instance.game.loader.GetActorType(_weapon.actorName);
            _shield = shieldType.Spawn();
            var ammo = _shield.GetComponent<Ammo>();
            if (ammo != null)
            {
                ammo.weapon = _weapon;
            }

            _shield.GetComponent<Shield>().launcher = actor;
            _shield.layer = (int)_layer;
        }
    }

    public void OnFrame(Actor actor)
    {
        if (_shield == null && _limiter.reached)
        {
            OnStart(actor);
        }
        if (_shield != null)
        {
            _limiter.Start();
        }
    }

    public void OnEnd(Actor actor)
    {
        if (_shield != null)
        {
            _limiter.Start();

            _shield.SendMessage("OnEndFiring");
        }
    }
}
