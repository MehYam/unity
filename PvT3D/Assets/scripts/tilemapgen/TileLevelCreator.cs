using UnityEngine;
using System.Collections;

using PvT3D.Util;

using Layer = lifeEngine.Layer<Tile>;
using Point = lifeEngine.Point<int>;

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
        var layer = new Layer((int)size.x, (int)size.y);
        layer.Fill((x, y) => new Tile('_'));
        Generate(layer, padding);
    }
    public void Generate(Layer layer, float padding)
    {
        Clear();

        var bounds = floor.GetComponent<Renderer>().bounds;
        Debug.Log("Tile bounds: " + bounds + ", " + bounds.size);

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
            if (tile.type == '_')
            {
                AddTile(x, y, GameObject.Instantiate(floor));

                foreach (var dir in lifeEngine.Util.cardinalDirections)
                {
                    var neighbor = lifeEngine.Util.Add(dir, new Point(x, y));
                    if (!layer.IsValid(neighbor) || layer.Get(neighbor).Empty)
                    {
                        // need to put up a wall
                        var wallGO = GameObject.Instantiate(wall);
                        var angle = Util.Angle(lifeEngine.Util.down.ToVector2()) - Util.Angle(dir.ToVector2());

                        wallGO.transform.eulerAngles = new Vector3(0, angle, 0);
                        AddTile(x, y, wallGO);
                    }
                }
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
        var retval = new Layer(width, height);

        for (int y = 0; y < height; ++y)
        {
            var line = lines[y];
            if (line.Length == width)
            {
                for (int x = 0; x < line.Length; ++x)
                {
                    retval.Set(new Point(x, height-y-1), new Tile(line[x]));
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
