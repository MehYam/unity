using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceMouse : MonoBehaviour
{
    //KAI: copy pasta with FaceForward - abstract how?  Some should swap between the FaceMouse and FaceForward behaviors, maybe
    [Tooltip("Maximum rotation speed in Rotations Per Second.  -1 == instant rotation")]
    public float maxRPS = 1;

    void Update()  //KAI: this is way smoother than doing this in FixedUpdate - review our FixedUpdate use.
    {
        var angles = gameObject.transform.eulerAngles;
        var targetAngleY = Util.DegreesRotationToMouseInY(gameObject.transform.position);

        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var maxRotationThisFrame = maxRPS > 0 ? maxRPS * Time.fixedDeltaTime * 360 : 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
