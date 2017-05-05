using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRoom : MonoBehaviour
{
    void OnActorEnter(Actor actor)
    {
        if (actor.room != this)
        {
            SendMessage("OnRoomEntered", this, SendMessageOptions.DontRequireReceiver);
        }
    }
}
