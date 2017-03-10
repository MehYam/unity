using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testDragAndDrop : MonoBehaviour
{
    public void OnDrop(string whatever)
    {
        Debug.LogFormat("OnDrop {0}", whatever);
    }
}
