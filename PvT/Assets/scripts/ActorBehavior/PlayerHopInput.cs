using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class PlayerHopInput : IActorBehavior
{
    // some copy/pasta here with DeathHopperAI
    IActorBehavior playerInput;
    public PlayerHopInput()
    {
        playerInput = new PlayerButton(
            "Fire1",
            ButtonDownUpdate,
            ButtonDownUpdate,
            null
        );
    }

    ICompletableActorBehavior hop;
    public void FixedUpdate(Actor actor)
    {
        ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);
        playerInput.FixedUpdate(actor);

        if (hop != null)
        {
            hop.FixedUpdate(actor);
            if (hop.IsComplete(actor))
            {
                hop = null;

                actor.StartCoroutine(DeathHopperAI.AnimateLanding(actor, Consts.CollisionLayer.FRIENDLY_AMMO));
            }
        }
    }

    void ButtonDownUpdate(Actor actor)
    {
        if (hop == null)
        {
            hop = new HopBehavior();

            // jump onto mouse cursor
            var mouseInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseInWorld.z = 0;

            // calculate the velocity required based on the height of the jump
            var jumpVector = (mouseInWorld - actor.transform.position);
            jumpVector *= HopBehavior.AIRBORNE_TIME * 1.4f ;

            var velocity = jumpVector;
            actor.rigidbody2D.velocity = velocity;

            //actor.transform.Translate(lookAt);

        }
    }
}
