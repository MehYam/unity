using UnityEngine;
using System.Collections;

public class Switch : MonoBehaviour
{
    public GameObject target;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OPEN SESAME");
        var door = Main.Instance.game.mapDoors[0];

        door.GetComponent<LaserGate>().on = false;
    }
    void OnTriggerExit2D(Collider2D other)
    {
        var door = Main.Instance.game.mapDoors[0];

        door.GetComponent<LaserGate>().on = true;
    }
}
