using UnityEngine;
using System.Collections.Generic;


// This solves two problems:
//
// 1) it avoids having the Unity engine call scores of Update()'s on tiny little helper objects, and
// 2) it lets us manage the execution order of those Update()'s man
public interface IActorBehavior
{
    void FixedUpdate(GameObject go);
}

public sealed class CompositeBehavior : IActorBehavior
{
    readonly IList<IActorBehavior> subBehaviors = new List<IActorBehavior>();

    public void AddBehavior(IActorBehavior behavior)
    {
        subBehaviors.Add(behavior);
    }

    public void FixedUpdate(GameObject go)
    {
        foreach (var behavior in subBehaviors)
        {
            behavior.FixedUpdate(go);
        }
    }
}

public sealed class BehaviorSequence: IActorBehavior
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
    public void FixedUpdate(GameObject go)
    {
        if (subBehaviors.Count > 0)
        {
            if (subBehaviors[currentItem].rate.Now)
            {
                ++currentItem;
            }
            if (currentItem >= subBehaviors.Count)
            {
                currentItem = 0;
            }
            subBehaviors[currentItem].behavior.FixedUpdate(go);
        }
    }
}

public sealed class ActorBehaviorFactory
{
    static public readonly ActorBehaviorFactory Instance = new ActorBehaviorFactory();

    ActorBehaviorFactory() { }

}
