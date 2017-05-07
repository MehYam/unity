using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAtTargetBehavior : IBehavior
{
    readonly GameObject target;
    readonly WeaponSchematic schematic;
    public FireAtTargetBehavior(GameObject target, WeaponSchematic schematic)
    {
        this.target = target;
        this.schematic = schematic;
    }
    bool _firing = false;
    public bool firing
    {
        private get { return _firing; }
        set
        {
            if (value && !_firing)
            {
                schematic.OnFireStart();
            }
            else if (!value && _firing)
            {
                schematic.OnFireEnd();
            }
            _firing = value;
        }
    }
    public void FixedUpdate(Actor actor)
    {
        if (_firing)
        {
            schematic.OnFireFrame();
        }
    }
}
