using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceForward : MonoBehaviour
{
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;

    //KAI: this is really doing two unrelated things, facing forward OR facing the fire direction.  Would be nice to put
    // some thought into selecting between different scripts

    [Tooltip("Rotate towards cursor or thumbstick while firing")]
    public bool faceFiringDirection = true;
	void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var rb = gameObject.GetComponent<Rigidbody>();

        // determine the target angle based on heading or firing direction
        float targetAngleY = angles.y;
        if (faceFiringDirection)
        {
            var firingDirection = InputUtil.GetFiringVector(transform.position);
            if (firingDirection != Vector3.zero)
            {
                targetAngleY = Util.DegreesRotationInY(firingDirection);
            }
        }
        if (targetAngleY == angles.y && rb.velocity != Vector3.zero && InputUtil.MovementVector != Vector3.zero)
        {
            targetAngleY = Util.DegreesRotationInY(rb.velocity);
        }
        if (targetAngleY != angles.y)
        {
            // tween the rotation
            var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
            var maxRotationThisFrame = maxRPS * Time.fixedDeltaTime * 360;

            angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

            angles.y = angles.y + angleDelta;
            gameObject.transform.eulerAngles = angles;
        }
    }
}
