using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceForward : MonoBehaviour
{
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;

    [Tooltip("Rotate towards cursor when firing")]
    public bool faceMouseWhileFiring = true;
	void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var rb = gameObject.GetComponent<Rigidbody>();

        // determine the target angle based on heading or mouse cursor
        float targetAngleY = angles.y;
        if (faceMouseWhileFiring && Input.GetButton("Fire1"))
        {
            targetAngleY = Util.DegreesRotationToMouseInY(gameObject.transform.position);
        }
        else if (rb.velocity != Vector3.zero && PlayerInput.UNDER_FORCE)
        {
            targetAngleY = Util.DegreesRotationInY(rb.velocity);
        }

        // tween the rotation
        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var maxRotationThisFrame = maxRPS * Time.fixedDeltaTime * 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
