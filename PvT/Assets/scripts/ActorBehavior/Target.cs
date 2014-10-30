using UnityEngine;
using System.Collections;

public sealed class PlayerTarget : ITarget
{
    // singleton, this is really just a lambda
    PlayerTarget() {}

    static public ITarget Instance = new PlayerTarget();
    public GameObject actor
    {
        get { return Main.Instance.game.player; }
    }
}

public sealed class Target : ITarget
{
    readonly GameObject gameObject;
    public Target(GameObject go) { gameObject = go; }

    public GameObject actor
    {
        get { return gameObject; }
    }
}
