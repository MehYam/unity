using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PvT3D.Util;
public class FaceTargetBehavior : IBehavior
{
    readonly GameObject target;

    public FaceTargetBehavior(GameObject target) {  this.target = target; }
    public void FixedUpdate(Actor actor)
    {
        // determine the target angle based on heading or mouse cursor
        float targetAngleY = Util.DegreesRotationInY(target.transform.position - actor.gameObject.transform.position);

        // tween the rotation
        var angles = actor.gameObject.transform.eulerAngles;
        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);

#if USE_ROTATIONAL_FORCE
        var rb = gameObject.GetComponent<Rigidbody>();

        rb.AddTorque(0, angleDelta * _actor.rotationalAcceleration, 0);
#else
        var maxRotationThisFrame = actor.maxRPS * Time.fixedDeltaTime * 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        actor.gameObject.transform.eulerAngles = angles;
#endif
    }
}
