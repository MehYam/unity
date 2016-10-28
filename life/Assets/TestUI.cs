using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using lifeEngine;

public class TestUI : MonoBehaviour
{
    public Text debugText;
	void Start()
    {
        GlobalEvent.Instance.TileMouseover += OnTileMouseover;
	}
    void OnDestroy()
    {
        GlobalEvent.Instance.TileMouseover -= OnTileMouseover;
    }
    Point<int> _lastTile;
    void OnTileMouseover(Point<int> tile)
    {
        _lastTile = tile;
    }
    void Update()
    {
        var world = Main.Instance.world;

        var temp = world.temps.Get(_lastTile);
        var room = world.rooms.Get(_lastTile);

        debugText.text = string.Format("Tile: {0}\n{1:0.00}C\nIsOutside: {2}\nRoom Id: {4}\nIsRoom: {5} \nWall: {3}",
            _lastTile,
            temp,
            world.IsOutside(_lastTile.x, _lastTile.y),
            world.walls.Get(_lastTile) != null,
            world.rooms.Get(_lastTile),
            world.IsRoom(_lastTile.x, _lastTile.y)
            );
    }
}
