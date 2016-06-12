using UnityEngine;
using System.Collections;

public sealed class TiltOnTurn : MonoBehaviour
{
    public float multiplier = 0.1f;
    public float maxAngle = 60;

    float _lastAngle = 0;
    float _lastTime = 0;
    void FixedUpdate()
    {
        if (Time.fixedTime > 0)
        {
            var currentAngle = gameObject.transform.eulerAngles.y;
            var angularVelocity = (currentAngle - _lastAngle) / (Time.fixedTime - _lastTime);

            var angles = gameObject.transform.eulerAngles;
            angles.z = Mathf.Clamp(-angularVelocity * multiplier, -maxAngle, maxAngle);

            gameObject.transform.eulerAngles = angles;

            _lastTime = Time.fixedTime;
            _lastAngle = currentAngle;
        }
    }
}
