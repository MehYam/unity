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
    public event Action<Actor> ActorSpawned = delegate { };
    public event Action<Actor> ActorDeath = delegate { };
    public event Action<string> DebugString = delegate { };

    public event Action<SimpleRoom> RoomEntered = delegate { };
    public event Action<SimpleDoor> DoorOpened = delegate { };

    public void FireMainReady() { MainReady(); }
    public void FireActorSpawned(Actor a) { ActorSpawned(a); }
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }
    public void FireDebugString(string text) { DebugString(text); }

    public void FireRoomEntered(SimpleRoom room) { RoomEntered(room); }
    public void FireDoorOpened(SimpleDoor door) { DoorOpened(door); }
}
