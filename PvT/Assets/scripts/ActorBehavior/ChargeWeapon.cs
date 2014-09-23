using UnityEngine;
using System.Collections;

public sealed class ChargeWeapon
{
    readonly Consts.CollisionLayer _layer;
    readonly WorldObjectType.Weapon _weapon;
    public ChargeWeapon(Consts.CollisionLayer layer, WorldObjectType.Weapon weapon)
    {
        _layer = layer;
        _weapon = weapon;
    }

    float _startTime = 0;
    public void StartCharge(Actor actor)
    {
        Debug.Log("ChargeWeapon starting");
        _startTime = Time.fixedTime;
    }

    float _currentCharge = 0;
    public void Charge(Actor actor)
    {
        _currentCharge = Mathf.Min(1, (Time.fixedTime - _startTime) / _weapon.chargeSeconds);

        if (_currentCharge == 1)
        {
            var selfHarm = Time.fixedDeltaTime * Consts.WEAPON_CHARGE_OVERLOAD_PCT_DMG * actor.health;
            Debug.Log("ChargeWeapon self harm " + selfHarm);
            
            // deliver self damage to the wielder
            actor.TakeDamage(selfHarm);
        }
    }

    public void Discharge(Actor actor)
    {
        Debug.Log("ChargeWeapon discharge of " + _currentCharge);
        Main.Instance.game.SpawnHotspot(actor, _weapon, _currentCharge, _layer); 

        _startTime = 0;
        _currentCharge = 0;
    }
}
