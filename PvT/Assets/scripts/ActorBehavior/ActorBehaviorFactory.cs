using UnityEngine;
using System.Collections.Generic;


// This solves two problems:
//
// 1) it avoids having the Unity engine call scores of Update()'s on tiny little helper objects, and
// 2) it lets us manage the execution order of those Update()'s man
public interface IActorBehavior
{
    void Update(GameObject go);
}

public sealed class CompositeBehavior : IActorBehavior
{
    readonly IList<IActorBehavior> subBehaviors = new List<IActorBehavior>();

    public void AddBehavior(IActorBehavior behavior)
    {
        subBehaviors.Add(behavior);
    }

    public void Update(GameObject go)
    {
        foreach (var behavior in subBehaviors)
        {
            behavior.Update(go);
        }
    }
}

public sealed class ActorBehaviorFactory
{
    static public readonly ActorBehaviorFactory Instance = new ActorBehaviorFactory();

    ActorBehaviorFactory() { }

}
