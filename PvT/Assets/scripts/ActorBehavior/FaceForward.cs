using UnityEngine;
using System.Collections;

// points the actor in their direction of travel
public sealed class FaceForward : IActorBehavior
{
	public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        if (go.rigidbody2D.velocity != Vector2.zero)
        {
            go.transform.rotation = Consts.GetLookAtAngle(go.transform, go.rigidbody2D.velocity);
        }
	}
}
