using UnityEngine;
using System.Collections;

namespace PvT.DOM
{
    // This solves two problems:
    //
    // 1) it avoids having the Unity engine call scores of Update()'s on tiny little helper objects, and
    // 2) it lets us manage the execution order of those Update()'s manually
    public interface IActorBehavior
    {
        void FixedUpdate(Actor actor);
    }
    public interface IActorVisualBehavior
    {
        void Update(Actor actor);
    }
    public interface ICompletableActorBehavior : IActorBehavior
    {
        bool IsComplete(Actor actor);
    }
}
