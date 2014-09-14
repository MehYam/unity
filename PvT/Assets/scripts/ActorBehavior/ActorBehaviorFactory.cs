using UnityEngine;
using System.Collections.Generic;

using PvT.DOM;
using PvT.Util;

//THINK ABOUT REPLACING THESE WITH LAMBDAS.  THAT'S ALL THEY ARE.

/// <summary>
/// A behavior list that runs its sub-behaviors at once
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
/// A behavior list that iterates through its behaviors over user-specified intervals
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
            var b = subBehaviors[currentItem].behavior;
            if (b != null)
            {
                b.FixedUpdate(actor);
            }
        }
    }
}

/// <summary>
/// Allows blocking of a behavior with another one (or none at all)
/// </summary>
public sealed class BypassedBehavior : IActorBehavior
{
    readonly Actor actor;
    readonly IActorBehavior currentBehavior;
    readonly IActorBehavior bypassedBehavior;

    /// <summary>
    /// Supplants an actor's registered behaviors
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="newBehavior">pass in null to disable behaviors</param>
    public BypassedBehavior(Actor actor, IActorBehavior newBehavior)
    {
        this.actor = actor;
        this.currentBehavior = newBehavior;
        this.bypassedBehavior = actor.behavior;

        actor.behavior = this;
    }

    public void FixedUpdate(Actor actor)
    {
        DebugUtil.Assert(actor == this.actor);
        if (currentBehavior != null)
        {
            currentBehavior.FixedUpdate(actor);
        }
    }

    public void Restore()
    {
        actor.behavior = bypassedBehavior;
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
            if (_facePlayer == null){_facePlayer = new FacePlayerBehavior(Consts.MAX_MOB_ROTATION_DEG_PER_SEC);}
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
    IActorBehavior _heroRegen;
    public IActorBehavior heroRegen
    {
        get
        {
            if (_heroRegen == null) { _heroRegen = new HealthRegen(Consts.HERO_REGEN); }
            return _heroRegen;
        }
    }
    public IActorBehavior CreateFacePoint(Vector2 point)
    {
        return new FacePointBehavior(point);
    }
    public IActorBehavior CreatePositionTween(Vector2 position, float seconds)
    {
        return new TweenPositionBehavior(position, seconds);
    }
    public IActorBehavior CreateRotateToPlayer(float degPerSec)
    {
        return new FacePlayerBehavior(degPerSec);
    }
    public IActorBehavior CreateRoam(float maxRotate, bool stopBeforeRotate)
    {
        return new RoamBehavior(maxRotate, stopBeforeRotate);
    }
    public IActorBehavior CreateAutofire(RateLimiter rate, Consts.Layer layer, WorldObjectType.Weapon[] weapons = null)
    {
        return new AutofireBehavior(rate, layer, weapons);
    }
    public IActorBehavior CreateTurret(RateLimiter rate, Consts.Layer layer)
    {
        return new CompositeBehavior(
            CreateAutofire(rate, layer),
            facePlayer
        );
    }
    public IActorBehavior OnFire(IActorBehavior onPrimary, IActorBehavior onSecondary)
    {
        return new PlayerfireBehavior(onPrimary, onSecondary);
    }
    public IActorBehavior OnPlayerInput(string button, IActorBehavior onFire)
    {
        return new PlayerFire(button, onFire);
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
    public IActorBehavior CreateSubduedByHerolingsBehavior()
    {
        return new SubduedByHerolingsBehavior();
    }
}

sealed class FacePointBehavior : IActorBehavior
{
    readonly Vector2 point;
    public FacePointBehavior(Vector2 point)
    {
        this.point = point;
    }
    public void FixedUpdate(Actor actor)
    {
        Util.LookAt2D(actor.transform, point, -1);
    }
}
sealed class FacePlayerBehavior : IActorBehavior
{
    public const float ROTATE_IMMEDIATE = -1;

    readonly float degPerSec;
    public FacePlayerBehavior(float degPerSec = ROTATE_IMMEDIATE)
    {
        this.degPerSec = degPerSec;
    }

    public void FixedUpdate(Actor actor)
    {
        float maxRotation = -1;
        if (degPerSec != ROTATE_IMMEDIATE)
        {
            maxRotation = Time.fixedDeltaTime * degPerSec;
        }
        Util.LookAt2D(actor.transform, Main.Instance.game.player.transform, maxRotation);
    }
}
sealed class PlayerGravitate : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var newRot = Util.GetLookAtAngle(Main.Instance.game.player.transform.localPosition - go.transform.localPosition);
        var thrustVector = Util.GetLookAtVector(newRot.eulerAngles.z, actor.acceleration);

        actor.gameObject.rigidbody2D.AddForce(thrustVector);
    }
}
sealed class PlayerHome : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var newRot = Util.GetLookAtAngle(Main.Instance.game.player.transform.position - go.transform.position);

        //actor.gameObject.transform.rotation = newRot;
        actor.gameObject.rigidbody2D.velocity = Util.GetLookAtVector(newRot.eulerAngles.z, actor.maxSpeed / 2);
    }
}

