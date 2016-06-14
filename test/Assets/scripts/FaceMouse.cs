using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceMouse : MonoBehaviour
{
    public float rotationalLimit = 100;
    void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var targetY = Util.DegreesRotationToMouseInY(gameObject.transform.position);

        var maxDeltaThisFrame = rotationalLimit * Time.fixedDeltaTime;
        var angleDelta = Mathf.Clamp(Mathf.DeltaAngle(angles.y, targetY), -maxDeltaThisFrame, maxDeltaThisFrame);

        angles.y += angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
