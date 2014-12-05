﻿using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Map : MonoBehaviour
{
    const string MARKERS_PARENT = "Markers";
    const string PLAYER_SPAWN = "playerSpawn";
    const string LASER_GATE = "laserGate";
	void Start()
    {
        var mesh = GetComponentInChildren<MeshRenderer>();
        var bounds = mesh.bounds;

        // center the map in the world
        gameObject.transform.position = -bounds.center;

        // find the level objects and process them
        var markers = transform.FindChild(MARKERS_PARENT);
        if (markers != null)
        {
            foreach (Transform marker in markers.transform)
            {
                var mapObject = new Util.StringArrayParser(marker.name.Split('.'));
                switch (mapObject.NextString())
                {
                    case PLAYER_SPAWN:
                        Main.Instance.game.playerSpawn = marker.position;
                        break;
                    case LASER_GATE:
                        BuildDoor(marker.gameObject, mapObject);
                        break;
                }
            }
            GameObject.Destroy(markers.gameObject);
        }
        bounds = mesh.bounds;
        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}

    const float TILE_PIXELS = 128;
    const float TILE_PIXELS_PER_UNIT = 50;
    const float TILE_SIZE = TILE_PIXELS / TILE_PIXELS_PER_UNIT;
    const float LASER_WIDTH = 32 / TILE_PIXELS_PER_UNIT;

    const string VERTICAL = "vertical";
    const string HORIZONTAL = "horizontal";

    void BuildDoor(GameObject marker, Util.StringArrayParser arguments)
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
        }
    }
}
