using UnityEngine;
using System.Collections;

public class PlayerInput : IActorBehavior
{
    readonly int maxVelocity;
    readonly int acceleration;

    public PlayerInput(int maxVelocity, int acceleration)
    {
        this.maxVelocity = maxVelocity;
        this.acceleration = acceleration;
    }

    // Update is called once per frame
    bool isMoving = true;
	public void Update(GameObject go)
    {
        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        var vel = go.rigidbody2D.velocity;
        if (Mathf.Abs(vel.x) > maxVelocity)
        {
            horz = 0;
        }
        if (Mathf.Abs(vel.y) > maxVelocity)
        {
            vert = 0;
        }
        if (horz != 0 || vert != 0)
        {
            go.rigidbody2D.AddForce(new Vector2(horz * acceleration, vert * acceleration));
        }

        go.rigidbody2D.angularVelocity = 0;

        // watch for transition between moving and not moving
        if ((go.rigidbody2D.velocity == Vector2.zero) == isMoving)
        {
            isMoving = !isMoving;
            Debug.Log("Moving: " + isMoving);
        }
    }
}
