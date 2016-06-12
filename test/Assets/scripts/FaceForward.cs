using UnityEngine;
using System.Collections;

public sealed class FaceForward : MonoBehaviour
{
    static private float DegreesRotationAboutY(Vector3 point)
    {
        return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
    }
    static private float GetLookAtAngle(Vector3 point)
    {
        return DegreesRotationAboutY(point);
    }
	void FixedUpdate()
    {
        var rb = gameObject.GetComponent<Rigidbody>();
        if (rb.velocity != Vector3.zero)
        {
            var angles = gameObject.transform.eulerAngles;
            angles.y = GetLookAtAngle(rb.velocity);
            gameObject.transform.eulerAngles = angles;
        }
	}
}
