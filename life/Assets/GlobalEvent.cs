using System;
using UnityEngine;

using lifeEngine;

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
    public event Action<Point<int>> TileMouseover = delegate { };
    public event Action<Point<int>> TileMouseout = delegate { };
    public event Action<Point<int>> TileMousedown = delegate { };
    public event Action<Point<int>> TileMouseup = delegate { };
    public event Action<Point<int>> TileSelected = delegate { };
    public event Action<Point<int>> TileUnselected = delegate { };
    public event Action<Actor> ActorSelected = delegate { };
    public event Action<Actor> ActorUnselected = delegate { };

    public void FireTileMouseover(Point<int> tile) { TileMouseover(tile); }
    public void FireTileMouseout(Point<int> tile) { TileMouseout(tile); }
    public void FireTileMousedown(Point<int> tile) { TileMousedown(tile); }
    public void FireTileMouseup(Point<int> tile) { TileMouseup(tile); }
    public void FireTileSelected(Point<int> tile) { TileSelected(tile); }
    public void FireTileUnselected(Point<int> tile) { TileUnselected(tile);  }
    public void FireActorSelected(Actor actor) { ActorSelected(actor); }
    public void FireActorUnselected(Actor actor) { ActorUnselected(actor); }

    public event Action<Actor> ActorSpawned = delegate { };
    public void FireActorSpawned(Actor a) { ActorSpawned(a); }

    public event Action<Actor> ActorDeath = delegate { };
    public void FireActorDeath(Actor actor) { ActorDeath(actor); }

    public event Action<string> DebugString = delegate { };
    public void FireDebugString(string text) { DebugString(text); }
}
