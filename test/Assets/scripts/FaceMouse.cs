using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceMouse : MonoBehaviour
{
    //KAI: copy pasta with FaceForward - abstract how?  Some should swap between the FaceMouse and FaceForward behaviors, maybe
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;

    void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var targetAngleY = Util.DegreesRotationToMouseInY(gameObject.transform.position);

        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var maxRotationThisFrame = maxRPS * Time.fixedDeltaTime * 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
