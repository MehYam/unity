using UnityEngine;
using System;
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
            if (behavior != null)
            {
                behavior.FixedUpdate(actor);
            }
        }
    }
}

/// <summary>
///  Wraps a behavior in a timer, so that it occurs only so often
/// </summary>
public sealed class PeriodicBehavior : IActorBehavior
{
    readonly IActorBehavior behavior;
    readonly RateLimiter rate;
    public PeriodicBehavior(IActorBehavior behavior, RateLimiter rate)
    {
        this.behavior = behavior;
        this.rate = rate;
    }
    public void FixedUpdate(Actor actor)
    {
        if (behavior != null && rate.reached)
        {
            behavior.FixedUpdate(actor);
            rate.Start();
        }
    }
}

/// <summary>
/// A behavior list that iterates through its behaviors over user-specified intervals
/// </summary>
public sealed class SequencedBehavior: IActorBehavior
{
    struct Phase
    {
        public readonly IActorBehavior behavior;
        public readonly RateLimiter duration;

        public Phase(IActorBehavior behavior, RateLimiter duration) { this.behavior = behavior; this.duration = duration; }
    }

    readonly IList<Phase> phases = new List<Phase>();
    int _current;

    /// <summary>
    /// Add a behavior to the list of behaviors in the sequence
    /// </summary>
    /// <param name="b">The behavior to add</param>
    /// <param name="duration">The duration over which to run the behavior</param>
    public void Add(IActorBehavior b, RateLimiter rate)
    {
        phases.Add(new Phase(b, rate));
    }
    public void FixedUpdate(Actor actor)
    {
        if (phases.Count == 0) return;

        // loop through the behaviors until we reach the end or have to wait
        int phaseCount = 0;
        while (true)
        {
            var phase = phases[_current];
            if (phase.duration.reached)
            {
                // advance to the next phase and start it
                ++_current;
                if (_current >= phases.Count)
                {
                    _current = 0;
                }
                phase = phases[_current];
                phase.duration.Start();
            }
            if (phase.behavior != null)
            {
                phase.behavior.FixedUpdate(actor);
                ++phaseCount;
            }

            if (!phase.duration.reached || phaseCount == phases.Count)
            {
                // have to wait out the current phase, or we've wrapped past the end, bail
                break;
            }
        }
    }
}

