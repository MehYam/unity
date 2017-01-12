using UnityEngine;
using System.Collections;

using lifeEngine;

public class Tile
{
    public static char[] types = { 'X', '_' };

    public readonly char type;
    public Tile(char type)
    {
        this.type = type;
    }
    public bool Empty {  get { return type != 'X' && type != '_'; } }
    public override string ToString()
    {
        return type.ToString();
    }
}

public sealed class TileLevelCreator : MonoBehaviour
{
    public GameObject floor;
    public GameObject wall;
    public GameObject cornerWall;
    public GameObject endWall;

    public void Generate(Vector2 size, float padding)
    {
        Clear();

        int cols = (int)size.x;
        int rows = (int)size.y;

        var bounds = floor.GetComponent<Renderer>().bounds;
        Debug.Log("bounds: " + bounds + ", " + bounds.size);

        var tileMiddle = bounds.size / 2;
        var topLeft = tileMiddle - new Vector3(
            ((bounds.size.x * cols) + (padding * (cols - 1))) / 2,
            0,
            ((bounds.size.z * rows) + (padding * (rows - 1))) / 2
        );

        var parent = new GameObject("tiles");
        parent.transform.parent = transform;

        System.Action<int, int, GameObject> AddTile = (x, y, tile) =>
        {
            tile.name = string.Format("tile {0}x{1}", x, y);

            tile.transform.position = topLeft + new Vector3(x * (bounds.size.x + padding), 0, y * (bounds.size.z + padding));
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
    static int CountNeighbors(Layer<Tile> layer, Point<int> tile)
    {
        int retval = 0;
        foreach (var direction in Util.cardinalDirections)
        {
            var neighbor = Util.Add(tile, direction);
            if (layer.IsValid(neighbor) && !layer.Get(neighbor).Empty)
            {
                ++retval;
            }
        }
        return retval;
    }
    static Point<int> GetNeighborDirection(Layer<Tile> layer, Point<int> tile, System.Func<Point<int>, bool> matchNeighbor)
    {
        foreach (var direction in Util.cardinalDirections)
        {
            var neighbor = Util.Add(tile, direction);
            if (matchNeighbor(neighbor))
            {
                return neighbor;
            }
        }
        return Util.zero;
    }
    public void Generate(Layer<Tile> layer, float padding)
    {
        //KAI: copypasta from above
        Clear();

        var bounds = floor.GetComponent<Renderer>().bounds;
        Debug.Log("bounds: " + bounds + ", " + bounds.size);

        var tileMiddle = bounds.size / 2;
        var topLeft = tileMiddle - new Vector3(
            ((bounds.size.x * layer.size.x) + (padding * (layer.size.x - 1))) / 2,
            0,
            ((bounds.size.z * layer.size.y) + (padding * (layer.size.y - 1))) / 2
        );

        var parent = new GameObject("tiles");
        parent.transform.parent = transform;

        System.Action<int, int, GameObject> AddTile = (x, y, tile) =>
        {
            tile.name = string.Format("tile {0}x{1}", x, y);

            tile.transform.position = topLeft + new Vector3(x * (bounds.size.x + padding), 0, y * (bounds.size.z + padding));
            tile.transform.parent = parent.transform;
        };

        layer.ForEach((x, y, tile) =>
        {
            var pos = new Point<int>(x, y);
            switch (tile.type)
            {
                case '_':
                    AddTile(x, y, GameObject.Instantiate(floor));
                    break;
                case 'X':
                    var neighbors = CountNeighbors(layer, pos);
                    GameObject tileGeometry = null;
                    switch (neighbors)
                    {
                        case 1:
                            tileGeometry = GameObject.Instantiate(endWall);
                            break;
                        case 2:
                            tileGeometry = GameObject.Instantiate(cornerWall);
                            break;
                        case 3:
                            tileGeometry = GameObject.Instantiate(wall);
                            var emptyNeighborPos = GetNeighborDirection(layer, pos, p => !layer.IsValid(p) || layer.Get(p).Empty);
                            break;
                    }
                    if (tileGeometry != null)
                    {
                        AddTile(x, y, tileGeometry);
                    }
                    break;
            }
        });
    }
    public void ParseAndGenerate(string textFile)
    {
        var lines = textFile.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        var height = 0;

        var width = lines[0].Length;
        foreach (var line in lines)
        {
            if (line.Length == width)
            {
                ++height;
            }
        }
        var retval = new Layer<Tile>(width, height);

        for (int y = 0; y < height; ++y)
        {
            var line = lines[y];
            if (line.Length == width)
            {
                for (int x = 0; x < line.Length; ++x)
                {
                    retval.Set(new Point<int>(x, height-y-1), new Tile(line[x]));
                }
            }
        }
        Generate(retval, 1);
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
