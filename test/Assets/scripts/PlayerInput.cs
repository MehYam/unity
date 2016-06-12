using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
    public float acceleration = 1;
	void FixedUpdate()
    {
        var current = CurrentMovementVector;
	    if (current != Vector3.zero)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(current * acceleration);
        }
	}

    Vector3 CurrentMovementVector
    {
        get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
    }
}
