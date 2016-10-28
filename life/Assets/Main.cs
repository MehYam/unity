using UnityEngine;
using System.Collections;

using lifeEngine;
using System.Collections.Generic;
using life.util;

public class Main : MonoBehaviour
{
    public Sprite[] glyphs;
    public float outdoorTemperature = 0;

    void Start()
    {
        Instance = this;
        //Debug.Log("System.Environment.Version " + System.Environment.Version);

        var layer = Operations.LoadLayerFile("c:\\source\\cs\\life\\simplerooms2.txt");

        world = new World(layer);
        world.RecalculateRooms();
        world.thermodynamicsEnabled = false;

        RenderLayer(layer);

        // set some initial temperatures
        const float INDOOR_TEMP = 20;
        world.temps.Fill((x, y, tile) => world.IsOutside(x, y) ? world.outdoorTemperature : INDOOR_TEMP);

        Camera.main.transform.position = Camera.main.transform.position + (Vector3)(world.ground.size.ToVector2() / 2);
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
    static readonly Dictionary<char, int> s_glyphTypeToIndex = new Dictionary<char, int>()
    {
        //KAI: there's probably a way to indicate the character in the inspector panel, and do this all automatically, if it's worth it
        {' ', 5},
        {'#', 6},
        {'.', 3},
        {'h', 2},
        {'c', 1},
        {'t', 8}
    };
    void RenderLayer(Layer<Tile> layer)
    {
        tiles = new Layer<GameObject>(layer.size.x, layer.size.y);

        Vector2 offset = Vector2.zero; // use this to center the map on the origin: new Vector2(layer.size.x / 2, layer.size.y / 2);
        layer.ForEach((x, y, tile) =>
        {
            var go = InitSprite(
                string.Format("tile_{0:000}_{1:000}", x, y),
                glyphs[s_glyphTypeToIndex[tile.type]],
                Constants.SortingLayer.GROUND
            );
            go.transform.parent = gameObject.transform;
            go.transform.position = new Vector2(x, y) - offset;
            tiles.Set(x, y, go);
        });
    }
    void FixedUpdate()
    {
        world.outdoorTemperature = outdoorTemperature;
        world.Tick(Time.time, Time.deltaTime);

        foreach (var actor in world._actors)
        {
            var go = actorToGameObject[actor];
            go.transform.position = actor.pos.ToVector2();
        }
    }
    /// <summary>
    /// /////////////////////////////////////////////////////////////////////
    /// </summary>
    //KAI: This is an ad-hoc DOM for the app to control the model, and access GameObjects.  
    public static Main Instance { get; private set; }
    public World world { private set; get; }
    public Layer<GameObject> tiles { private set; get; }
    public readonly Dictionary<Actor, GameObject> actorToGameObject = new Dictionary<Actor, GameObject>();

    public Actor AddActor(char type, Point<float> pos)
    {
        var actor = new Actor(type);
        actor.pos = pos;

        world.AddActor(actor);

        var gameObj = InitSprite(type.ToString(), glyphs[s_glyphTypeToIndex[type]], Constants.SortingLayer.ACTORS);
        gameObj.transform.position = pos.ToVector2();

        actorToGameObject[actor] = gameObj;
        return actor;
    }
    public Actor selectedActor { get; set; }
}
