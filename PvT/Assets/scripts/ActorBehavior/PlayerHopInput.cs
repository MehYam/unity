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
        playerInput.FixedUpdate(actor);

        if (hop != null)
        {
            hop.FixedUpdate(actor);
            if (hop.IsComplete(actor))
            {
                hop = null;

                actor.rigidbody2D.velocity = Vector2.zero;
            }
        }
    }

    void ButtonDownUpdate(Actor actor)
    {
        if (hop == null)
        {
            hop = new HopBehavior();

            var lookAt = Util.GetLookAtVectorToMouse(actor.transform.position);
            actor.rigidbody2D.velocity = lookAt * actor.maxSpeed;

            ActorBehaviorFactory.Instance.faceForward.FixedUpdate(actor);
        }
    }
}
