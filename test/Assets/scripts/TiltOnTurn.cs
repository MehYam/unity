using UnityEngine;
using System.Collections;

public sealed class TiltOnTurn : MonoBehaviour
{
    public float sensitivity = 100;
    public float multiplier = 0.1f;  //KAI: this is really "sensitivity".  Give this more thought.
    public float maxTiltAngle = 60;

//KAI:

// the misnomer "sensitivity" is really just doing a smoothing on angle adjustments.  We won't need to do this here if
// we do it instead in FaceForward, which is arguably the right place (nothing should turn infinitely fast).
//
// Since we're likely to be doing non-physics/non-force driven movement like this, it may be worth either using some
// .SmoothAngle library call somewhere, or making our own.
    float _lastAngle = 0;
    float _lastTime = 0;
    void FixedUpdate()
    {
        if (Time.fixedTime > 0)
        {
            var currentAngle = gameObject.transform.eulerAngles.y;
            var angularVelocity = Mathf.DeltaAngle(_lastAngle, currentAngle) / (Time.fixedTime - _lastTime);

            var angles = gameObject.transform.eulerAngles;
            var newTargetZAngle = Mathf.Clamp(-angularVelocity * multiplier, -maxTiltAngle, maxTiltAngle);
            if (sensitivity > 0)
            {
                var angleDelta = Mathf.DeltaAngle(angles.z, newTargetZAngle);
                var limit = sensitivity * Time.fixedDeltaTime;

                newTargetZAngle = angles.z + Mathf.Clamp(angleDelta, -limit, limit);
            }
            angles.z = newTargetZAngle;

            gameObject.transform.eulerAngles = angles;

            _lastTime = Time.fixedTime;
            _lastAngle = currentAngle;
        }
    }
}
