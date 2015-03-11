using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;


// NOT YET PORTED OVER
    //void AttachToLaserGate(LaserGate gate)
    //{
    //    gate.Flicker(Consts.HEROLING_ATTACH_BOREDOM);
    
    //    // attach
    //    transform.parent = gate.transform;

    //    // sidle
    //    var gimmeAKiss = transform.localPosition + Util.ScatterRandomly(0.25f);
    //    gimmeAKiss.Scale(new Vector3(0.2f, 1));
    //    transform.localPosition = gimmeAKiss;

    //    // disable physics
    //    Util.DisablePhysics(gameObject);

    //    SetBehavior(ATTACHED);
    //    _roamBoredom = null;
    //    _attachBoredom = new Timer(Consts.HEROLING_ATTACH_BOREDOM);
    //}


public sealed class Heroling : MonoBehaviour
{
    static public int ActiveHerolings { get; private set; }
    static public void ReturnAll()
    {
        var instances = GameObject.FindObjectsOfType<Heroling>();
        foreach (var instance in instances)
        {
            instance.Return();
        }
    }
    void OnEnable()
    {
        ++ActiveHerolings;
    }
    void OnDisable()
    {
        --ActiveHerolings;
    }

    Timer? _roamBoredom;
    void Start()
    {
        _roamBoredom = new Timer(Consts.HEROLING_ROAM_BOREDOM);

        SetBehavior(ROAM);

        // make it appear on top of mobs and friendlies
        GetComponent<SpriteRenderer>().sortingLayerID = (int)Consts.SortingLayer.AMMO_TOP;

        GetComponent<Actor>().target = PlayerTarget.Instance;

        GlobalGameEvent.Instance.FireHerolingLaunched();
    }

    static ActorAttrs SPEED_BOOST = null;  // not set at static time because we need to load the ActorAttrs from the config
    void SetBehavior(IActorBehavior b, bool speedBoost = false)
    {
        var actor = GetComponent<Actor>();
        actor.behavior = b;

        if (speedBoost)
        {
            if (SPEED_BOOST == null)
            {
                SPEED_BOOST = new ActorAttrs(2 * actor.actorType.attrs.maxSpeed, 0, 0);
            }
            actor.AddActorModifier(SPEED_BOOST);
        }
        else if (SPEED_BOOST != null)
        {
            actor.RemoveActorModifier(SPEED_BOOST);
        }
    }

    Timer? _attachBoredom;
    public void Attach(GameObject attachTo)
    {
        var actor = attachTo.GetComponent<Actor>();
        if (actor.isCapturable)
        {
            // attach
            transform.parent = attachTo.transform;

            // sidle up
            var gimmeAKiss = transform.localPosition + Util.ScatterRandomly(0.25f);
            gimmeAKiss.Scale(new Vector3(0.4f, 0.4f));
            transform.localPosition = gimmeAKiss;

            // disable physics
            Util.DisablePhysics(gameObject);

            SetBehavior(ATTACHED);
            _roamBoredom = null;
            _attachBoredom = new Timer(Consts.HEROLING_ATTACH_BOREDOM);

            GlobalGameEvent.Instance.FireHerolingAttached(attachTo.GetComponent<Actor>());
        }
    }

    void Return()
    {
        gameObject.layer = (int)Consts.CollisionLayer.HEROLINGS_RETURNING;
        if (transform.parent != null)  //KAI: dorked, because all ammo's parented initially
        {
            transform.parent.gameObject.SendMessage("OnHerolingDetach", this);
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

    //KAI: better done via coroutines?
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(string.Format("{0} receiving collision from {1}", name, collision.gameObject.name));
        collision.gameObject.SendMessage("OnHerolingCollide", this);
    }

    static readonly IActorBehavior ROAM = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceMouse,
        ActorBehaviorFactory.Instance.thrust
    );
    static readonly IActorBehavior RETURN = new CompositeBehavior(
        ActorBehaviorFactory.Instance.faceForward,
        ActorBehaviorFactory.Instance.homeToTarget
    );
    static readonly IActorBehavior ATTACHED = null;
}
