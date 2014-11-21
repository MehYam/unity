using UnityEngine;
using System.Collections;

using PvT.DOM;

public class PlayerInput : IActorBehavior
{
    readonly IActorBehavior duringInput;
    public PlayerInput(IActorBehavior duringInputBehavior = null)
    {
        duringInput = duringInputBehavior;
    }

    public void FixedUpdate(Actor actor)
    {
        var current = MasterInput.impl.CurrentMovementVector;
        if (current != Vector2.zero)
        {
            if (actor.thrustEnabled)
            {
                actor.gameObject.rigidbody2D.AddForce(current * actor.attrs.acceleration);
            }
            if (duringInput != null)
            {
                duringInput.FixedUpdate(actor);
            }
        }
    }
}
