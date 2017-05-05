using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleDoor : MonoBehaviour
{
    [SerializeField] GameObject left = null;
    [SerializeField] GameObject right = null;

    bool open = false;
    //KAI: left off here, save original panel extents so that they're restored when doors close

    void OnCollisionEnter(Collision col)
    {
        //var otherActor = col.collider.GetComponent<Actor>();
        Open();
    }

    void Open()
    {
        if (!open)
        {
            LeanTween.scaleX(left, 0.1f, 1).setEaseOutElastic();
            LeanTween.moveLocalX(left, -9, 1).setEaseOutElastic();
            LeanTween.scaleX(right, 0.1f, 1).setEaseOutElastic();
            LeanTween.moveLocalX(right, 9, 1).setEaseOutElastic();

            open = true;
        }
    }
    void Close()
    {
        if (open)
        {

        }
    }
    void Lock()
    {
        // turn off the colliders, or save state?
    }
}
