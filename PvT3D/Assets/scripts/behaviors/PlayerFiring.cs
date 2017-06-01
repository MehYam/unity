using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class PlayerFiring : MonoBehaviour
{
    IWeaponControl weapon;
    bool fireState = false;
    void OnWeaponControlStart(IWeaponControl weapon)
    {
        this.weapon = weapon;
    }
    void FixedUpdate()
    {
        var fv = InputUtil.GetFiringVector(transform.position);
        if (fv != Vector3.zero)
        {
            // determine the change in state
            if (!fireState)
            {
                fireState = true;
                weapon.OnFireStart();
            }
            weapon.OnFireFrame();
        }
        else if (fireState)
        {
            weapon.OnFireEnd();
            fireState = false;
        }
    }
}
