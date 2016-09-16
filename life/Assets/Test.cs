using UnityEngine;
using System.Collections;

using lifeEngine;

public sealed class Test : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        GlobalEvent.Instance.TileClick += OnTileClick;
        GlobalEvent.Instance.TileMouseover += OnTileMouseover;
    }
    void OnDestroy()
    {
        GlobalEvent.Instance.TileClick -= OnTileClick;
        GlobalEvent.Instance.TileMouseover -= OnTileMouseover;
    }
    void OnTileClick(Point<int> tile)
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
}
