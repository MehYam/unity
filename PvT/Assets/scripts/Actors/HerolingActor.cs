using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    static public int ActiveHerolings { get; private set; }

    RateLimiter _reabsorbTimeout;
    RateLimiter _launchBoredom;
    void Awake()
    {
        _reabsorbTimeout = new RateLimiter(Consts.HEROLING_UNABSORBABLE);
        _launchBoredom = new RateLimiter(Consts.HEROLING_LAUNCH_BOREDOM);

        ++ActiveHerolings;
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
        // attach
        transform.parent = mob;

        // sidle up
        var gimmeAKiss = transform.localPosition;
        gimmeAKiss = gimmeAKiss + Consts.ScatterRandomly(0.25f);
        gimmeAKiss.Scale(new Vector3(0.4f, 0.4f));
        transform.localPosition = gimmeAKiss;

        // disable physics
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.isKinematic = true;
        collider2D.enabled = false;

        behavior = ATTACHED;
        _launchBoredom = null;
        _attachBoredom = new RateLimiter(Consts.HEROLING_ATTACH_BOREDOM);

        GlobalGameEvent.Instance.FireHerolingAttached(mob.GetComponent<Actor>());
    }
    void DetachFromMob()
    {
        if (transform.parent != null)
        {
            var parent = transform.parent.GetComponent<Actor>();
            if (parent != null)
            {
                GlobalGameEvent.Instance.FireHerolingDetached(parent);
            }
            transform.parent = null;
        }

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
            DetachFromMob();
            _launchBoredom = null;
        }
        else if (_attachBoredom != null && _attachBoredom.reached)
        {
            DetachFromMob();
            _attachBoredom = null;
        }
    }

    void Reabsorb()
    {
        GameObject.Destroy(gameObject);
    }
    void OnDestroy()
    {
        //KAI: need something tighter than this - squishy Unity behavior might make this
        // number inaccurate
        --ActiveHerolings;
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
