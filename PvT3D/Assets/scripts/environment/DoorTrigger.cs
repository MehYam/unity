using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.LogFormat("{0} entered door trigger of {1}", other.name, name);

        var actor = other.GetComponent<Actor>();
        if (actor != null)
        {
            Debug.Log("have actor");
            var room = transform.parent.GetComponentInParent<SimpleRoom>();
            if (room != null)
            {
                Debug.Log("have room");

                //KAI: don't need SendMessage given that we have an concrete interfaces... think about this.
                room.SendMessage("OnActorEnter", actor);
                actor.SendMessage("OnRoomEnter", room);
            }
        }
    }
}
