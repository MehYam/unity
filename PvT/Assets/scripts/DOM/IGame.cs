using UnityEngine;
using System.Collections;

public interface IGame
{
    XRect WorldBounds { get; }

    void SetMap(GameObject mapPrefab);

    GameObject player { get; }
    bool enemyInPossession { get; }

    Loader loader { get; }
    Effects effects { get; }

    GameObject SpawnPlayer(Vector3 location);
    GameObject SpawnMob(string vehicleKey, bool defaultAI = true);
    GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.CollisionLayer layer);

    void Debug_Respawn(Loader loader);
}