sealed class ThrustBehavior : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        if (actor.thrustEnabled)
        {
            var thrustVector = Util.GetLookAtVector(actor.gameObject.transform.rotation.eulerAngles.z, actor.acceleration);
            actor.gameObject.rigidbody2D.AddForce(thrustVector);
        }
    }
}

sealed class RoamBehavior : IActorBehavior
{
    enum State { ROTATING, FORWARD, BRAKING };
    State state;

    readonly float maxRotation;
    readonly bool brake;
    public RoamBehavior(float maxRotation, bool brake)
    {
        this.maxRotation = maxRotation;
        this.brake = brake;

        state = State.ROTATING;
        PickTarget();
    }

    Vector2 target;
    void PickTarget()
    {
        var bounds = new XRect(Main.Instance.game.WorldBounds);
        bounds.Inflate(-1);

        target = new Vector2(Random.Range(bounds.left, bounds.right), Random.Range(bounds.bottom, bounds.top));
    }
    public void FixedUpdate(Actor actor)
    {
        switch (state)
        {
            case State.ROTATING: 
                Util.LookAt2D(actor.transform, target, maxRotation);
                if (Util.IsLookingAt(actor.transform, target, 0.1f))
                {
                    state = State.FORWARD;
                }
                break;
            case State.FORWARD: 
                Util.LookAt2D(actor.transform, target, maxRotation);
                ActorBehaviorFactory.Instance.thrust.FixedUpdate(actor);

                if (Util.Sub(actor.transform.position, target).sqrMagnitude < 1)
                {
                    state = State.BRAKING;
                }
                break;
            case State.BRAKING:
                if (!brake || actor.rigidbody2D.velocity.sqrMagnitude < 0.1f)
                {
                    PickTarget();
                    state = State.ROTATING;
                }
                break;
        }
    }
}

sealed class FaceForward : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        if (go.rigidbody2D.velocity != Vector2.zero)
        {
            go.transform.rotation = Util.GetLookAtAngle(go.rigidbody2D.velocity);
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
            go.transform.rotation = Util.GetLookAtAngle(lookDirection);
        }
    }
}

sealed class AutofireBehavior : IActorBehavior
{
    readonly RateLimiter rate;
    readonly Consts.Layer layer;
    readonly WorldObjectType.Weapon[] weapons;

