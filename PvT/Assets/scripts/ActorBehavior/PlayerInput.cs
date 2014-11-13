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
        var go = actor.gameObject;

        var current = MasterInput.CurrentInputVector;
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
    }
}
