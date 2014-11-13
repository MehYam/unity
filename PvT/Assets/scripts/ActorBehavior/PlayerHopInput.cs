using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class PlayerHopInput : IActorBehavior
{
    IActorBehavior playerInput;
    public PlayerHopInput()
    {
        playerInput = new PlayerButton(
            MasterInput.impl.Primary,
            ButtonDownUpdate,
            ButtonDownUpdate,
            null
        );
    }

    public void FixedUpdate(Actor actor)
    {
        ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);
        playerInput.FixedUpdate(actor);
    }

    void ButtonDownUpdate(Actor actor)
    {
        var hop = actor.gameObject.GetOrAddComponent<HopBehavior>();
        if (hop.complete)
        {
            // jump onto mouse cursor
            var mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseInWorld.z = 0;

            // calculate the velocity required based on the height of the jump
            var jumpVector = (mouseInWorld - actor.transform.position);
            jumpVector *= HopBehavior.AIRBORNE_TIME * 1.4f ;  //KAI: magic value

            var velocity = jumpVector;
            actor.rigidbody2D.velocity = velocity;

            hop.Hop(Consts.CollisionLayer.FRIENDLY_AMMO);
        }
    }
}
