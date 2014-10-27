using UnityEngine;
using System.Collections;

public sealed class ActorModifier
{
    static public readonly ActorModifier IDENTITY = new ActorModifier(1, 1);

    public readonly float maxSpeed;
    public readonly float acceleration;

    public ActorModifier(float maxSpeed, float acceleration)
    {
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
    }

    public override string ToString()
    {
        return string.Format("maxSpeed {1}, acceleration {2}", maxSpeed, acceleration);
    }
}
