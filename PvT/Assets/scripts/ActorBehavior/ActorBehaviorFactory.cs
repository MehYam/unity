using UnityEngine;
using System.Collections.Generic;

//THINK ABOUT REPLACING THESE WITH LAMBDAS.  THAT'S ALL THEY ARE.

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
    IActorBehavior _playerGravitate;
    public IActorBehavior playerGravitate
    {
        get
        {
            if (_playerGravitate == null) { _playerGravitate = new PlayerGravitate(); }
            return _playerGravitate;
        }
    }
    IActorBehavior _playerHome;
    public IActorBehavior playerHome
    {
        get
        {
            if (_playerHome == null) { _playerHome = new PlayerHome(); }
            return _playerHome;
        }
    }
    IActorBehavior _drift;
    public IActorBehavior drift
    {
        get
        {
            if (_drift == null) { _drift = new Drift(); }
            return _drift;
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
    public IActorBehavior OnFire(IActorBehavior onFireBehavior)
    {
        return new PlayerfireBehavior(onFireBehavior);
    }
    public IActorBehavior CreateShield()
    {
        return new PlayerShieldBehavior();
    }
    public IActorBehavior CreateFadeWithHealthAndExpiry(float maxHealth)
    {
        return new FadeWithHealthAndExpiry(maxHealth);
    }
    public IActorBehavior CreateTankTreadAnimator(GameObject treadLeft, GameObject treadRight)
    {
        return new TankTreadAnimator(treadLeft, treadRight);
    }
    public IActorBehavior CreateHeroAnimator(GameObject hero)
    {
        return new HeroAnimator(hero);
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
sealed class PlayerGravitate : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var newRot = Consts.GetLookAtAngle(go.transform, Main.Instance.game.player.transform.localPosition - go.transform.localPosition);
        var thrustVector = Consts.GetLookAtVector(newRot.eulerAngles.z, actor.acceleration);

        actor.gameObject.rigidbody2D.AddForce(thrustVector);
    }
}
sealed class PlayerHome : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var newRot = Consts.GetLookAtAngle(go.transform, Main.Instance.game.player.transform.position - go.transform.position);

        //actor.gameObject.transform.rotation = newRot;
        actor.gameObject.rigidbody2D.velocity = Consts.GetLookAtVector(newRot.eulerAngles.z, actor.maxSpeed / 2);
    }
}

sealed class ThrustBehavior : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var thrustVector = Consts.GetLookAtVector(actor.gameObject.transform.rotation.eulerAngles.z, actor.acceleration);
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
        var thrustLookAt = new Vector2(0, actor.acceleration);

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
                //KAI: MAJOR CHEESE
                if (weapon.type != "HEROLING" || HerolingActor.ActiveHerolings < Consts.HEROLING_LIMIT)
                {
                    var ammo = game.loader.GetVehicle(weapon.type);
                    game.SpawnAmmo(actor, ammo, weapon, layer);
                }
            }
            rate.Start();
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
    const float LAUNCH_LIFE = 3;
    const float SHIELD_RECHARGE = 1;
    const float BOOST_SECONDS = 0.3f;
    GameObject _shield;
    float _lastShieldTime = -SHIELD_RECHARGE;
    public void FixedUpdate(Actor actor)
    {
        var firing = (Input.GetButton("Fire1") || Input.GetButton("Jump"));  //KAI: -> utils class
        if (firing)
        {
            ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);
            if (_shield == null && (Time.fixedTime - _lastShieldTime >= SHIELD_RECHARGE))
            {
                var game = Main.Instance.game;

                // create the GameObject
                var shieldWeapon = actor.worldObject.weapons[0];
                var type = game.loader.GetVehicle(shieldWeapon.type);
                _shield = Main.Instance.game.SpawnAmmo(actor, type, shieldWeapon, Consts.Layer.FRIENDLY);

                _shield.rigidbody2D.velocity = Vector2.zero;
                GameObject.Destroy(_shield.rigidbody2D);  //KAI: cheese

                // init the Actor
                var shieldActor = _shield.GetComponent<Actor>();
                shieldActor.health = actor.worldObject.health;
                shieldActor.SetExpiry(Actor.EXPIRY_INFINITE);
                shieldActor.explodesOnDeath = false;
                shieldActor.showsHealthBar = false;
                shieldActor.behavior = ActorBehaviorFactory.Instance.CreateFadeWithHealthAndExpiry(actor.worldObject.health);

                _shield.transform.parent = actor.transform;
                _shield.transform.localPosition = new Vector3(shieldWeapon.offset.x, shieldWeapon.offset.y);

                // boost!
                actor.AddModifier(new ActorModifier(Time.fixedTime + BOOST_SECONDS, actor.worldObject.maxSpeed * 10, ((VehicleType)(actor.worldObject)).acceleration * 2));
                _lastShieldTime = Time.fixedTime;
            }
        }
        if (!firing && _shield != null)
        {
            // point the actor to the mouse briefly to fire the shield in that direction
            ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);

            // release shield
            var shieldActor = _shield.GetComponent<Actor>();
            shieldActor.SetExpiry(LAUNCH_LIFE);

            _shield.transform.parent = GameObject.Find("_ammoParent").transform;
            _shield.AddComponent<Rigidbody2D>();
            _shield.rigidbody2D.drag = 0;
            _shield.rigidbody2D.mass = 500;
            _shield.rigidbody2D.velocity = actor.rigidbody2D.velocity;
            _shield = null;

            actor.AddModifier(null);  //KAI: cheese
        }
    }
}

/// <summary>
///  Fades out an actor as its death and/or expiry approaches
/// </summary>
sealed class FadeWithHealthAndExpiry : IActorBehavior
{
    readonly float maxHealth;

    public FadeWithHealthAndExpiry(float maxHealth)
    {
        // we can't get this for shields since their original health isn't in WorldObjectType, but is derived from
        // the launcher
        this.maxHealth = maxHealth;  
    }

    const float END_FADE_SECONDS = 2;
    public void FixedUpdate(Actor actor)
    {
        // use the percentage of health remaining OR the percentage of the final second of life remaining,
        // whichever is less.  This way the object fades during its last second, even at full health.
        float pct = actor.health / maxHealth;
        if (actor.expireTime != Actor.EXPIRY_INFINITE)
        {
            float timeRemaining = actor.expireTime - Time.fixedTime;
            float timePct = Mathf.Min(END_FADE_SECONDS, timeRemaining);

            pct = Mathf.Min(pct, timePct);
        }
        var sprite = actor.GetComponent<SpriteRenderer>();
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, pct);
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

sealed class HeroAnimator : IActorBehavior
{
    readonly Animator anim;
    public HeroAnimator(GameObject hero)
    {
        anim = hero.GetComponent<Animator>();
    }
    public void FixedUpdate(Actor actor)
    {
        anim.speed = 0.01f + actor.rigidbody2D.velocity.sqrMagnitude/10;
    }
}

sealed class Drift : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var pos = actor.transform.localPosition;
        pos.x += Time.fixedDeltaTime * 0.01f;
        actor.transform.localPosition = pos;
    }
}
