using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_TestRoom2 : MonoBehaviour
{
    void OnRoomEntered(SimpleRoom room)
    {
        Debug.LogFormat("OnRoomEntered {0}", name);
        StartCoroutine(RoomScript());
    }
    IEnumerator RoomScript()
    {
        yield return new WaitForSeconds(1);

        // close all doors
        var doors = GetComponentsInChildren<SimpleDoor>();
        foreach (var door in doors)
        {
            door.Close();
        }

        yield return new WaitForSeconds(1);

        var spawner = GetComponentInChildren<Spawner>();
        spawner.target = Main.game.player.gameObject;
        spawner.enemy = Main.game.defaultEnemyPrefab2;
        spawner.enabled = true;
    }
}
