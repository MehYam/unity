using UnityEngine;
using UnityEngine.Assertions;
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
    readonly IList<Action<Actor>> subBehaviors = new List<Action<Actor>>();

    public CompositeBehavior(params object[] behaviors)
    {
        foreach (var b in behaviors)
        {
            if (b != null)
            {
                var action = b as Action<Actor>;
                var behavior = b as IActorBehavior;
                if (action != null)
                {
                    Add(action);
                }
                else if (behavior != null)
                {
                    Add(behavior);
                }
                else
                {
                    throw new InvalidOperationException("Composite behaviors can only be type Action<Actor> or IActorBehavior, found a " + b.GetType().Name);
                }
            }
        }
    }

    public void Add(IActorBehavior behavior)
    {
        subBehaviors.Add(behavior.FixedUpdate);
    }
    public void Add(Action<Actor> behaviorAction)
    {
        subBehaviors.Add(behaviorAction);
    }
    public void FixedUpdate(Actor actor)
    {
        foreach (var behavior in subBehaviors)
        {
            if (behavior != null)
            {
                behavior(actor);
            }
        }
    }
}

/// <summary>
/// Binds a RateLimiter and IActorBehavior together
/// </summary>
public sealed class PeriodicBehavior : IActorBehavior
{
    readonly IActorBehavior behavior;
    Timer rate;
    public PeriodicBehavior(IActorBehavior behavior, Timer rate)
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
public sealed class TimedSequenceBehavior: IActorBehavior
{
    struct Phase
    {
        public readonly Action<Actor> action;
        public Timer duration;

        public Phase(Action<Actor> action, Timer duration) { this.action = action; this.duration = duration; }
    }

    readonly IList<Phase> phases = new List<Phase>();
    int _current;

