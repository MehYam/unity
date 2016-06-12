using UnityEngine;
using System.Collections;

public sealed class TiltOnTurn : MonoBehaviour
{
    public float smoothing = 100;
    public float multiplier = 0.1f;
    public float maxAngle = 60;

    float _lastAngle = 0;
    float _lastTime = 0;
    void FixedUpdate()
    {
        if (Time.fixedTime > 0)
        {
            var currentAngle = gameObject.transform.eulerAngles.y;
            var angularVelocity = Mathf.DeltaAngle(_lastAngle, currentAngle) / (Time.fixedTime - _lastTime);

            var angles = gameObject.transform.eulerAngles;
            var newTargetZAngle = Mathf.Clamp(-angularVelocity * multiplier, -maxAngle, maxAngle);
            if (smoothing > 0)
            {
                var angleDelta = Mathf.DeltaAngle(angles.z, newTargetZAngle);
                var smoothingRate = smoothing * Time.fixedDeltaTime;

                newTargetZAngle = angles.z + Mathf.Clamp(angleDelta, -smoothingRate, smoothingRate);
            }
            angles.z = newTargetZAngle;

            gameObject.transform.eulerAngles = angles;

            _lastTime = Time.fixedTime;
            _lastAngle = currentAngle;
        }
    }
}
