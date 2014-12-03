using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
    static readonly string MARKERS_PARENT = "Markers";
    static readonly string PLAYER_SPAWN = "playerSpawn";
	void Start()
    {
        var mesh = GetComponentInChildren<MeshRenderer>();
        var bounds = mesh.bounds;

        // center the map in the world
        gameObject.transform.position = -bounds.center;
        bounds = mesh.bounds;

        // find the player spawn marker
        var markers = transform.FindChild(MARKERS_PARENT);
        if (markers != null)
        {
            var spawn = markers.FindChild(PLAYER_SPAWN);
            if (spawn != null)
            {
                Main.Instance.game.playerSpawn = spawn.transform.position;
            }
            GameObject.Destroy(markers.gameObject);
        }

        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}
}
