using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGame
{
    //KAI: is this interface useful?
    Actor               player { get; set; }

    GameObject          actorParent { get; }
    GameObject          ammoParent { get; }
    GameObject          effectParent { get; }

    GameObject          defaultPlayerPrefab { get; }
    GameObject          defaultEnemyPrefab { get; }
    GameObject          smallExplosionPrefab { get; }
    GameObject          bigExplosionPrefab { get; }
    GameObject          plasmaExplosionPrefab { get; }
    GameObject          damageSmokePrefab { get; }
}
