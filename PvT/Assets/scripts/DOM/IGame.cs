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
    GameObject SpawnMob(string vehicleKey);
    GameObject SpawnAmmo(Actor launcher, ActorType.Weapon weapon, Consts.CollisionLayer layer);
    GameObject SpawnHotspot(Actor launcher, ActorType.Weapon weapon, float damageMultiplier, Consts.CollisionLayer layer);

    void ShakeGround();
    void PlaySound(AudioClip clip, Vector2 position, float volume = 1);

    void Debug_Respawn(Loader loader);
}
