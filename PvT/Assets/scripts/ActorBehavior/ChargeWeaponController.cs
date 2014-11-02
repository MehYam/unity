using UnityEngine;
using System.Collections;

public sealed class ChargeWeaponController
{
    readonly Consts.CollisionLayer _layer;
    readonly WorldObjectType.Weapon _weapon;
    readonly RateLimiter _limiter;
    public ChargeWeaponController(Consts.CollisionLayer layer, WorldObjectType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
        _limiter = new RateLimiter(weapon.rate);
        _limiter.End();
    }

    float _startTime = 0;
    public void StartCharge(Actor actor)
    {
        _startTime = Time.fixedTime;
    }

    float _currentCharge = 0;
    public void Charge(Actor actor)
    {
        if (_limiter.reached)
        {
            if (_startTime == 0)
            {
                StartCharge(actor);
            }

            _currentCharge = Mathf.Min(1, (Time.fixedTime - _startTime) / _weapon.chargeSeconds);

            if (_currentCharge == 1)
            {
                var selfHarm = Time.fixedDeltaTime * Consts.WEAPON_CHARGE_OVERLOAD_PCT_DMG * actor.health;
                //Debug.Log("ChargeWeapon self harm " + selfHarm + " over " + Time.fixedDeltaTime);
            
                // deliver self damage to the wielder
                actor.TakeDamage(selfHarm);
            }

            if (actor.isPlayer)
            {
                //KAI: HACK
                Main.Instance.hud.score.text = string.Format("Charge: {0}%", (int)(_currentCharge * 100));
            }
        }
    }

    public void Discharge(Actor actor)
    {
        if (_currentCharge > 0)
        {
            Main.Instance.game.SpawnHotspot(actor, _weapon, _currentCharge, _layer); 

            _startTime = 0;
            _currentCharge = 0;
            _limiter.Start();

            Main.Instance.hud.score.text = "";
        }
    }
}
