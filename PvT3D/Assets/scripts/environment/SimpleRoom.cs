using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRoom : MonoBehaviour
{
    void OnActorEnter(Actor actor)
    {
        if (actor.room != this)
        {
            Debug.LogFormat("Room {0} sees actor {1} enter", name, actor.name);
        }
    }
}
