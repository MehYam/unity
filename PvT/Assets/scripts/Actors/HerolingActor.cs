using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    RateLimiter _reabsorbTimeout;
    RateLimiter _launchBoredom;
    void Awake()
    {
        _reabsorbTimeout = new RateLimiter(Consts.HEROLING_UNABSORBABLE);
        _launchBoredom = new RateLimiter(Consts.HEROLING_LAUNCH_BOREDOM);
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
                if (_reabsorbTimeout.reached)
                {
                    Reabsorb();
                }
                break;
        }
    }
    RateLimiter _attachBoredom;
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
        _attachBoredom = new RateLimiter(Consts.HEROLING_ATTACH_BOREDOM);
    }
    void TimeToGoBack()
    {
        transform.parent = null;

        // re-enable physics
        rigidbody2D.isKinematic = false;
        collider2D.enabled = true;

        // go back home
        behavior = ROAM_BACK;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_launchBoredom != null && _launchBoredom.reached)
        {
            TimeToGoBack();
            _launchBoredom = null;
        }
        else if (_attachBoredom != null && _attachBoredom.reached)
        {
            TimeToGoBack();
            _attachBoredom = null;
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
        ActorBehaviorFactory.Instance.playerHome
    );
}
