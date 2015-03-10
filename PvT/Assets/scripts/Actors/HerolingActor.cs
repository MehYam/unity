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

    Timer? _roamBoredom;
    protected override void Start()
    {
        base.Start();
        takenDamageMultiplier = 0;

        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;

        _roamBoredom = new Timer(Consts.HEROLING_ROAM_BOREDOM);

        SetBehavior(ROAM);

        ++ActiveHerolings;

        SetExpiry(Actor.EXPIRY_INFINITE);

        // make it appear on top of mobs and friendlies
        GetComponent<SpriteRenderer>().sortingLayerID = (int)Consts.SortingLayer.AMMO_TOP;

        target = PlayerTarget.Instance;

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

    protected override void HandleCollision(GameObject other, Vector2 point, Vector2 normal_unused, Vector2 relativeVelocity_unused)
    {
        switch ((Consts.CollisionLayer)other.layer)
        {
            case Consts.CollisionLayer.ENVIRONMENT:
                var laserGate = other.transform.parent != null ?
                    other.transform.parent.GetComponent<LaserGate>() : null;
                if (laserGate != null)
                {
                    AttachToLaserGate(laserGate);
                }
                break;
            case Consts.CollisionLayer.MOB:
                if (behavior == ROAM)
                {
                    AttachToMob(other.transform);
                }
                break;
            case Consts.CollisionLayer.FRIENDLY:
                Reabsorb();
                break;
        }
    }

    static ActorAttrs SPEED_BOOST = null;  // not set at static time because we need to load the ActorAttrs from the config
    void SetBehavior(IActorBehavior b, bool speedBoost = false)
    {
        behavior = b;

        if (speedBoost)
        {
            if (SPEED_BOOST == null)
            {
                SPEED_BOOST = new ActorAttrs(2 * actorType.attrs.maxSpeed, 0, 0);
            }
            AddActorModifier(SPEED_BOOST);
        }
        else if (SPEED_BOOST != null)
        {
            RemoveActorModifier(SPEED_BOOST);
        }
    }

    Timer? _attachBoredom;
    void AttachToMob(Transform mob)
    {
        var mobActor = mob.GetComponent<Actor>();
        if (mobActor.isCapturable)
        {
            // attach
            transform.parent = mob;

            // sidle up
            var gimmeAKiss = transform.localPosition  + Util.ScatterRandomly(0.25f);
            gimmeAKiss.Scale(new Vector3(0.4f, 0.4f));
            transform.localPosition = gimmeAKiss;

            // disable physics
            Util.DisablePhysics(gameObject);

            SetBehavior(ATTACHED);
            _roamBoredom = null;
            _attachBoredom = new Timer(Consts.HEROLING_ATTACH_BOREDOM);

            //++mob.GetComponent<Actor>().attachedHerolings;
            GlobalGameEvent.Instance.FireHerolingAttached(mob.GetComponent<Actor>());
        }
    }
    void AttachToLaserGate(LaserGate gate)
    {
        gate.Flicker(Consts.HEROLING_ATTACH_BOREDOM);
    
        // attach
        transform.parent = gate.transform;

        // sidle
        var gimmeAKiss = transform.localPosition + Util.ScatterRandomly(0.25f);
        gimmeAKiss.Scale(new Vector3(0.2f, 1));
        transform.localPosition = gimmeAKiss;

        // disable physics
        Util.DisablePhysics(gameObject);

        SetBehavior(ATTACHED);
        _roamBoredom = null;
        _attachBoredom = new Timer(Consts.HEROLING_ATTACH_BOREDOM);
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
            }
        }
        // re-enable physics
        Util.EnablePhysics(gameObject);
        
        // go back home
        SetBehavior(RETURN, true);
    }

    void FixedUpdate()
    {
        if (_roamBoredom != null && _roamBoredom.Value.reached)
        {
            Return();
            _roamBoredom = null;
        }
        else if (_attachBoredom != null && _attachBoredom.Value.reached)
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
        ActorBehaviorFactory.Instance.homeToTarget
    );
}
