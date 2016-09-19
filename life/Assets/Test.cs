using UnityEngine;
using System.Collections;

using lifeEngine;

public sealed class Test : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
        var ge = GlobalEvent.Instance;
        ge.TileMousedown += OnTileMousedown;
        ge.TileMouseover += OnTileMouseover;
        ge.TileMouseout += OnTileMouseout;
        ge.TileMouseup += OnTileMouseover;  //KAI: because at this point we're still hovered, but should the caller assume this?

        ge.TileSelected += OnTileSelected;
        ge.TileUnselected += OnTileUnselected;
        ge.ActorSelected += OnActorSelected;
        ge.ActorUnselected += OnActorUnselected;
    }
    void OnDestroy()
    {
        var ge = GlobalEvent.Instance;

        ge.TileMousedown -= OnTileMousedown;
        ge.TileMouseover -= OnTileMouseover;
        ge.TileMouseout -= OnTileMouseout;
        ge.TileMouseup -= OnTileMouseover;

        ge.TileSelected -= OnTileSelected;
        ge.TileUnselected -= OnTileUnselected;
        ge.ActorSelected -= OnActorSelected;
        ge.ActorUnselected -= OnActorUnselected;
    }

    Actor _selectedActor = null;
    Point<int>? _selectedTile = null;
    void OnTileMousedown(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.red;

        var actor = main.world.FindActor(a => a.pos.ToInt() == tile);
        if (actor != null)
        {
            if (_selectedActor != null)
            {
                GlobalEvent.Instance.FireActorUnselected(_selectedActor);
            }
            _selectedActor = actor;
            GlobalEvent.Instance.FireActorSelected(_selectedActor);
        }
        else
        {
            if (_selectedTile != null)
            {
                GlobalEvent.Instance.FireTileUnselected(_selectedTile.Value);
            }
            _selectedTile = tile;
            GlobalEvent.Instance.FireTileSelected(_selectedTile.Value);
        }
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
    void OnTileSelected(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.red;
    }
    void OnTileUnselected(Point<int> tile)
    {
        var main = Main.Instance;
        var gobj = main.tiles.Get(tile);

        var renderer = gobj.GetComponent<SpriteRenderer>();
        renderer.color = Color.yellow;
    }
    void OnActorSelected(Actor actor)
    {
        var go = Main.Instance.actorToGameObject[actor];
        if (go != null)
        {
            go.GetComponent<SpriteRenderer>().color = Color.magenta;
        }
    }
    void OnActorUnselected(Actor actor)
    {
        var go = Main.Instance.actorToGameObject[actor];
        if (go != null)
        {
            go.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
