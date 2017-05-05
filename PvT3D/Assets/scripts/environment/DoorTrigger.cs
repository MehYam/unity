using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Debug.LogFormat("{0} entered door trigger", other.name);

        var actor = other.GetComponent<Actor>();
        if (actor != null)
        {
            var room = transform.parent.GetComponentInParent<SimpleRoom>();
            if (room != null)
            {
                //KAI: don't need SendMessage given that we have an concrete interfaces... think about this.
                room.SendMessage("OnActorEnter", actor);
                actor.SendMessage("OnRoomEnter", room);
            }
        }
    }
}
