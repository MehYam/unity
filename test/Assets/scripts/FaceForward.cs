using UnityEngine;
using System.Collections;

public sealed class FaceForward : MonoBehaviour
{
    public float rotationalLimit = 100;

    static private float DegreesRotationAboutY(Vector3 point)
    {
        return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
    }
    static private float GetLookAtAngle(Vector3 point)
    {
        return DegreesRotationAboutY(point);
    }
    static private Vector3 MouseWorldCoordinates
    {
        get
        {
            var mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;

            return Camera.main.ScreenToWorldPoint(mousePos);
        }
    }
	void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var rb = gameObject.GetComponent<Rigidbody>();

        float targetAngleY = angles.y;
        if (Input.GetButton("Fire1"))
        {
            targetAngleY = GetLookAtAngle(MouseWorldCoordinates - gameObject.transform.position);
        }
        else if (rb.velocity != Vector3.zero && PlayerInput.UNDER_FORCE)
        {
            targetAngleY = GetLookAtAngle(rb.velocity);
        }

        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var rotationalLimitThisFrame = rotationalLimit * Time.fixedDeltaTime;

        angleDelta = Mathf.Clamp(angleDelta, -rotationalLimitThisFrame, rotationalLimitThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
