using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleDoor : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        var otherActor = col.collider.GetComponent<Actor>();
        gameObject.GetComponent<Animation>().Play();
    }
}
