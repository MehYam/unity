using UnityEngine;
using System.Collections;

using PvT.DOM;

public sealed class StaticTarget : ITarget
{
    readonly Vector2 _position;
    public StaticTarget(Vector2 position) { _position = position; }

    public Vector2 position
    {
        get { return _position; }
    }
}

public sealed class PlayerTarget : ITarget
{
    // singleton, this is really just a lambda
    PlayerTarget() {}

    static public ITarget Instance = new PlayerTarget();
    public Vector2 position
    {
        get { return Main.Instance.game.player.transform.position; }
    }
}
