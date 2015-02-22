using UnityEngine;
using System.Collections;

public sealed class PlayerAccel : MonoBehaviour
{
    public float acceleration = 0;
    public bool useMotor = true;

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
        if (useMotor)
        {
            if (horz < 0)
            {
                AccelerateMotor(right, acceleration, useMotor);
            }
            else if (horz > 0)
            {
                AccelerateMotor(left, -acceleration, useMotor);
            }
        }
        else
        {
            rigidbody2D.AddForce(Vector2.right * horz * acceleration / 50);
        }

        float vert = Input.GetAxis("Vertical");
        if (vert != 0)
        {
            rigidbody2D.AddForce(Vector2.up * vert * 100);
        }
	}

    static void AccelerateMotor(WheelJoint2D joint, float accel, bool useMotor)
    {
        var motor = joint.motor;
        motor.motorSpeed = accel;

        joint.motor = motor;
        joint.useMotor = true;
    }
}
