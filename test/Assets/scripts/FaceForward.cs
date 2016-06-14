using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class FaceForward : MonoBehaviour
{
    public float rotationalLimit = 100;
    public bool faceMouseWhileFiring = true;
	void FixedUpdate()
    {
        var angles = gameObject.transform.eulerAngles;
        var rb = gameObject.GetComponent<Rigidbody>();

        float targetAngleY = angles.y;
        if (faceMouseWhileFiring && Input.GetButton("Fire1"))
        {
            targetAngleY = Util.DegreesRotationToMouseInY(gameObject.transform.position);
        }
        else if (rb.velocity != Vector3.zero && PlayerInput.UNDER_FORCE)
        {
            targetAngleY = Util.DegreesRotationInY(rb.velocity);
        }

        //KAI: copy pasta with FaceMouse and TiltOnTurn, somewhat - not sure how to abstract this yet.  Might be better done with Quaternion.Slerp or similar
        var angleDelta = Mathf.DeltaAngle(angles.y, targetAngleY);
        var rotationalLimitThisFrame = rotationalLimit * Time.fixedDeltaTime;

        angleDelta = Mathf.Clamp(angleDelta, -rotationalLimitThisFrame, rotationalLimitThisFrame);

        angles.y = angles.y + angleDelta;
        gameObject.transform.eulerAngles = angles;
    }
}
