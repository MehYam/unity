using UnityEngine;
using System.Collections;

public interface IGame
{
    XRect WorldBounds { get; }

    GameObject player { get; }
    GameObject currentlyPossessed { get; }

    Loader loader { get; }
    Effects effects { get; }

    void SpawnPlayer(Vector3 location);
    void SpawnMob(string vehicleKey);
    GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer);

    void Debug_Respawn(Loader loader);
}
