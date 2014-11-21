using UnityEngine;
using System.Collections;

public sealed class ActorMovementModifier
{
    static public readonly ActorMovementModifier IDENTITY = new ActorMovementModifier(1, 1);

    public readonly float speedMultiplier;
    public readonly float accelerationMultiplier;

    public ActorMovementModifier(float speedMultiplier, float accelerationMultiplier)
    {
        this.speedMultiplier = speedMultiplier;
        this.accelerationMultiplier = accelerationMultiplier;
    }

    public override string ToString()
    {
        return string.Format("maxSpeed {1}, acceleration {2}", speedMultiplier, accelerationMultiplier);
    }
}
