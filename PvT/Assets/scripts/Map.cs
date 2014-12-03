using UnityEngine;
using System.Collections;

public sealed class Map : MonoBehaviour
{
	void Start()
    {
        var mesh = GetComponentInChildren<MeshRenderer>();
        var bounds = mesh.bounds;

        // center the map in the world
        gameObject.transform.position = -bounds.center;
        bounds = mesh.bounds;

        // find the player spawn marker
        var spawn = GameObject.FindWithTag(Consts.PLAYER_SPAWN_TAG);
        if (spawn != null)
        {
            Main.Instance.game.playerSpawn = spawn.transform.position;
            GameObject.Destroy(spawn);
        }

        GlobalGameEvent.Instance.FireMapReady(gameObject, new XRect(bounds.min, bounds.max));
	}
}
