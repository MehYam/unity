using UnityEngine;
using System.Collections.Generic;


// This solves two problems:
//
// 1) it avoids having the Unity engine call scores of Update()'s on tiny little helper objects, and
// 2) it lets us manage the execution order of those Update()'s manually
public interface IActorBehavior
{
    void FixedUpdate(Actor actor);
}

/// <summary>
/// A behavior that runs N sub-behaviors at once
/// </summary>
public sealed class CompositeBehavior : IActorBehavior
{
    readonly IList<IActorBehavior> subBehaviors = new List<IActorBehavior>();

    public CompositeBehavior(params IActorBehavior[] behaviors)
    {
        foreach (var b in behaviors) Add(b);
    }

    public void Add(IActorBehavior behavior)
    {
        subBehaviors.Add(behavior);
    }

    public void FixedUpdate(Actor actor)
    {
        foreach (var behavior in subBehaviors)
        {
            behavior.FixedUpdate(actor);
        }
    }
}

/// <summary>
/// A behavior that runs a bunch of sub-behaviors in sequence
/// </summary>
public sealed class SequencedBehavior: IActorBehavior
{
    struct Item
    {
        public readonly IActorBehavior behavior;
        public readonly RateLimiter rate;

        public Item(IActorBehavior behavior, RateLimiter rate) { this.behavior = behavior; this.rate = rate; }
    }

    readonly IList<Item> subBehaviors = new List<Item>();
    int currentItem;

    /// <summary>
    /// Add a behavior to the list of behaviors in the sequence
    /// </summary>
    /// <param name="b">The behavior to add</param>
    /// <param name="duration">The duration over which to run the behavior</param>
    public void Add(IActorBehavior b, RateLimiter rate)
    {
        subBehaviors.Add(new Item(b, rate));
    }
    public void FixedUpdate(Actor actor)
    {
        if (subBehaviors.Count > 0)
        {
            if (subBehaviors[currentItem].rate.reached)
            {
                ++currentItem;
                if (currentItem >= subBehaviors.Count)
                {
                    currentItem = 0;
                }
                subBehaviors[currentItem].rate.Start();
            }
            subBehaviors[currentItem].behavior.FixedUpdate(actor);
        }
    }
}

//KAI: IActorBehavior could be replaced by lambda's - food for thought.  It would make it easier to
// allow different types of behaviors that take different signatures.  It also changes the nature of this factory
// from one that spits out singletons to one that holds a bunch of lambda functions
public sealed class ActorBehaviorFactory
{
    static public readonly ActorBehaviorFactory Instance = new ActorBehaviorFactory();

    ActorBehaviorFactory() { }

    IActorBehavior _facePlayer;
    public IActorBehavior facePlayer
    {
        get
        {
            if (_facePlayer == null){_facePlayer = new FacePlayerBehavior();}
            return _facePlayer;
        }
    }
    IActorBehavior _followPlayer;
    public IActorBehavior followPlayer
    {
        get
        {
            if (_followPlayer == null){_followPlayer = new CompositeBehavior(facePlayer, thrust);}
            return _followPlayer;
        }
    }
    IActorBehavior _thrust;
    public IActorBehavior thrust
    {
        get
        {
            if (_thrust == null){_thrust = new ThrustBehavior();}
            return _thrust;
        }
    }
    IActorBehavior _faceMouse;
    public IActorBehavior faceMouse
    {
        get
        {
            if (_faceMouse == null) { _faceMouse = new FaceMouse(); }
            return _faceMouse;
        }
    }
    IActorBehavior _faceMouseOnFire;
    public IActorBehavior faceMouseOnFire
    {
        get
        {
            if (_faceMouseOnFire == null) { _faceMouseOnFire = new FaceMouse(true); }
            return _faceMouseOnFire;
        }
    }
    IActorBehavior _faceForward;
    public IActorBehavior faceForward
    {
        get
        {
            if (_faceForward == null) { _faceForward = new FaceForward(); }
            return _faceForward;
        }
    }
    //IActorBehavior _whirl;
    //public IActorBehavior whirl
    //{
    //    get
    //    {
    //        if (_whirl == null) { _whirl = new Spin(10); }
    //        return _whirl;
    //    }
    //}
    public IActorBehavior CreatePatrol(RateLimiter rate)
    {
        return new Patrol();
    }
    public IActorBehavior CreateAutofire(RateLimiter rate, Consts.Layer layer)
    {
        return new AutofireBehavior(rate, layer);
    }
    public IActorBehavior CreatePlayerfire(IActorBehavior onFireBehavior)
    {
        return new PlayerfireBehavior(onFireBehavior);
    }
    public IActorBehavior CreateShield()
    {
        return new PlayerShieldBehavior();
    }
    public IActorBehavior CreateTankTreadAnimator(GameObject treadLeft, GameObject treadRight)
    {
        return new TankTreadAnimator(treadLeft, treadRight);
    }
}

sealed class FacePlayerBehavior : IActorBehavior
{
    float RotationalInertia = 0;  // from 0 to 1, 1 being completely inert
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var previous = go.transform.localRotation;
        var newRot = Consts.GetLookAtAngle(go.transform, Main.Instance.game.player.transform.localPosition - go.transform.localPosition);

