using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    static public int ActiveHerolings { get; private set; }

    RateLimiter _reabsorbTimeout;
    RateLimiter _roamBoredom;
    void Awake()
    {
        _reabsorbTimeout = new RateLimiter(Consts.HEROLING_UNABSORBABLE);
        _roamBoredom = new RateLimiter(Consts.HEROLING_ROAM_BOREDOM);

        behavior = ROAM;

        ++ActiveHerolings;

        GlobalGameEvent.Instance.FireHerolingLaunched();
    }

    protected override void HandleCollision(ContactPoint2D contact)
    {
        var coll = contact.collider.gameObject;
        switch ((Consts.Layer)coll.layer)
        {
            case Consts.Layer.MOB:
                if (behavior == ROAM)
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
        _roamBoredom = null;
        _attachBoredom = new RateLimiter(Consts.HEROLING_ATTACH_BOREDOM);

        GlobalGameEvent.Instance.FireHerolingAttached(mob.GetComponent<Actor>());
    }
    void Return()
    {
        if (transform.parent != null)  //KAI: dorked, because all ammo's parented initially
        {
            var parentActor = transform.parent.GetComponent<Actor>();
            if (parentActor != null)
            {
                transform.parent = null;
                GlobalGameEvent.Instance.FireHerolingDetached(parentActor);

                // re-enable physics
                rigidbody2D.isKinematic = false;
                collider2D.enabled = true;
            }
        }
        // go back home
        behavior = RETURN;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_roamBoredom != null && _roamBoredom.reached)
        {
            Return();
            _roamBoredom = null;
        }
        else if (_attachBoredom != null && _attachBoredom.reached)
        {
            Return();
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

        GlobalGameEvent.Instance.FireHerolingDestroyed();
    }

    //KAI: cheese?
    static readonly IActorBehavior ROAM = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.playerGravitate
    );

    static readonly IActorBehavior ATTACHED = null;

    static readonly IActorBehavior RETURN = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.playerHome
    );
}
