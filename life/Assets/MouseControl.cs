using UnityEngine;
using System.Collections;

public sealed class MouseControl : MonoBehaviour
{
    [Range(5, 150)]
    public float zoomSensitivity = 50;
    const float BASE_ZOOM_SENSITIVITY = 100;

    Vector3 startPosition;
    Vector3 origin;
    Vector3 offset;
    bool dragging = false;

    void Start()
    {
        startPosition = Camera.main.transform.position;
    }

    lifeEngine.Point<int> _lastTile = lifeEngine.Util.zero;
    void LateUpdate()
    {
        var mouse = Input.mousePosition;
        var mouseInWorld = Camera.main.ScreenToWorldPoint(mouse);

        // dragging position with middle wheel
        if (Input.GetMouseButton(2))
        {
            offset = mouseInWorld - Camera.main.transform.position;
            if (!dragging)
            {
                dragging = true;
                origin = mouseInWorld;
            }
        }
        else
        {
            dragging = false;
        }
        if (dragging)
        {
            Camera.main.transform.position = origin - offset;
        }

        // zoom with mousewheel
        Camera.main.orthographicSize -= Input.mouseScrollDelta.y * zoomSensitivity / BASE_ZOOM_SENSITIVITY;

        // map current mouse position to tile
        //KAI: should only do this when mouse moves, but it's no big deal
        var coords = new lifeEngine.Point<int>(Mathf.RoundToInt(mouseInWorld.x), Mathf.RoundToInt(mouseInWorld.y));
        var world = Main.Instance.world;
        var center = lifeEngine.Util.Divide(world.map.size, 2);
        var tile = lifeEngine.Util.Add(coords, center);

        if (lifeEngine.Util.Within(tile, lifeEngine.Util.zero, world.map.size))
        {
            if (_lastTile != tile)
            {
                Debug.Log("tile: " + tile);
                GlobalEvent.Instance.FireTileMouseover(tile);
            }
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("click: " + tile);
                GlobalEvent.Instance.FireTileClick(tile);
            }
            _lastTile = tile;
        }
    }
}
