using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAtTargetBehavior : IBehavior
{
    readonly GameObject target;
    readonly IWeaponControl weapon;
    public FireAtTargetBehavior(GameObject target, IWeaponControl weapon)
    {
        this.target = target;
        this.weapon = weapon;
    }
    bool _firing = false;
    public bool firing
    {
        private get { return _firing; }
        set
        {
            if (value && !_firing)
            {
                weapon.OnFireStart();
            }
            else if (!value && _firing)
            {
                weapon.OnFireEnd();
            }
            _firing = value;
        }
    }
    public void FixedUpdate(Actor actor)
    {
        if (_firing)
        {
            weapon.OnFireFrame();
        }
    }
}
