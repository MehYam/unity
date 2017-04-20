using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleDoor : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        var otherActor = col.collider.GetComponent<Actor>();
        Debug.Log("door struck by " + otherActor.name);

        gameObject.GetComponent<Animation>().Play();
    }
}
