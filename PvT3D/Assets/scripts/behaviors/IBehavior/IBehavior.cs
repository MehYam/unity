using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This interface's sole purpose to replace GameObjects and MonoBehaviors with simple C# classes
/// (some of which can be singletons, if stateless).  This may be a premature optimization.
/// </summary>
public interface IBehavior
{
    void FixedUpdate(Actor actor);
}
