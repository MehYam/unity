using System;
using UnityEngine;

/// <summary>
/// GlobalGameEvent is a broker that allows events to be attached to and fired from disparate
/// components.  It allows these events to be subscribed to anonymously, and makes it easy
/// to clean up all held references with the ReleaseAllListeners() call.
/// </summary>
class GlobalEvent
{
    static GlobalEvent _singleton = null;
    GlobalEvent() { }

    static public GlobalEvent Instance
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = new GlobalEvent();
            }
            return _singleton;
        }
    }
    static public void ReleaseAllListeners()
    {
        _singleton = null;
    }

    /// <summary>
    /// Events
    /// </summary>
    public event Action MainReady = delegate { };
    public void FireMainReady() { MainReady(); }

    public event Action<Actor> ActorSpawned = delegate { };
    public void FireActorSpawned(Actor a) { ActorSpawned(a); }

    public event Action<Actor> ActorDeath = delegate { };
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }

    public event Action<string> DebugString = delegate { };
    public void FireDebugString(string text) { DebugString(text); }
}
