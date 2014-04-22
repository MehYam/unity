using UnityEngine;
using System.Collections;

// points the actor in their direction of travel
public sealed class FaceForward : IActorBehavior
{
	public void Update(GameObject go)
    {
        if (go.rigidbody2D.velocity != Vector2.zero)
        {
            go.transform.rotation = Consts.GetLookAtAngle(go.transform, go.rigidbody2D.velocity);
        }
	}
}
