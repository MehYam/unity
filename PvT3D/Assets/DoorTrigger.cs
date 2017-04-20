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
            Debug.Log("oen");
            var room = transform.parent.GetComponentInParent<SimpleRoom>();
            if (room != null)
            {
                Debug.Log("two");
                //KAI: don't need SendMessage given that we have an Actor interface... think about this.
                actor.SendMessage("OnRoomEnter", room);
            }
        }
    }
}
