using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_StartingRoom : MonoBehaviour
{
	void Start()
    {
        // Spawn player
		var player = GameObject.Instantiate(Main.game.playerPrefab);
        player.transform.parent = Main.game.actorParent.transform;
        
        Main.game.player = player.GetComponent<Actor>();
	}
}