    /// <summary>
    /// Add a behavior to the list of behaviors in the sequence
    /// </summary>
    /// <param name="b">The behavior to add</param>
    /// <param name="duration">The duration over which to run the behavior</param>
    public void Add(IActorBehavior b, Timer rate)
    {
        Add(b == null ? (Action<Actor>)null : b.FixedUpdate, rate);
    }
    public void Add(Action<Actor> a, Timer rate)
    {
        phases.Add(new Phase(a, rate));
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
            if (phase.action != null)
            {
                phase.action(actor);
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

public class SequenceBehavior : IActorBehavior
{
    readonly IList<ICompletableActorBehavior> _phases = new List<ICompletableActorBehavior>();
    int _current;

    public SequenceBehavior(params ICompletableActorBehavior[] behaviors)
    {
        foreach (var b in behaviors)
        {
            Add(b);
        }
    }
    public void Add(ICompletableActorBehavior behavior)
    {
        _phases.Add(behavior);
    }

    public void FixedUpdate(Actor actor)
    {
        if (_current >= _phases.Count)
        {
            _current = 0;
        }
        if (_current < _phases.Count)
        {
            _phases[_current].FixedUpdate(actor);
            if (_phases[_current].IsComplete(actor))
            {
                ++_current;
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
        Assert.AreEqual(actor, this.actor);
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
    static public readonly IActorBehavior NULL = null;
    static public readonly ActorBehaviorFactory Instance = new ActorBehaviorFactory();

    ActorBehaviorFactory() { }

    IActorBehavior _faceTarget;
    public IActorBehavior faceTarget
    {
        get
        {
            if (_faceTarget == null)
            {
                _faceTarget = new FaceTarget();
            }
            return _faceTarget;
        }
    }
    IActorBehavior _followPlayer;
    public IActorBehavior followPlayer
    {
        get
        {
            if (_followPlayer == null){_followPlayer = new CompositeBehavior(faceTarget, thrust);}
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
    IActorBehavior _thrustAway;
    public IActorBehavior thrustAway
    {
        get
        {
            if (_thrustAway == null) { _thrustAway = new ThrustBehavior(180); }
            return _thrustAway;
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
    IActorBehavior _gravitateToTarget;
    public IActorBehavior gravitateToTarget
    {
        get
        {
            if (_gravitateToTarget == null) { _gravitateToTarget = new GravitateToTarget(); }
            return _gravitateToTarget;
        }
    }
    IActorBehavior _homeToTarget;
    public IActorBehavior homeToTarget
    {
        get
        {
            if (_homeToTarget == null) { _homeToTarget = new HomeToTarget(); }
            return _homeToTarget;
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
        return new FacePoint(point);
    }
    public IActorBehavior CreatePositionTween(Vector2 position, float seconds)
    {
        return new TweenPositionBehavior(position, seconds);
    }
    public IActorBehavior CreateRoam(bool stopBeforeRotate)
    {
        return new RoamBehavior(stopBeforeRotate);
    }
    IActorBehavior CreateOneAutofire(Consts.CollisionLayer layer, ActorType.Weapon weapon)
    {
        return new PeriodicWeaponDischargeBehavior(layer, weapon);
    }
    public IActorBehavior CreateAutofire(Consts.CollisionLayer layer, ActorType.Weapon[] weapons)
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
            var retval = new TimedSequenceBehavior();
            foreach (var w in weapons)
            {
                float rate = w.sequence != iLastSequence ? w.attrs.rate : 0;
                retval.Add(CreateOneAutofire(layer, w), new Timer(rate));

                iLastSequence = w.sequence;
            }
            return retval;
        }
        return null;
    }
    public IActorBehavior CreateTurret(Consts.CollisionLayer layer, ActorType.Weapon[] weapons)
    {
        return new CompositeBehavior(
            CreateAutofire(layer, weapons),
            faceTarget
        );
    }
    public IActorBehavior CreatePlayerButton(Func<bool> buttonCallback, IActorBehavior behavior)
    {
        Action<Actor> onFrame = behavior == null ? (Action<Actor>)null : behavior.FixedUpdate;
        return new PlayerButton(buttonCallback, null, onFrame, null);
    }
    public IActorBehavior CreatePlayerButton(Func<bool> buttonCallback, IActorBehavior onDown, IActorBehavior onFrame, IActorBehavior onUp)
    {
        return new PlayerButton(buttonCallback, onDown.FixedUpdate, onFrame.FixedUpdate, onUp.FixedUpdate);
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

sealed class FacePoint : IActorBehavior
{
    readonly Vector2 point;
    public FacePoint(Vector2 point)
    {
        this.point = point;
    }
    public void FixedUpdate(Actor actor)
    {
        Util.LookAt2D(actor.transform, point, -1);
    }
}
sealed class FaceTarget : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        float maxRotation = Time.fixedDeltaTime * actor.maxRotationalVelocity;
        Util.LookAt2D(actor.transform, actor.target.position, maxRotation);
    }
}
sealed class FaceForward : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        if (go.GetComponent<Rigidbody2D>().velocity != Vector2.zero)
        {
            go.transform.eulerAngles = new Vector3(0, 0, Util.GetLookAtAngle(go.GetComponent<Rigidbody2D>().velocity));
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
        if (!whileFiring || MasterInput.impl.Primary())
        {
            // point towards the mouse when firing
            actor.gameObject.transform.eulerAngles = new Vector3(0, 0, Util.GetAngleToMouse(actor.gameObject.transform));
        }
    }
}
/// <summary>
/// Gives an object gravity towards the target.  This will tend to make the object elasticly rotate
/// around the player if the player is moving.
/// </summary>
sealed class GravitateToTarget : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var lookAt = Util.GetLookAtVector(go.transform.position, actor.target.position);
        actor.AddThrust(lookAt * actor.attrs.acceleration);
    }
}
/// <summary>
/// Similar to PlayerGravitate, the difference being that the object gets instant velocity towards the
/// player, making it unerringly collide with the target
/// </summary>
sealed class HomeToTarget : IActorBehavior
{
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var lookAt = Util.GetLookAtVector(go.transform.position, actor.target.position);
        actor.gameObject.GetComponent<Rigidbody2D>().velocity = lookAt * actor.attrs.maxSpeed;
    }
}

sealed class ThrustBehavior : IActorBehavior
{
    readonly float angle;
    public ThrustBehavior(float angle = 0) { this.angle = angle; }
    public void FixedUpdate(Actor actor)
    {
        if (actor.thrustEnabled)
        {
            var thrustVector = Util.GetLookAtVector(actor.gameObject.transform.rotation.eulerAngles.z + angle) * actor.attrs.acceleration;
            actor.AddThrust(thrustVector);
        }
    }
}

sealed class RoamBehavior : IActorBehavior
{
    enum State { ROTATING, FORWARD, BRAKING };
    State state;

    readonly bool brake;
    public RoamBehavior(bool brake)
    {
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
                Util.LookAt2D(actor.transform, target, actor.maxRotationalVelocity * Time.fixedDeltaTime);
                if (Util.IsLookingAt(actor.transform, target, 0.1f))
                {
                    state = State.FORWARD;
                }
                break;
            case State.FORWARD:
                Util.LookAt2D(actor.transform, target, actor.maxRotationalVelocity * Time.fixedDeltaTime);
                ActorBehaviorFactory.Instance.thrust.FixedUpdate(actor);

                if ((actor.transform.position - (Vector3)target).sqrMagnitude < 1)
                {
                    state = State.BRAKING;
                }
                break;
            case State.BRAKING:
                if (!brake || actor.GetComponent<Rigidbody2D>().velocity.sqrMagnitude < 0.1f)
                {
                    PickTarget();
                    state = State.ROTATING;
                }
                break;
        }
    }
}

class WeaponDischargeBehavior : IActorBehavior
{
    readonly Consts.CollisionLayer layer;
    protected readonly ActorType.Weapon weapon;

    public WeaponDischargeBehavior(Consts.CollisionLayer layer, ActorType.Weapon weapon)
    {
        this.layer = layer;
        this.weapon = weapon;
    }
    public virtual void FixedUpdate(Actor actor)
    {
        if (actor.firingEnabled)
        {
            Main.Instance.game.SpawnAmmo(actor, weapon, layer);
        }
    }
}

sealed class PeriodicWeaponDischargeBehavior : WeaponDischargeBehavior
{
    Timer rate;
    public PeriodicWeaponDischargeBehavior(Consts.CollisionLayer layer, ActorType.Weapon weapon) : base(layer, weapon) {}

    public override void FixedUpdate(Actor actor)
    {
        if (rate.reached)
        {
            base.FixedUpdate(actor);
            rate.Start(actor.weaponMods.rate * weapon.attrs.rate);
        }
    }
}

// KAI: trying something new here - Actions instead of interfaces, for slightly more flexibility
sealed class PlayerButton : IActorBehavior
{
    readonly Func<bool> buttonCallback;
    readonly Action<Actor> onDown;
    readonly Action<Actor> onFrame;
    readonly Action<Actor> onUp;

    public PlayerButton(Func<bool> buttonCallback, Action<Actor> onDown, Action<Actor> onFrame, Action<Actor> onUp)
    {
        this.buttonCallback = buttonCallback;
        this.onDown = onDown;
        this.onFrame = onFrame;
        this.onUp = onUp;
    }
    public PlayerButton(Func<bool> buttonCallback, IActorBehavior onDown, IActorBehavior onFrame, IActorBehavior onUp)
    {
        this.buttonCallback = buttonCallback;
        this.onDown = onDown == null ? null : (Action<Actor>)onDown.FixedUpdate;
        this.onFrame = onFrame == null ? null : (Action<Actor>)onFrame.FixedUpdate;
        this.onUp = onUp == null ? null : (Action<Actor>)onFrame.FixedUpdate;
    }
    bool down;
    public void FixedUpdate(Actor actor)
    {
        if (buttonCallback())
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
        else if (down)
        {
            if (onUp != null) onUp(actor);
            down = false;
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

    const float MIN_ALPHA = 0.25f;
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
        pct = Mathf.Max(pct, MIN_ALPHA);
        
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
        left.speed = right.speed = actor.GetComponent<Rigidbody2D>().velocity.sqrMagnitude;
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
        anim.speed = Mathf.Max(actor.GetComponent<Rigidbody2D>().velocity.sqrMagnitude/10, 0.1f);
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
    Timer spinRate = new Timer(1, 0.5f);
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
        if (actor.GetComponent<Rigidbody2D>() == null || !actor.GetComponent<Rigidbody2D>().isKinematic)
        {
            actor.gameObject.transform.Translate(
                UnityEngine.Random.Range(-VIBE, VIBE), 
                UnityEngine.Random.Range(-VIBE, VIBE), 0);
        }
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
        if (actor.health < actor.attrs.maxHealth)
        {
            actor.health += healthPerSecond * Time.fixedDeltaTime;
            actor.health = Mathf.Min(actor.attrs.maxHealth, actor.health);
        }
    }
}

sealed class TweenPositionBehavior : IActorBehavior
{
    //KAI: copy pasta with TweenPosition
    sealed class TweenState
    {
        readonly Vector3 destination;
        readonly float time;
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

sealed class TweenRotationBehavior : ICompletableActorBehavior
{
    sealed class TweenState
    {
        public readonly float targetAngle;
        readonly float time;
        public TweenState(float targetAngle, float time)
        {
            this.targetAngle = targetAngle;
            this.time = time * Consts.SMOOTH_DAMP_MULTIPLIER;
        }
        float velocity = 0;
        public float Update(float angle)
        {
            return Mathf.SmoothDampAngle(angle, targetAngle, ref velocity, time);
        }
    }

    readonly TweenState _state;
    public TweenRotationBehavior(float targetAngle, float time)
    {
        _state = new TweenState(targetAngle, time);
    }

    public void FixedUpdate(Actor actor)
    {
        var angles = actor.transform.eulerAngles;
        angles.z = _state.Update(actor.transform.eulerAngles.z);

        actor.transform.eulerAngles = angles;
    }

    static readonly float ANGLE_EPSILON = 1;
    public bool IsComplete(Actor actor)
    {
        return Math.Abs(Mathf.DeltaAngle(actor.transform.eulerAngles.z, _state.targetAngle)) < ANGLE_EPSILON;
    }
}
