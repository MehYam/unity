using UnityEngine;
using System.Collections;

using lifeEngine;
using System.Collections.Generic;
using life.util;

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

        Camera.main.transform.position = Camera.main.transform.position + (Vector3)(world.map.size.ToVector2() / 2);

        Test_RenderActors();
    }
    void OnDestroy()
    {
        Instance = null;
        GlobalEvent.ReleaseAllListeners();
    }
    static GameObject InitSprite(string name, Sprite sprite, string sortLayer)
    {
        var retval = new GameObject(name);
        var renderer = retval.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = sortLayer;

        return retval;
    }
    void RenderLayer(Layer<Tile> layer)
    {
        tiles = new Layer<GameObject>(layer.size.x, layer.size.y);

        var glyphLookup = new Dictionary<char, int>();
        glyphLookup[' '] = 5;
        glyphLookup['#'] = 6;
        glyphLookup['.'] = 3;

        Vector2 offset = Vector2.zero; // use this to center the map on the origin: new Vector2(layer.size.x / 2, layer.size.y / 2);
        layer.ForEach((x, y, tile) =>
        {
            var go = InitSprite(
                string.Format("tile_{0:000}_{1:000}", x, y),
                glyphs[glyphLookup[tile.type]],
                Constants.SortingLayer.GROUND
            );
            go.transform.parent = gameObject.transform;
            go.transform.position = new Vector2(x, y) - offset;
            tiles.Set(x, y, go);
        });
    }
    void Test_RenderActors()
    {
        var human = new Actor('H');
        human.pos = new Point<float>(0, 0);

        var critter = new Actor('C');
        critter.pos = new Point<float>(world.map.size.x - 2, world.map.size.y - 1);

        world.AddActor(human);
        world.AddActor(critter);

        var goHuman = InitSprite("human", glyphs[2], Constants.SortingLayer.ACTORS);
        var goCritter = InitSprite("critter", glyphs[1], Constants.SortingLayer.ACTORS);

        goHuman.transform.position = human.pos.ToVector2();
        goCritter.transform.position = critter.pos.ToVector2();
    }
}
