using UnityEngine;
using System.Collections;

using PvT3D.Util;

using Layer = kaiGameUtil.Layer<Tile>;
using Point = kaiGameUtil.Point<int>;

public struct Tile
{
    public static char EMPTY = (char)0;
    public static char[] types = { EMPTY, 'X', '_', '-', 'D' };

    public readonly char type;
    public Tile(char type)
    {
        this.type = type == ' ' ? EMPTY : type;  // this is wacky, but 'type' gets set to 0 in the default constructor case for structs;  so instead of ' ' and 0 both meaning empty, we canonicalize ' ' into 0
    }
    public bool Empty {  get { return type == EMPTY; } }
    public override string ToString()
    {
        return Empty ? " " : type.ToString();
    }
}
public sealed class TileLevelCreator : MonoBehaviour
{
    [SerializeField] GameObject floor = null;
    [SerializeField] GameObject wall = null;
    [SerializeField] GameObject cornerWall = null;
    [SerializeField] GameObject endWall = null;
    [SerializeField] GameObject door = null;

    // These will be drawn by the custom editor
    [HideInInspector] public TextAsset levelFile;
    [HideInInspector] public Vector2 size = new Vector2(5, 5);
    [HideInInspector] public int padding = 1;

    public void Generate(Vector2 size, float padding)
    {
        var layer = new Layer((int)size.x, (int)size.y);
        layer.Fill((x, y) => new Tile());
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
        parent.transform.localPosition = Vector3.zero;

        System.Action<int, int, GameObject, string> AddTile = (x, y, tile, name) =>
        {
            tile.name = string.Format("{0}x{1} {2}", x, y, name);

            tile.transform.parent = parent.transform;
            tile.transform.localPosition = topLeft + new Vector3(x * (bounds.size.x + padding), 0, y * (bounds.size.z + padding));
        };

        layer.ForEach((x, y, tile) =>
        {
            if (!tile.Empty)
            {
                AddTile(x, y, GameObject.Instantiate(floor), floor.name);

                foreach (var dir in kaiGameUtil.Util.cardinalDirections)
                {
                    var neighbor = kaiGameUtil.Util.Add(dir, new Point(x, y));
                    if (!layer.IsValid(neighbor) || layer.Get(neighbor).Empty)
                    {
                        // need to put up a wall, which is possibly a door
                        string tileName = null;
                        GameObject wallGO = null;
                        if (tile.type == 'D')
                        {
                            tileName = "door";
                            wallGO = GameObject.Instantiate(door);
                        }
                        else
                        {
                            tileName = "wall";
                            wallGO = GameObject.Instantiate(wall);
                        }
                        var angle = Util.Angle(kaiGameUtil.Util.down.ToVector2()) - Util.Angle(dir.ToVector2());

                        wallGO.transform.eulerAngles = new Vector3(0, angle, 0);
                        AddTile(x, y, wallGO, tileName);
                    }
                }
            }
        });
    }
    public void ParseAndGenerate(string textFile)
    {
        var lines = textFile.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        var height = lines.Length;
        var width = 0;
        foreach (var line in lines)
        {
            width = System.Math.Max(width, line.Length);
        }
        var retval = new Layer(width, height);

        for (int y = 0; y < height; ++y)
        {
            var line = lines[y];
            for (int x = 0; x < width; ++x)
            {
                var tile = x < line.Length ? new Tile(line[x]) : new Tile();
                retval.Set(new Point(x, height-y-1), tile);
            }
        }
        Debug.Log(retval);
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
