using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class SketchupLevelCreator : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall;
    public GameObject cornerWall;

    public Vector2 size;
    public float spacer = 0;
    public void Generate()
    {
        Clear();

        int cols = (int)size.x;
        int rows = (int)size.y;

        var bounds = floor.GetComponent<Renderer>().bounds;
        Debug.Log("bounds: " + bounds + ", " + bounds.size);

        var tileMiddle = bounds.size / 2;
        var topLeft = tileMiddle - new Vector3(
            ((bounds.size.x * cols) + (spacer * (cols - 1))) / 2,
            0,
            ((bounds.size.z * rows) + (spacer * (rows - 1))) / 2
        );

        var parent = new GameObject("tiles");
        parent.transform.parent = transform;

        System.Action<int, int, GameObject> AddTile = (x, y, tile) =>
        {
            tile.name = string.Format("tile {0}x{1}", x, y);

            tile.transform.position = topLeft + new Vector3(x * (bounds.size.x + spacer), 0, y * (bounds.size.z + spacer));
            tile.transform.parent = parent.transform;
        };

        // do the interior
        for (var x = 1; x < (cols-1); ++x)
        {
            for (var y = 1; y < (rows-1); ++y)
            {
                AddTile(x, y, GameObject.Instantiate(floor));
            }
        }

        // corners
        var corner = GameObject.Instantiate(cornerWall);
        corner.transform.eulerAngles = new Vector3(0, 90, 0);
        AddTile(0, 0, corner);

        corner = GameObject.Instantiate(cornerWall);
        AddTile(rows - 1, 0, corner);

        corner = GameObject.Instantiate(cornerWall);
        corner.transform.eulerAngles = new Vector3(0, 180, 0);
        AddTile(0, cols - 1, corner);

        corner = GameObject.Instantiate(cornerWall);
        corner.transform.eulerAngles = new Vector3(0, 270, 0);
        AddTile(rows - 1, cols - 1, corner);

        // edges
        for (var y = 1; y < (cols - 1); ++y)
        {
            var edge = GameObject.Instantiate(wall);
            edge.transform.eulerAngles = new Vector3(0, 90, 0);
            AddTile(0, y, edge);

            edge = GameObject.Instantiate(wall);
            edge.transform.eulerAngles = new Vector3(0, 270, 0);
            AddTile(cols - 1, y, edge);
        }
        for (var x = 1; x < (rows - 1); ++x)
        {
            var edge = GameObject.Instantiate(wall);
            edge.transform.eulerAngles = new Vector3(0, 0, 0);
            AddTile(x, 0, edge);

            edge = GameObject.Instantiate(wall);
            edge.transform.eulerAngles = new Vector3(0, 180, 0);
            AddTile(x, rows - 1, edge);
        }
    }
    public void Clear()
    {
        if (transform.childCount > 0)
        {
            var obj = transform.GetChild(0).gameObject;
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
            Debug.Assert(transform.childCount == 0, "SketchupLevelEditor has too many children");
        }
    }
}
