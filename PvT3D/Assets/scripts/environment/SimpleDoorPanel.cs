using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDoorPanel : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        //KAI: there must be a better way to do this
        transform.parent.SendMessage("OnCollisionEnter", col);
    }
}