/// <summary>
/// Allows blocking of a behavior with another one (or none at all)
/// </summary>
public class BypassedBehavior : IActorBehavior
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
    IActorBehavior CreateOneAutofire(Consts.CollisionLayer layer, WorldObjectType.Weapon weapon)
    {
        return new PeriodicBehavior(new WeaponDischargeBehavior(layer, weapon), new RateLimiter(weapon.rate));
    }
    public IActorBehavior CreateAutofire(Consts.CollisionLayer layer, WorldObjectType.Weapon[] weapons)
    {
        if (weapons != null && weapons.Length > 0)
        {
            if (weapons.Length == 1)
            {
                return CreateOneAutofire(layer, weapons[0]);
            }

            //KAI: the logic ahead isn't quite right - this is a good place to use LINQ -
            // sort by sequence, then iterate through them
            int iLastSequence = 0;
            var retval = new SequencedBehavior();
            foreach (var w in weapons)
            {
                float rate = w.sequence != iLastSequence ? w.rate : 0;
                retval.Add(CreateOneAutofire(layer, w), new RateLimiter(rate));

                iLastSequence = w.sequence;
            }
            return retval;
        }
        return null;
    }
    public IActorBehavior CreateTurret(Consts.CollisionLayer layer, WorldObjectType.Weapon[] weapons)
    {
        return new CompositeBehavior(
            CreateAutofire(layer, weapons),
            facePlayer
        );
    }
    public IActorBehavior CreatePlayerButton(string button, IActorBehavior behavior)
    {
        return new PlayerButton(button, null, behavior, null);
    }
    public IActorBehavior CreatePlayerButton(string button, IActorBehavior onDown, IActorBehavior onFrame, IActorBehavior onUp)
    {
        return new PlayerButton(button, onDown, onFrame, onUp);
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
    public IActorBehavior CreateHerolingOverwhelmBehavior()
    {
        return new GoHomeYouAreDrunkBehavior();
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

        target = new Vector2(
            UnityEngine.Random.Range(bounds.left, bounds.right), 
            UnityEngine.Random.Range(bounds.bottom, bounds.top)
        );
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

sealed class WeaponDischargeBehavior : IActorBehavior
{
    readonly Consts.CollisionLayer layer;
    readonly WorldObjectType.Weapon weapon;

    public WeaponDischargeBehavior(Consts.CollisionLayer layer, WorldObjectType.Weapon weapon)
    {
        this.layer = layer;
        this.weapon = weapon;
    }
    public void FixedUpdate(Actor actor)
    {
        if (actor.firingEnabled)
        {
            var game = Main.Instance.game;

            //KAI: MAJOR CHEESE, maybe reimplement as an ammo limit
            if (weapon.vehicleName != "HEROLING" || HerolingActor.ActiveHerolings < Consts.HEROLING_LIMIT)
            {
                game.SpawnAmmo(actor, weapon, layer);
            }
        }
    }
}

sealed class PlayerButton : IActorBehavior
{
    readonly string button;
    readonly IActorBehavior behaviorDown;
    readonly IActorBehavior behaviorFrame;
    readonly IActorBehavior behaviorUp;

    public PlayerButton(string button, IActorBehavior behaviorDown, IActorBehavior behaviorFrame, IActorBehavior behaviorUp)
    {
        this.button = button;
        this.behaviorDown = behaviorDown;
        this.behaviorFrame = behaviorFrame;
        this.behaviorUp = behaviorUp;
    }
    bool down;
    public void FixedUpdate(Actor actor)
    {
        if (Input.GetButton(button))
        {
            if (!down)
            {
                if (behaviorDown != null) behaviorDown.FixedUpdate(actor);
                down = true;
            }
            else
            {
                if (behaviorFrame != null) behaviorFrame.FixedUpdate(actor);
            }
        }
        else if (down && behaviorUp != null)
        {
            behaviorUp.FixedUpdate(actor);
            down = false;
        }
    }
}

// KAI: trying something new here - Actions instead of interfaces, for slightly more flexibility
sealed class PlayerButton_newhotness : IActorBehavior
{
    readonly string button;
    readonly Action<Actor> onDown;
    readonly Action<Actor> onFrame;
    readonly Action<Actor> onUp;

    public PlayerButton_newhotness(string button, Action<Actor> onDown, Action<Actor> onFrame, Action<Actor> onUp)
    {
        this.button = button;
        this.onDown = onDown;
        this.onFrame = onFrame;
        this.onUp = onUp;
    }
    bool down;
    public void FixedUpdate(Actor actor)
    {
        if (Input.GetButton(button))
        {
            if (!down)
            {
                if (onDown != null) onDown(actor);
                down = true;
            }
            else
            {
                if (onFrame != null) onFrame(actor);
            }
        }
        else if (down && onUp != null)
        {
            onUp(actor);
            down = false;
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
                _shield = Main.Instance.game.SpawnAmmo(actor, shieldWeapon, Consts.CollisionLayer.FRIENDLY);

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

sealed class GoHomeYouAreDrunkBehavior : IActorBehavior
{
    RateLimiter spinRate = new RateLimiter(1, 0.5f);
    float spinSpeed = 0;

    public GoHomeYouAreDrunkBehavior()
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
        actor.gameObject.transform.Translate(
            UnityEngine.Random.Range(-VIBE, VIBE), 
            UnityEngine.Random.Range(-VIBE, VIBE), 0);
    }

    void NewSpin()
    {
        spinSpeed = UnityEngine.Random.Range(-SPIN, SPIN);
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
