using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_StartingRoom : MonoBehaviour
{
	void Start()
    {
        //KAI: left off here, trying to understand how to make the shield work correctly.
        // I think the thing to do is use a fixed joint.  To test this, add a player ship
        // instance and manually configure the fixed joint with the shield.  Modify the stuff
        // below to make this easy during development

        // Spawn player
		var player = GameObject.Instantiate(Main.game.defaultPlayerPrefab);
        player.transform.parent = Main.game.actorParent.transform;
        
        Main.game.player = player.GetComponent<Actor>();
	}
}
