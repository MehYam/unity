using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitateToTargetBehavior : IBehavior
{
    readonly GameObject target;
    public GravitateToTargetBehavior(GameObject target) { this.target = target; }
    public void FixedUpdate(Actor actor)
    {
        actor.GetComponent<Rigidbody>().AddForce((target.transform.position - actor.transform.position).normalized * actor.acceleration);
    }
}
