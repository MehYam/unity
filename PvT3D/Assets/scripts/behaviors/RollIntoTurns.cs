using UnityEngine;
using System.Collections;

/// <summary>
/// Implements rolling into turns by adjusting the model's Z axis
/// </summary>
public sealed class RollIntoTurns : MonoBehaviour
{
    [Tooltip("Time required to complete the roll.  Smaller values = faster response")]
    public float rollTime = 0.2f;

    [Tooltip("Maximum amount of roll to apply during turns")]
    public float maxRollAngle = 60;

    float _lastHeading = 0;
    float _velocityZ = 0;
    Actor _actor;
    void Start()
    {
        _actor = GetComponent<Actor>();
    }
    void FixedUpdate()
    {
        if (Time.fixedDeltaTime > 0)
        {
            // calculate a rotational velocity
            var currentHeading = gameObject.transform.eulerAngles.y;
            var rotationVelocity = Mathf.DeltaAngle(currentHeading, _lastHeading) / Time.fixedDeltaTime;
            var rotationPctOfMax = rotationVelocity / 360 / _actor.maxRPS;

            // tilt the ship to mimic roll during turns
            var angles = gameObject.transform.eulerAngles;
            var targetRollAngle = Mathf.Clamp(rotationPctOfMax * maxRollAngle, -maxRollAngle, maxRollAngle);

            angles.z = Mathf.SmoothDampAngle(angles.z, targetRollAngle, ref _velocityZ, rollTime);

            gameObject.transform.eulerAngles = angles;

            _lastHeading = currentHeading;
        }
    }
}