        // implement a crude rotational drag by "softening" the delta.  KAI: look into relying more on the physics engine to handle this
        var angleDelta = Consts.diffAngle(previous.eulerAngles.z, newRot.eulerAngles.z);
        angleDelta *= (1 - RotationalInertia);
        go.transform.Rotate(0, 0, angleDelta);
    }
}

sealed class ThrustBehavior : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var v = (VehicleType)actor.worldObject;
        var thrustVector = Consts.GetLookAtVector(actor.gameObject.transform.rotation.eulerAngles.z, v.acceleration);

        actor.gameObject.rigidbody2D.AddForce(thrustVector);
    }
}

sealed class Patrol : IActorBehavior
{
    public Patrol()
    {
        //var bounds = Main.Instance.game.WorldBounds;
        //nextTarget = new Vector2(Consts.CoinFlip() ? bounds.left : bounds.right, Random.Range(bounds.bottom, bounds.top));
    }

    public void FixedUpdate(Actor actor)
    {
        var v = (VehicleType)actor.worldObject;
        var thrustLookAt = new Vector2(0, v.acceleration);

        // apply the thrust in the direction of the actor
        thrustLookAt = Consts.RotatePoint(thrustLookAt, -Consts.ACTOR_NOSE_OFFSET - actor.gameObject.transform.rotation.eulerAngles.z);
        actor.gameObject.rigidbody2D.AddForce(thrustLookAt);
    }
}

sealed class FaceForward : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        if (go.rigidbody2D.velocity != Vector2.zero)
        {
            go.transform.rotation = Consts.GetLookAtAngle(go.transform, go.rigidbody2D.velocity);
        }
    }
}

sealed class FaceMouse : IActorBehavior
{
    readonly bool whileFiring;
    public FaceMouse(bool whileFiring = false)
    {
        this.whileFiring = whileFiring;
    }
    public void FixedUpdate(Actor actor)
    {
        if (!whileFiring || Input.GetButton("Fire1"))
        {
            // point towards the mouse when firing
            var go = actor.gameObject;
            var mouse = Input.mousePosition;
            var screenPoint = Camera.main.WorldToScreenPoint(go.transform.position);
            var lookDirection = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
            go.transform.rotation = Consts.GetLookAtAngle(go.transform, lookDirection);
        }
    }
}

sealed class AutofireBehavior : IActorBehavior
{
    readonly RateLimiter rate = null;
    readonly Consts.Layer layer;

    public AutofireBehavior(RateLimiter rate, Consts.Layer layer)
    {
        this.rate = rate;
        this.layer = layer;
    }
    public void FixedUpdate(Actor actor)
    {
        if (rate.reached)
        {
            var game = Main.Instance.game;
            foreach (var weapon in actor.worldObject.weapons)
            {
                var ammo = game.loader.GetVehicle(weapon.type);
                game.SpawnAmmo(actor, ammo, weapon, layer);
            }
        }
    }
}

sealed class PlayerfireBehavior : IActorBehavior
{
    readonly IActorBehavior onFire;
    public PlayerfireBehavior(IActorBehavior onFire)
    {
        this.onFire = onFire;
    }
    public void FixedUpdate(Actor actor)
    {
        if ((Input.GetButton("Fire1") || Input.GetButton("Jump")))
        {
            onFire.FixedUpdate(actor);
        }
    }
}

sealed class PlayerShieldBehavior : IActorBehavior
{
    GameObject _currentShield;
    public void FixedUpdate(Actor actor)
    {
        var firing = (Input.GetButton("Fire1") || Input.GetButton("Jump"));  //KAI: -> utils class
        if (firing && _currentShield == null)
        {
            var game = Main.Instance.game;

            var shieldWeapon = actor.worldObject.weapons[0];
            var type = game.loader.GetVehicle(shieldWeapon.type);
            _currentShield = Main.Instance.game.SpawnAmmo(actor, type, shieldWeapon, Consts.Layer.FRIENDLY);

            var shieldActor = _currentShield.GetComponent<Actor>();
            shieldActor.timeToLive = 0;

            _currentShield.transform.parent = actor.transform;
            _currentShield.rigidbody2D.velocity = Vector2.zero;
        }

        if (_currentShield != null)
        {
            if (firing)
            {
                _currentShield.transform.localPosition = Vector3.zero;
                _currentShield.transform.rotation = actor.transform.rotation;
            }
            else
            {
                _currentShield.transform.parent = GameObject.Find("_ammoParent").transform;
                _currentShield.rigidbody2D.velocity = actor.rigidbody2D.velocity;
                _currentShield = null;
            }
        }
    }
}

sealed class TankTreadAnimator : IActorBehavior
{
    readonly Animator left;
    readonly Animator right;
    public TankTreadAnimator(GameObject treadLeft, GameObject treadRight)
    {
        left = treadLeft.GetComponentInChildren<Animator>();
        right = treadRight.GetComponentInChildren<Animator>();
    }
    public void FixedUpdate(Actor actor)
    {
        left.speed = right.speed = actor.rigidbody2D.velocity.sqrMagnitude;
    }
}
