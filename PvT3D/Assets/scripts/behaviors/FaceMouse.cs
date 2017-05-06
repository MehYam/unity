using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceMouse : MonoBehaviour
{
    Actor _actor;
    void Start()
    {
        _actor = GetComponent<Actor>();
    }
    void Update()  //KAI: this is way smoother than doing this in FixedUpdate - review our FixedUpdate use.
    {
        var angles = gameObject.transform.eulerAngles;
        var targetAngleY = Util.DegreesRotationToMouseInY(gameObject.transform.position);

        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var maxRotationThisFrame = _actor.maxRPS > 0 ? _actor.maxRPS * Time.fixedDeltaTime * 360 : 360;

        angleDelta = Mathf.Clamp(angleDelta, -maxRotationThisFrame, maxRotationThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
