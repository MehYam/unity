using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRoom : MonoBehaviour
{
    void OnActorEnter(Actor actor)
    {
        Debug.LogFormat("SimpleRoom.OnActorEnter {0} {1}", actor.room, this.name);
        if (actor.room != this)
        {
            SendMessage("OnRoomEntered", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
