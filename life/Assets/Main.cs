using UnityEngine;
using System.Collections;

using lifeEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    public Sprite[] glyphs;

    // This is the app's DOM.  Think about hiding this behind interfaces, like in the PvT and PvT3D projects
    public static Main Instance { get; private set; }
    public World world { private set; get; }
    public Layer<GameObject> tiles { private set; get; }

    void Start()
    {
        Instance = this;
        //Debug.Log("System.Environment.Version " + System.Environment.Version);

        var layer = Operations.LoadLayerFile("c:\\source\\cs\\life\\simplerooms2.txt");

        world = new World(layer);
        RenderLayer(layer);
    }
    void OnDestroy()
    {
        Instance = null;
        GlobalEvent.ReleaseAllListeners();
    }
    void RenderLayer(Layer<Tile> layer)
    {
        tiles = new Layer<GameObject>(layer.size.x, layer.size.y);

        var glyphLookup = new Dictionary<char, int>();
        glyphLookup[' '] = 5;
        glyphLookup['#'] = 6;
        glyphLookup['.'] = 3;

        Vector2 offset = new Vector2(layer.size.x / 2, layer.size.y / 2);
        layer.ForEach((x, y, tile) =>
        {
            var sprite = glyphs[glyphLookup[tile.type]];

            var gobj = new GameObject(string.Format("tile_{0:000}_{1:000}", x, y));
            gobj.transform.parent = gameObject.transform;

            var renderer = gobj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = Constants.SortingLayer.GROUND;

            gobj.transform.position = new Vector2(x, y) - offset;

            tiles.Set(x, y, gobj);
        });
    }
}
