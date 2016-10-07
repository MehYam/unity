using UnityEngine;
using System.Collections;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float distance;

	void LateUpdate()
    {
        var pos = target.transform.position;

        pos.y += distance;
        transform.position = pos;

        transform.LookAt(target.transform);
	}
}
