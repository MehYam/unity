using UnityEngine;
using System.Collections;

/// <summary>
/// This is useful for assigning targets to behaviors.  Some targets need to be dynamically
/// looked up (i.e. the player), others are temporary and can be referred to directly (i.e. mobs)
/// </summary>
public interface ITarget
{
    GameObject actor { get; }
}
