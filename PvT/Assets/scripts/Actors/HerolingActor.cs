using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class HerolingActor : Actor
{
    static public int ActiveHerolings { get; private set; }
    static public void RemoveAll()
    {
        var instances = GameObject.FindObjectsOfType<HerolingActor>();
        foreach (var instance in instances)
        {
            GameObject.Destroy(instance.gameObject);
        }
    }

    RateLimiter _reabsorbTimeout;
    RateLimiter _roamBoredom;
    protected override void Start()
    {
        base.Start();

        _reabsorbTimeout = new RateLimiter(Consts.HEROLING_UNABSORBABLE);
        _roamBoredom = new RateLimiter(Consts.HEROLING_ROAM_BOREDOM);

        SetBehavior(ROAM);

        ++ActiveHerolings;

        SetExpiry(Actor.EXPIRY_INFINITE);

        // make it appear on top of mobs and friendlies
        GetComponent<SpriteRenderer>().sortingLayerID = (int)Consts.SortingLayer.AMMO_TOP;

        // give it a push
        //body.velocity =
        //    launcher.rigidbody2D.velocity + 
        //    Util.GetLookAtVector(actor.transform.rotation.eulerAngles.z, type.maxSpeed);

        GlobalGameEvent.Instance.FireHerolingLaunched();
    }

    protected override void HandleCollision(ContactPoint2D contact)
    {
        var coll = contact.collider.gameObject;
        switch ((Consts.CollisionLayer)coll.layer)
        {
            case Consts.CollisionLayer.MOB:
                if (behavior == ROAM)
                {
                    AttachToMob(coll.transform);
                }
                break;
            case Consts.CollisionLayer.FRIENDLY:
                if (_reabsorbTimeout.reached)
                {
                    Reabsorb();
                }
                break;
        }
    }
    void SetBehavior(IActorBehavior b, ActorModifier m = null)
    {
        behavior = b;
        modifier = m;
    }

    RateLimiter _attachBoredom;
    void AttachToMob(Transform mob)
    {
        // attach
        transform.parent = mob;

        // sidle up
        var gimmeAKiss = transform.localPosition;
        gimmeAKiss = gimmeAKiss + Util.ScatterRandomly(0.25f);
        gimmeAKiss.Scale(new Vector3(0.4f, 0.4f));
        transform.localPosition = gimmeAKiss;

        // disable physics
        Util.DisablePhysics(gameObject);

        SetBehavior(ATTACHED);
        _roamBoredom = null;
        _attachBoredom = new RateLimiter(Consts.HEROLING_ATTACH_BOREDOM);

        GlobalGameEvent.Instance.FireHerolingAttached(mob.GetComponent<Actor>());
    }
    void Return()
    {
        gameObject.layer = (int)Consts.CollisionLayer.HEROLINGS_RETURNING;
        if (transform.parent != null)  //KAI: dorked, because all ammo's parented initially
        {
            var parentActor = transform.parent.GetComponent<Actor>();
            if (parentActor != null)
            {
                transform.parent = null;
                GlobalGameEvent.Instance.FireHerolingDetached(parentActor);

                // re-enable physics
                Util.EnablePhysics(gameObject);
            }
        }
        // go back home
        SetBehavior(RETURN, new ActorModifier(float.MaxValue, worldObjectType.maxSpeed * 1.5f, 0));
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
    }

    //KAI: cheese?
    static readonly IActorBehavior ROAM = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceMouse,
        ActorBehaviorFactory.Instance.thrust
    );

    static readonly IActorBehavior ATTACHED = null;

    static readonly IActorBehavior RETURN = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.playerHome
    );
}
