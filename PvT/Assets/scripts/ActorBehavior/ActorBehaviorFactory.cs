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
    public void AddBehavior(IActorBehavior b, RateLimiter rate)
    {
        subBehaviors.Add(new Item(b, rate));
    }
    public void FixedUpdate(Actor actor)
    {
        if (subBehaviors.Count > 0)
        {
            if (subBehaviors[currentItem].rate.now)
            {
                ++currentItem;
                subBehaviors[currentItem].rate.Start();
            }
            if (currentItem >= subBehaviors.Count)
            {
                currentItem = 0;
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

    public IActorBehavior CreatePatrol(RateLimiter rate)
    {
        return new Patrol(rate);
    }
    public IActorBehavior CreateAutofire(RateLimiter rate)
    {
        return new AutofireBehavior(rate);
    }

}

sealed class FacePlayerBehavior : IActorBehavior
{
    float RotationalInertia = 0;  // from 0 to 1, 1 being completely inert
    public void FixedUpdate(Actor actor)
    {
        var go = actor.gameObject;
        var previous = go.transform.localRotation;
        var newRot = Consts.GetLookAtAngle(go.transform, Main.Instance.Player.transform.localPosition - go.transform.localPosition);

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
        var thrustLookAt = new Vector2(0, actor.vehicle.acceleration * 10);

        // apply the thrust in the direction of the actor
        thrustLookAt = Consts.RotatePoint(thrustLookAt, -Consts.ACTOR_NOSE_OFFSET - actor.gameObject.transform.rotation.eulerAngles.z);
        actor.gameObject.rigidbody2D.AddForce(thrustLookAt);
    }
}

sealed class Patrol : IActorBehavior
{
    readonly RateLimiter rate;
    Vector2 nextTarget;

    public Patrol(RateLimiter rate)
    {
        this.rate = rate;

        var bounds = Main.Instance.gameState.WorldBounds;
        nextTarget = new Vector2(Consts.CoinFlip() ? bounds.left : bounds.right, Random.Range(bounds.bottom, bounds.top));
    }

    public void FixedUpdate(Actor actor)
    {
        var thrustLookAt = new Vector2(0, actor.vehicle.acceleration * 10);

        // apply the thrust in the direction of the actor
        thrustLookAt = Consts.RotatePoint(thrustLookAt, -Consts.ACTOR_NOSE_OFFSET - actor.gameObject.transform.rotation.eulerAngles.z);
        actor.gameObject.rigidbody2D.AddForce(thrustLookAt);
    }
}

sealed class AutofireBehavior : IActorBehavior
{
    readonly RateLimiter rate = null;

    public AutofireBehavior(RateLimiter rate)
    {
        this.rate = rate;
    }
    public void FixedUpdate(Actor actor)
    {
        if (rate.now)
        {
            var game = Main.Instance.gameState;
            foreach (var weapon in actor.vehicle.weapons)
            {
                var ammo = game.GetVehicle(weapon.type);
                game.SpawnMobAmmo(actor, ammo, weapon);
            }
        }
    }
}
