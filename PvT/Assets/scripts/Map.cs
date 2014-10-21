using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
	void Start()
    {
        var mesh = GetComponentInChildren<MeshRenderer>();
        var bounds = mesh.bounds;

        // center the map
        gameObject.transform.position = -bounds.center;
        bounds = mesh.bounds;

        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}
}
