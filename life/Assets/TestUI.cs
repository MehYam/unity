using UnityEngine;
using System.Collections;

using lifeEngine;

public class TestUI : MonoBehaviour
{
	void Start()
    {
        GlobalEvent.Instance.TileMouseover += OnTileMouseover;
	}
    void OnDestroy()
    {
        GlobalEvent.Instance.TileMouseover -= OnTileMouseover;
    }
    void OnTileMouseover(Point<int> tile)
    {
        var world = Main.Instance.world;
    }
}
