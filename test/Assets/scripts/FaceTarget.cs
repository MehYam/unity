using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceTarget : MonoBehaviour
{
    [Tooltip("The GameObject this object will LookAt()")]
    public GameObject target;

    //KAI: copy/pasta of FaceForward
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;

    void FixedUpdate()
    {
        // determine the target angle based on heading or mouse cursor
        float targetAngleY = Util.DegreesRotationInY(target.transform.position - gameObject.transform.position);

        // tween the rotation
        var angles = gameObject.transform.eulerAngles;
        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var maxRotationThisFrame = maxRPS * Time.fixedDeltaTime * 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
