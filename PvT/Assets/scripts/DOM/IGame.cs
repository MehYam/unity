using UnityEngine;
using System.Collections;

public interface IGame
{
    XRect WorldBounds { get; }

    GameObject player { get; }
    GameObject subduedByHerolings { get; }
    bool playerPossessesEnemy { get; }

    Loader loader { get; }
    Effects effects { get; }

    GameObject SpawnPlayer(Vector3 location);
    GameObject SpawnMob(string vehicleKey);
    GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer);

    void Debug_Respawn(Loader loader);
}
