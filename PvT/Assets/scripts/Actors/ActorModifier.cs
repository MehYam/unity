using UnityEngine;
using System.Collections;

public sealed class ActorModifier
{
    public readonly float expiry;
    public readonly float maxSpeed;
    public readonly float acceleration;

    public ActorModifier(float expiry, float maxSpeed, float acceleration)
    {
        this.expiry = expiry;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
    }

    public override string ToString()
    {
        return string.Format("expiry {0}, maxSpeed {1}, acceleration {2}", expiry, maxSpeed, acceleration);
    }
}
