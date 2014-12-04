using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
    const string MARKERS_PARENT = "Markers";
    const string PLAYER_SPAWN = "playerSpawn";
    const string HDOOR = "door.horizontal.metal";
    const string VDOOR = "door.vertical.metal";

	void Start()
    {
        var mesh = GetComponentInChildren<MeshRenderer>();
        var bounds = mesh.bounds;

        // center the map in the world
        gameObject.transform.position = -bounds.center;
        bounds = mesh.bounds;

        // find the level objects and process them
        var markers = transform.FindChild(MARKERS_PARENT);
        if (markers != null)
        {
            foreach (Transform marker in markers.transform)
            {
                switch(marker.name) {
                    case PLAYER_SPAWN:
                        Main.Instance.game.playerSpawn = marker.position;
                        break;
                    case HDOOR:
                    case VDOOR:
                        break;
                }
            }
            GameObject.Destroy(markers.gameObject);
        }
        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}
}
