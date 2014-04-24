using UnityEngine;
using System.Collections;

public sealed class FaceMouseOnFire : IActorBehavior
{
	public void FixedUpdate(GameObject go)
    {
        if (Input.GetButton("Fire1"))
        {
            // point towards the mouse when firing
            var mouse = Input.mousePosition;
            var screenPoint = Camera.main.WorldToScreenPoint(go.transform.localPosition);
            var lookDirection = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
            go.transform.rotation = Consts.GetLookAtAngle(go.transform, lookDirection);
        }
	}
}
