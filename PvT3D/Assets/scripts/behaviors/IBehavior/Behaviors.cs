using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A behavior list that runs its sub-behaviors at once
/// </summary>
public sealed class CompositeBehavior : IBehavior
{
    readonly IList<Action<Actor>> subBehaviors = new List<Action<Actor>>();

    public CompositeBehavior(params object[] behaviors)
    {
        foreach (var b in behaviors)
        {
            if (b != null)
            {
                var action = b as Action<Actor>;
                var behavior = b as IBehavior;
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
                    throw new InvalidOperationException("Composite behaviors can only be type Action<Actor> or IBehavior, found a " + b.GetType().Name);
                }
            }
        }
    }

    public void Add(IBehavior behavior)
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
