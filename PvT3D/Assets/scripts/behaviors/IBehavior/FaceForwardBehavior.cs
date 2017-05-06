using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PvT3D.Util;

        //KAI: pasta from FaceForward.cs

public class FaceForwardBehavior : IBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var gameObject = actor.gameObject;

        var angles = gameObject.transform.eulerAngles;
        var rb = gameObject.GetComponent<Rigidbody>();

        float targetAngleY = angles.y;
        if (rb.velocity != Vector3.zero)
        {
            targetAngleY = Util.DegreesRotationInY(rb.velocity);
        }
        if (targetAngleY != angles.y)
        {
            // tween the rotation
            var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
            var maxRotationThisFrame = actor.maxRPS * Time.fixedDeltaTime * 360;

            angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

            angles.y = angles.y + angleDelta;
            gameObject.transform.eulerAngles = angles;
        }
    }
}
