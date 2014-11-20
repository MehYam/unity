using UnityEngine;
using System.Collections;

public sealed class AntiRotate : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
