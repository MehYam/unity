using UnityEngine;
using System.Collections;

public class PlayerInput : IActorBehavior
{
    readonly VehicleType vehicle;
    bool isMoving = true;

    readonly IActorBehavior duringInput;
    public PlayerInput(VehicleType vehicle, IActorBehavior duringInputBehavior = null)
    {
        this.vehicle = vehicle;
        duringInput = duringInputBehavior;
    }
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;

        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horz != 0 || vert != 0)
        {
            //Debug.Log(actor.acceleration);
            actor.gameObject.rigidbody2D.AddForce(new Vector2(horz * actor.acceleration, vert * actor.acceleration));

            if (duringInput != null)
            {
                duringInput.FixedUpdate(actor);
            }
        }

        // watch for transition between moving and not moving
        if ((go.rigidbody2D.velocity == Vector2.zero) == isMoving)
        {
            isMoving = !isMoving;
        }
    }
}
