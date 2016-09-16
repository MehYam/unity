using UnityEngine;
using System.Collections;

using lifeEngine;

public sealed class Test : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        GlobalEvent.Instance.TileMousedown += OnTileMousedown;
        GlobalEvent.Instance.TileMouseover += OnTileMouseover;
        GlobalEvent.Instance.TileMouseout += OnTileMouseout;
        GlobalEvent.Instance.TileMouseup += OnTileMouseover;  //KAI: because at this point we're still hovered, but should the caller assume this?
    }
    void OnDestroy()
    {
        GlobalEvent.Instance.TileMousedown -= OnTileMousedown;
        GlobalEvent.Instance.TileMouseover -= OnTileMouseover;
        GlobalEvent.Instance.TileMouseout -= OnTileMouseout;
        GlobalEvent.Instance.TileMouseup -= OnTileMouseover;
    }
    void OnTileMousedown(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.red;
    }
    void OnTileMouseover(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.yellow;
    }
    void OnTileMouseout(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.white;
    }
}
