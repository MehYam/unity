using UnityEngine;
using System.Collections;

public class PlayerInput : IActorBehavior
{
    VehicleType vehicle;
    bool isMoving = true;

    public void FixedUpdate(Actor actor)
    {
        if (vehicle == null)
        {
            vehicle = (VehicleType)actor.worldObject;
        }
        var go = actor.gameObject;
        var body = go.rigidbody2D;

        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horz != 0 || vert != 0)
        {
            actor.gameObject.rigidbody2D.AddForce(new Vector2(horz * vehicle.acceleration, vert * vehicle.acceleration));
        }

        //go.rigidbody2D.angularVelocity = 0;

        // watch for transition between moving and not moving
        if ((go.rigidbody2D.velocity == Vector2.zero) == isMoving)
        {
            isMoving = !isMoving;
            //Debug.Log("Moving: " + isMoving);
        }
    }
}
