using UnityEngine;
using System.Collections;

using PvT.DOM;

public class PlayerInput : IActorBehavior
{
    bool isMoving = true;

    readonly IActorBehavior duringInput;
    public PlayerInput(IActorBehavior duringInputBehavior = null)
    {
        duringInput = duringInputBehavior;
    }
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;

        var horz = Input.GetAxis("Horizontal");
        var vert = Input.GetAxis("Vertical");
        if (horz != 0 || vert != 0)
        {
            if (actor.thrustEnabled)
            {
                actor.gameObject.rigidbody2D.AddForce(new Vector2(horz * actor.acceleration, vert * actor.acceleration));
            }
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
