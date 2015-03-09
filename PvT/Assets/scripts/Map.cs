using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.DOM;
using PvT.Util;

public sealed class Map : MonoBehaviour
{

    const string MARKERS_PARENT = "Markers";
    const string ROOM_MARKERS_PARENT = "RoomMarkers";

    public IList<ITarget> waypoints { get; private set; }
    public IList<GameObject> doors { get; private set; }
    
    readonly IList<XRect> _roomBoundaries = new List<XRect>();
	void Start()
    {
        // loop all the meshes in the level, take the bounds of the largest as our world bounds
        var meshes = GetComponentsInChildren<MeshRenderer>();
        MeshRenderer largestMesh = null;
        foreach (var mesh in meshes)
        {
            if (largestMesh == null || mesh.bounds.size.sqrMagnitude > largestMesh.bounds.size.sqrMagnitude)
            {
                largestMesh = mesh;
            }
        }

        DebugUtil.Assert(largestMesh != null, "Found no meshes in level");

        // center the map in the world
        gameObject.transform.position = -largestMesh.bounds.center;

        waypoints = new List<ITarget>();
        doors = new List<GameObject>();

        ProcessMarkers(transform.FindChild(MARKERS_PARENT));
        ProcessMarkers(transform.FindChild(ROOM_MARKERS_PARENT));

        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(largestMesh.bounds));

        StartCoroutine(TestRooms());
	}

    const string PLAYER_SPAWN_MARKER = "playerSpawn";
    const string LASER_GATE_MARKER = "laserGate";
    const string WAYPOINT_MARKER = "waypoint";
    const string ROOM_MARKER = "room";
    const string SWITCH_MARKER = "switch";
    void ProcessMarkers(Transform markerParent)
    {
        // find the level objects and process them
        if (markerParent != null)
        {
            foreach (Transform marker in markerParent.transform)
            {
                var markerNameParts = new Util.StringArrayParser(marker.name.Split('.'));
                switch (markerNameParts.NextString())
                {
                    case PLAYER_SPAWN_MARKER:
                        Main.Instance.game.playerSpawn = marker.position;
                        break;
                    case LASER_GATE_MARKER:
                        AddDoor(marker.gameObject, markerNameParts);
                        break;
                    case WAYPOINT_MARKER:
                        waypoints.Add(new StaticTarget(marker.position));
                        break;
                    case ROOM_MARKER:
                        AddRoom(marker.gameObject);
                        break;
                    case SWITCH_MARKER:
                        AddSwitch(marker.gameObject);
                        break;
                }
            }
            GameObject.Destroy(markerParent.gameObject);
        }
    }

    const float TILE_PIXELS = 128;
    const float TILE_PIXELS_PER_UNIT = 50;
    const float TILE_SIZE = TILE_PIXELS / TILE_PIXELS_PER_UNIT;
    const float LASER_WIDTH = 32 / TILE_PIXELS_PER_UNIT;

    const string VERTICAL = "vertical";
    const string HORIZONTAL = "horizontal";

    void AddDoor(GameObject marker, Util.StringArrayParser arguments)
    {
        bool horizontal = arguments.NextString() == HORIZONTAL;
        Color32 color = arguments.NextHexColor();
        int number = arguments.NextInt();

        // snap it to the tile boundaries
        var markerOffset = marker.transform.localPosition * transform.localScale.x;

        Vector2 tileIndex = new Vector2((int)(markerOffset.x / TILE_SIZE), (int)(markerOffset.y / TILE_SIZE));
        Vector2 space = new Vector2((TILE_SIZE - LASER_WIDTH) / Mathf.Max(1, number - 1), 0);
        Vector2 offsetToCenter = new Vector2(LASER_WIDTH, -TILE_SIZE) / 2;
        if (horizontal)
        {
            space = Util.SwapXY(space) * -1;
            offsetToCenter = Util.SwapXY(offsetToCenter) * -1;
        }
        var startingEdge = (Vector2)gameObject.transform.position + (tileIndex * TILE_SIZE);
        for (int i = 0; i < number; ++i)
        {
            var go = (GameObject)GameObject.Instantiate(Main.Instance.assets.laserGate);
            go.transform.position = startingEdge
                + offsetToCenter
                + (space * i);
            go.transform.parent = gameObject.transform; 

            var renderer = go.transform.GetChild(0).GetComponent<SpriteRenderer>();
            renderer.color = color;
            if (horizontal)
            {
                go.transform.Rotate(0, 0, 90);
            }
            doors.Add(go);
        }
    }

    void AddRoom(GameObject gameObject)
    {
        Debug.Log("AddRoom " + gameObject.name);
        var box = gameObject.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            var scaledSize = box.size / TILE_PIXELS_PER_UNIT;
            _roomBoundaries.Add(new XRect(
                gameObject.transform.position.x,
                gameObject.transform.position.y - scaledSize.y,
                gameObject.transform.position.x + scaledSize.x,
                gameObject.transform.position.y
                )
            );

            Debug.Log(_roomBoundaries[0]);
        }
    }

    void AddSwitch(GameObject gameObject)
    {
        gameObject.GetComponent<Collider2D>().isTrigger = true;
        gameObject.AddComponent<Switch>();

        gameObject.layer = (int)Consts.CollisionLayer.ENVIRONMENT;
        gameObject.transform.parent = transform;
    }

    IEnumerator TestRooms()
    {
        while(true)
        {
            var playerPos = Main.Instance.game.player.transform.position;
            yield return new WaitForSeconds(1);
        }
    }
}
