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

    static public Vector2 CurrentInputVector
    {
        get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); }
    }
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;

        var current = CurrentInputVector;
        if (current != Vector2.zero)
        {
            if (actor.thrustEnabled)
            {
                actor.gameObject.rigidbody2D.AddForce(current * actor.acceleration);
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
