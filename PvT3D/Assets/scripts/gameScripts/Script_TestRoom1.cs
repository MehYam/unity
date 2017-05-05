using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_TestRoom1 : MonoBehaviour
{
	void Start()
    {
	}
    void OnRoomEntered(SimpleRoom room)
    {
        StartCoroutine(RoomScript());
    }
    IEnumerator RoomScript()
    {
        yield return new WaitForSeconds(2);

        // close all doors
        var doors = GetComponentsInChildren<SimpleDoor>();
        foreach (var door in doors)
        {
            door.Close();
        }
    }
}
