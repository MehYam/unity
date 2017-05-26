using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_StartingRoom : MonoBehaviour
{
	void Start()
    {
        // Spawn player - check for a test player already dropped into the scene
        var playerMovement = FindObjectOfType<PlayerMovement>();

		var player = playerMovement == null ? GameObject.Instantiate(Main.game.defaultPlayerPrefab) : playerMovement.gameObject;
        player.transform.parent = Main.game.actorParent.transform;

        Main.game.player = player.GetComponent<Actor>();
	}
}
