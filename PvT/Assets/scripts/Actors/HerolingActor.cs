using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class HerolingActor : Actor
{
    static public int ActiveHerolings { get; private set; }
    static public void ReturnAll()
    {
        var instances = GameObject.FindObjectsOfType<HerolingActor>();
        foreach (var instance in instances)
        {
            instance.Return();
        }
    }

    RateLimiter _roamBoredom;
    protected override void Start()
    {
        base.Start();

        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;

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
    void OnActorDeath(Actor actor)
    {
        if (behavior == ATTACHED)
        {
            // if we're parented to a dying actor, return
            if (transform.parent == actor.transform)
            {
                Debug.Log("returning the herolings");
                Return();
            }
        }
    }
    void OnDestroy()
    {
        --ActiveHerolings;

        GlobalGameEvent.Instance.ActorDeath -= OnActorDeath;
    }

    protected override void HandleCollision(ContactPoint2D contact)
    {
        var collidingPartner = contact.collider.gameObject;
        switch ((Consts.CollisionLayer)collidingPartner.layer)
        {
            case Consts.CollisionLayer.MOB:
                if (behavior == ROAM)
                {
                    AttachToMob(collidingPartner.transform);
                }
                break;
            case Consts.CollisionLayer.FRIENDLY:
                Reabsorb();
                break;
        }
    }
    void SetBehavior(IActorBehavior b, ActorMovementModifier m = null)
    {
        behavior = b;
        speedModifier = m;
    }

    RateLimiter _attachBoredom;
    void AttachToMob(Transform mob)
    {
        var mobActor = mob.GetComponent<Actor>();
        if (mobActor.isCapturable)
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

            ++mob.GetComponent<Actor>().attachedHerolings;
            GlobalGameEvent.Instance.FireHerolingAttached(mob.GetComponent<Actor>());
        }
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
                
                --parentActor.attachedHerolings;
                GlobalGameEvent.Instance.FireHerolingDetached(parentActor);
            }
        }
        // re-enable physics
        Util.EnablePhysics(gameObject);
        
        // go back home
        SetBehavior(RETURN, new ActorMovementModifier(3, 1));
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

    //KAI: cheese?
    static readonly IActorBehavior ROAM = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceMouse,
        ActorBehaviorFactory.Instance.thrust
    );

    static readonly IActorBehavior ATTACHED = null;

    static readonly IActorBehavior RETURN = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.homeToPlayer
    );
}
