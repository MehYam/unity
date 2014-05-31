using UnityEngine;
using System.Collections;

public sealed class Ammo
{
    public readonly VehicleType type;
    public readonly int firePointIndex;

    public Ammo(VehicleType type, int firePointIndex) { this.type = type; this.firePointIndex = firePointIndex; }
}
