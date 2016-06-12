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

        if (Input.GetButton("Fire1"))
        {
            Debug.Log(Input.mousePosition + ", " + MouseWorldCoordinates);

            angles.y = GetLookAtAngle(MouseWorldCoordinates - gameObject.transform.position);
        }
        else if (rb.velocity != Vector3.zero)
        {
            angles.y = GetLookAtAngle(rb.velocity);
        }
        gameObject.transform.eulerAngles = angles;
    }
}
