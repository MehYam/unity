using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject Target;

    float _preserveCameraZ;
    void Start()
    {
        _preserveCameraZ = camera.transform.localPosition.z;
    }

    void LateUpdate()
    {
        if (Target != null)
        {
            var home = Target.transform.localPosition;
            home.z = _preserveCameraZ;
            
            transform.localPosition = home;
        }
    }
}
