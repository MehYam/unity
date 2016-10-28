using UnityEngine;
using System.Collections;

using lifeEngine;

public sealed class TestPathfind : MonoBehaviour
{
	void Start()
    {
        var ge = GlobalEvent.Instance;
        ge.ActorSelected += OnActorSelected;
        ge.ActorUnselected += OnActorUnselected;
        ge.TileSelected += OnTileSelected;
	}
    void OnDestroy()
    {
        var ge = GlobalEvent.Instance;
        ge.ActorSelected -= OnActorSelected;
        ge.TileSelected -= OnTileSelected;
    }
    Actor _selectedActor;
    void OnActorSelected(Actor actor)
    {
        _selectedActor = actor;
    }
    void OnActorUnselected(Actor actor)
    {
        _selectedActor = null;
    }
    void OnTileSelected(Point<int> tile)
    {
        if (_selectedActor != null)
        {
            _selectedActor.AddPriority(
                new lifeEngine.behavior.MoveTo(
                    Main.Instance.world,
                    _selectedActor,
                    tile));
        }
    }
}
