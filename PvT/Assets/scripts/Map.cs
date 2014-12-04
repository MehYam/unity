using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
    const string MARKERS_PARENT = "Markers";
    const string PLAYER_SPAWN = "playerSpawn";
    const string DOOR = "door";
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
                var nameDesignation = marker.name.Split('.');
                switch (nameDesignation[0])
                {
                    case PLAYER_SPAWN:
                        Main.Instance.game.playerSpawn = marker.position;
                        break;
                    case DOOR:
                        BuildDoor(bounds, marker.gameObject, nameDesignation);
                        break;
                }
            }
            GameObject.Destroy(markers.gameObject);
        }
        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}

    const float TILE_PIXELS = 128;
    const float TILE_PIXELS_PER_UNIT = 50;
    const float TILE_SIZE = TILE_PIXELS / TILE_PIXELS_PER_UNIT;

    const string VERTICAL = "vertical";
    const string HORIZONTAL = "horizontal";
    void BuildDoor(Bounds bounds, GameObject marker, string[] arguments)
    {
        bool horizontal = arguments[1] == HORIZONTAL;

        // snap it to the tile boundaries
        var markerOffset = marker.transform.localPosition * transform.localScale.x;
        int tileX = (int)(markerOffset.x / TILE_SIZE);
        int tileY = (int)(markerOffset.y / TILE_SIZE);

        Debug.Log(string.Format("Door at {0} looks like it's in tile {1}, {2}", marker.transform.position, tileX, tileY));

        var go = (GameObject)GameObject.Instantiate(Main.Instance.assets.laserGateAqua);

        go.transform.position = 
            gameObject.transform.position
            + new Vector3(tileX * TILE_SIZE, tileY * TILE_SIZE)
            + (new Vector3(TILE_SIZE, -TILE_SIZE) / 2);

        go.transform.parent = gameObject.transform;

        if (horizontal)
        {
            go.transform.Rotate(0, 0, 90);
        }
    }
}
