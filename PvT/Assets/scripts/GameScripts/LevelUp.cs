using UnityEngine;
using System.Collections;

using PvT.Util;

public class LevelUp : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
	    StartCoroutine(LevelUpSequence());
	}

    IEnumerator LevelUpSequence()
    {
        var main = Main.Instance;
        var actor = GetComponent<Actor>();
        actor.takenDamageMultiplier = 0;  // stop damage - level up completely heals a player

        DebugUtil.Assert(!string.IsNullOrEmpty(actor.actorType.upgradesTo), "Trying to level up an actor that can't");

        // 1. Attach the animation, wait a moment
        var flare = (GameObject)Instantiate(Main.Instance.assets.flareAnimation);
        flare.transform.parent = transform;
        flare.transform.localPosition = Vector2.zero;

        yield return new WaitForSeconds(Consts.FLARE_ANIMATION_PEAK_SECONDS);

        // 2. Spawn the new player, inherit the physics and positioning of the previous one
        //KAI: here's a weakness - spawning an actor shouldn't be that different from spawning the player (they shouldn't necessarily have two different calls)
        main.game.SpawnPlayer(transform.position, actor.actorType.upgradesTo);

        var player = main.game.player;
        player.transform.rotation = transform.rotation;
        player.rigidbody2D.velocity = rigidbody2D.velocity;

        flare.transform.parent = player.transform;
        flare.transform.localPosition = Vector2.zero;

        // 3. nuke old player
        Destroy(gameObject);
    }
}
