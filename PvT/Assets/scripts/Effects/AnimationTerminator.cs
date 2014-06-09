using UnityEngine;
using System.Collections;

public class AnimationTerminator : MonoBehaviour
{
    public GameObject TargetToRemove;
    void OnComplete()
    {
        if (TargetToRemove == null)
        {
            TargetToRemove = gameObject;
        }
        GameObject.Destroy(TargetToRemove);
    }
}
