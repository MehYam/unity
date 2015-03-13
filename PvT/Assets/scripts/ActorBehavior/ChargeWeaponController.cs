using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class ChargeWeaponController
{
    readonly Consts.CollisionLayer _layer;
    readonly ActorType.Weapon _weapon;
    
    Timer _limiter;
    public ChargeWeaponController(Consts.CollisionLayer layer, ActorType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
        _limiter = new Timer(weapon.attrs.rate);
        _limiter.Stop();
    }

    float _startTime = 0;
    public void OnStart(Actor actor)
    {
        _startTime = Time.fixedTime;
    }

    float _currentCharge = 0;
    public void OnFrame(Actor actor)
    {
        if (_limiter.reached)
        {
            if (_startTime == 0)
            {
                OnStart(actor);
            }

            _currentCharge = Mathf.Min(1, (Time.fixedTime - _startTime) / _weapon.attrs.chargeSeconds);

            if (_currentCharge == 1)
            {
                var selfHarm = Time.fixedDeltaTime * Consts.WEAPON_CHARGE_OVERLOAD_PCT_DMG * actor.health;
                //Debug.Log("ChargeWeapon self harm " + selfHarm + " over " + Time.fixedDeltaTime);
            
                // deliver self damage to the wielder
                actor.TakeDamage(selfHarm);
            }

            GlobalGameEvent.Instance.FireWeaponCharge(actor, _currentCharge);
        }
    }

    public void OnEnd(Actor actor)
    {
        if (_currentCharge > 0)
        {
            Main.Instance.game.SpawnHotspot(actor, _weapon, _currentCharge, _layer); 

            _startTime = 0;
            _currentCharge = 0;
            _limiter.Start();

            GlobalGameEvent.Instance.FireWeaponCharge(actor, _currentCharge);
        }
    }
}
