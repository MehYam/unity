using UnityEngine;
using System.Collections;

public sealed class PlayerAccel : MonoBehaviour
{
    public float acceleration = 0;
    public bool useWheels = true;

    WheelJoint2D left;
    WheelJoint2D right;
    void Start()
    {
        var joints = GetComponents<WheelJoint2D>();

        foreach (var joint in joints)
        {
            if (joint.connectedBody.gameObject.transform.localPosition.x < 0)
            {
                left = joint;
            }
            else
            {
                right = joint;
            }
        }
        Debug.Log(string.Format("Joints: {0}, {1} from {2} joints", left, right, joints.Length));
    }

	// Update is called once per frame
	void FixedUpdate()
    {
        left.useMotor = false;
        right.useMotor = false;

        float horz = Input.GetAxis("Horizontal");
        if (horz < 0)
        {
            SetAccel(right, acceleration);
        }
        else if (horz > 0)
        {
            SetAccel(left, -acceleration);
        }

        float vert = Input.GetAxis("Vertical");
        if (vert != 0)
        {
            rigidbody2D.AddForce(Vector2.up * vert * 1000);
        }
	}

    static void SetAccel(WheelJoint2D joint, float accel)
    {
        var motor = joint.motor;
        motor.motorSpeed = accel;

        joint.motor = motor;
        joint.useMotor = true;
    }
}
