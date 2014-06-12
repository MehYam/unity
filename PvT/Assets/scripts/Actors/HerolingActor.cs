using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // choose one of:
        //
        // launch behavior
        //
        // attach behavior
        //
        // rest (no) behavior
    }

    protected override void HandleCollision(ContactPoint2D contact)
    {
        var coll = contact.collider.gameObject;
        Debug.Log(coll.layer);
        if (coll.layer == (int)Consts.Layer.MOB)
        {
            // attach
            transform.parent = coll.transform;
            
            // sidle up closer to the mob
            var gimmeAKiss = transform.localPosition;
            gimmeAKiss.Scale(new Vector3(0.5f, 0.5f));
            transform.localPosition = gimmeAKiss;

            // disable physics
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.isKinematic = true;
            collider2D.enabled = false;
        }
    }
}
