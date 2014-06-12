using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    float _start = 0;
    void Awake()
    {
        _start = Time.fixedTime;
    }

    protected override void HandleCollision(ContactPoint2D contact)
    {
        var coll = contact.collider.gameObject;
        switch ((Consts.Layer)coll.layer)
        {
            case Consts.Layer.MOB:
                if (behavior == ROAM_OUT)
                {
                    AttachToMob(coll.transform);
                }
                break;
            case Consts.Layer.FRIENDLY:
                if ((Time.fixedTime - _start) > REABSORB_TIMEOUT)
                {
                    Debug.Log("time " + Time.fixedTime + " start " + _start);
                    Reabsorb();
                }
                break;
        }
    }
    void AttachToMob(Transform mob)
    {
        Debug.Log("Attaching to " + mob.gameObject.name);

        // attach
        transform.parent = mob;

        // sidle up
        var gimmeAKiss = transform.localPosition;
        gimmeAKiss.Scale(new Vector3(0.5f, 0.5f));
        transform.localPosition = gimmeAKiss;

        // disable physics
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.isKinematic = true;
        collider2D.enabled = false;

        behavior = ATTACHED;
        _whenAttached = Time.fixedTime;
    }
    void DetachFromMob()
    {
        transform.parent = null;

        // re-enable physics
        rigidbody2D.isKinematic = false;
        collider2D.enabled = true;

        // go back home
        behavior = ROAM_BACK;
    }

    const float REABSORB_TIMEOUT = 2;  // two seconds until can be reabsorbed
    const float ATTACH_BOREDOM = 5;  // five seconds until bored
    float _whenAttached = 0;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_whenAttached > 0 && (Time.fixedTime - _whenAttached) > ATTACH_BOREDOM)
        {
            DetachFromMob();

            Debug.Log("0-------------------------- release");
            _whenAttached = 0;
        }
    }

    void Reabsorb()
    {
        GameObject.Destroy(gameObject);
    }

    //KAI: cheese?
    static public readonly IActorBehavior ROAM_OUT = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.playerGravitate
    );

    static readonly IActorBehavior ATTACHED = null;

    static readonly IActorBehavior ROAM_BACK = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.playerGravitate
    );
}
