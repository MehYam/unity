using UnityEngine;
using System.Collections;

namespace PvT.DOM
{
    /// <summary>
    /// This is useful for assigning targets to behaviors.  Some targets need to be dynamically
    /// looked up (i.e. the player).
    /// </summary>
    public interface ITarget
    {
        Vector2 position { get; }
    }
}