    public AutofireBehavior(RateLimiter rate, Consts.Layer layer, WorldObjectType.Weapon[] weapons)
    {
        this.rate = rate;
        this.layer = layer;
        this.weapons = weapons;
    }
    public void FixedUpdate(Actor actor)
    {
        if (actor.firingEnabled && rate.reached)
        {
            var game = Main.Instance.game;
            WorldObjectType.Weapon[] w = weapons;
            if (w == null)
            {
                w = actor.worldObject.weapons;
            }
            foreach (var weapon in w)
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

sealed class PlayerFire : IActorBehavior
{
    readonly string button;
    readonly IActorBehavior behavior;
    public PlayerFire(string button, IActorBehavior behavior)
    {
        this.button = button;
        this.behavior = behavior;
    }
    public void FixedUpdate(Actor actor)
    {
        if (Input.GetButton(button))
        {
            behavior.FixedUpdate(actor);
        }
    }
}

sealed class PlayerfireBehavior : IActorBehavior
{
    readonly IActorBehavior onFire;
    readonly IActorBehavior onSecondary;
    public PlayerfireBehavior(IActorBehavior onFire, IActorBehavior onSecondary)
    {
        this.onFire = onFire;
        this.onSecondary = onSecondary;
    }
    public void FixedUpdate(Actor actor)
    {
        if (onFire != null && Input.GetButton("Fire1"))
        {
            onFire.FixedUpdate(actor);
        }
        if (onSecondary != null && Input.GetButton("Jump"))
        {
            Debug.Log(Input.GetButton("Fire2"));
            onSecondary.FixedUpdate(actor);
        }
    }
}

sealed class PlayerShieldBehavior : IActorBehavior
{
    const float LAUNCH_LIFE = 1.5f;
    const float SHIELD_RECHARGE = 1;
    const float BOOST_SECONDS = 0.3f;
    GameObject _shield;
    float _lastShieldTime = -SHIELD_RECHARGE;

    enum State { NONE, FIRING_AT_MOUSE, FIRING_FORWARD }
    State _prevState;
    public void FixedUpdate(Actor actor)
    {
        var newState = State.NONE;
        if (Input.GetButton("Jump"))
        {
            newState = State.FIRING_FORWARD;
        }
        else if (Input.GetButton("Fire1"))
        {
            newState = State.FIRING_AT_MOUSE;
        }
        if (newState != State.NONE)
        {
            if (newState == State.FIRING_AT_MOUSE)
            {
                ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);
            }
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
                shieldActor.health = actor.worldObject.health * Consts.SHIELD_HEALTH_MULTIPLIER;
                shieldActor.SetExpiry(Actor.EXPIRY_INFINITE);
                shieldActor.explodesOnDeath = false;
                shieldActor.showsHealthBar = false;
                shieldActor.behavior = ActorBehaviorFactory.Instance.CreateFadeWithHealthAndExpiry(actor.worldObject.health);

                _shield.transform.parent = actor.transform;

                // boost!
                actor.modifier = new ActorModifier(Time.fixedTime + BOOST_SECONDS, actor.worldObject.maxSpeed * 2, ((VehicleType)(actor.worldObject)).acceleration * 2);
                _lastShieldTime = Time.fixedTime;
            }
        }
        if (_shield != null)
        {
            var shieldWeapon = actor.worldObject.weapons[0];
            _shield.transform.localPosition = shieldWeapon.offset;

            if (newState == State.NONE)
            {
                // point the actor to the mouse briefly to fire the shield in that direction, if that's how we were firing
                if (_prevState == State.FIRING_AT_MOUSE)
                {
                    ActorBehaviorFactory.Instance.faceMouse.FixedUpdate(actor);
                }

                // release shield
                var shieldActor = _shield.GetComponent<Actor>();
                shieldActor.SetExpiry(LAUNCH_LIFE);

                _shield.transform.parent = Main.Instance.AmmoParent.transform;
                _shield.AddComponent<Rigidbody2D>();
                _shield.rigidbody2D.drag = 1;
                _shield.rigidbody2D.mass = 500;

                // boost the shield away
                //var shieldBoost = new Vector2();
                //shieldBoost = Util.GetLookAtVector(_shield.transform.rotation.eulerAngles.z, Consts.SHIELD_BOOST) + actor.rigidbody2D.velocity;
                //shieldBoost = Vector2.ClampMagnitude(shieldBoost, actor.worldObject.maxSpeed * 4);

                _shield.rigidbody2D.velocity = actor.rigidbody2D.velocity;
                _shield = null;

                actor.modifier = null;
            }
        }
        _prevState = newState;
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
        anim.speed = Mathf.Max(actor.rigidbody2D.velocity.sqrMagnitude/10, 0.1f);
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

sealed class SubduedByHerolingsBehavior : IActorBehavior
{
    RateLimiter spinRate = new RateLimiter(1, 0.5f);
    float spinSpeed = 0;

    public SubduedByHerolingsBehavior()
    {
        NewSpin();
    }
    const float SPIN = 1;
    const float VIBE = 0.02f;
    public void FixedUpdate(Actor actor)
    {
        if (spinRate.reached)
        {
            NewSpin();
        }

        actor.gameObject.transform.Rotate(0, 0, spinSpeed);
        actor.gameObject.transform.Translate(Random.Range(-VIBE, VIBE), Random.Range(-VIBE, VIBE), 0);
    }

    void NewSpin()
    {
        spinSpeed = Random.Range(-SPIN, SPIN);
        spinRate.Start();
    }
}

sealed class HealthRegen : IActorBehavior
{
    readonly float healthPerSecond;
    public HealthRegen(float healthPerSecond)
    {
        this.healthPerSecond = healthPerSecond;
    }
    public void FixedUpdate(Actor actor)
    {
        if (actor.health < actor.worldObject.health)
        {
            actor.health += healthPerSecond * Time.fixedDeltaTime;
            actor.health = Mathf.Min(actor.worldObject.health, actor.health);
        }
    }
}

sealed class TweenPositionBehavior : IActorBehavior
{
    //KAI: copy pasta with TweenPosition
    sealed class TweenState
    {
        public readonly Vector3 destination;
        public readonly float time;
        public TweenState(Vector3 destination, float time)
        {
            this.destination = destination;
            this.time = time * Consts.SMOOTH_DAMP_MULTIPLIER;
        }
        Vector3 velocities = Vector3.zero;
        public Vector3 Update(Transform transform)
        {
            return Vector3.SmoothDamp(transform.position, destination, ref velocities, time * Consts.SMOOTH_DAMP_MULTIPLIER);
        }
    }

    readonly TweenState _state;
    public TweenPositionBehavior(Vector3 destination, float time)
    {
        _state = new TweenState(destination, time);
    }

    public void FixedUpdate(Actor actor)
    {
        actor.transform.position = _state.Update(actor.transform);
    }
}
