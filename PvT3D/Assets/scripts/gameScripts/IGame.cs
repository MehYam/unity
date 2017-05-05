using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGame
{
    GameObject          actorParent { get; }
    Actor               player { get; set; }

    GameObject          defaultPlayerPrefab { get; }
    GameObject          defaultEnemyPrefab { get; }
}
