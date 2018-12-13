using UnityEngine;
using System.Collections;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float distance;

	void LateUpdate()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }
        var pos = target.transform.position;

        pos.y += distance;
        pos.z -= 20;
        transform.position = pos;

        transform.LookAt(target.transform);
	}
}
