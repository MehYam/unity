using UnityEngine;
using System.Collections;

public class Thrust : MonoBehaviour
{
    public float Magnitude = 3;

	// Update is called once per frame
	void Update()
    {
        var thrustLookAt = new Vector2(0, Magnitude);

        // now rotate the point in the direction of the actor, and apply the trust in that direction
        thrustLookAt = Consts.RotatePoint(thrustLookAt, -Consts.ACTOR_NOSE_OFFSET - transform.rotation.eulerAngles.z);
        rigidbody2D.AddForce(thrustLookAt);
	}
}
