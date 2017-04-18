using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleDoor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("SimpleDoor struck by: " + other.name);
    }
}
