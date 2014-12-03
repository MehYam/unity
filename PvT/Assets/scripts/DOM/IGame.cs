using UnityEngine;
using System.Collections;

public interface IGame
{
    XRect WorldBounds { get; }

    void SetMap(GameObject mapPrefab);

    GameObject player { get; }
    Vector2 playerSpawn { get; set; }

    bool enemyInPossession { get; }

    Loader loader { get; }
    Effects effects { get; }

    void SpawnPlayer(Vector2 location, string actorTypeName = null);
    GameObject SpawnMob(string vehicleKey);
    GameObject SpawnAmmo(Actor launcher, ActorType.Weapon weapon, Consts.CollisionLayer layer);
    GameObject SpawnHotspot(Actor launcher, ActorType.Weapon weapon, float damageMultiplier, Consts.CollisionLayer layer);

    void ShakeGround();
    void PlaySound(Actor actor, Sounds.ActorEvent evt, float volume = 1);
    void PlaySound(Sounds.GlobalEvent evt, Vector2 pos, float volume = 1);

    void Debug_Respawn(Loader loader);
}
