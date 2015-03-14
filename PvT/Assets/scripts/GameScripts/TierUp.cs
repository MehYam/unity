using UnityEngine;
using System.Collections;

using PvT.Util;

public class TierUp : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        StartCoroutine(Sequence());
    }

    public ActorType levelTo { private get; set; }
    IEnumerator Sequence()
    {
        var main = Main.Instance;
        var actor = GetComponent<Actor>();

        actor.takenDamageMultiplier = 0;  // stop damage - level up completely heals a player
        actor.health = actor.attrs.maxHealth;  // swapping ships fully heals the player, but it looks better to do this before Consts.FLARE_ANIMATION_PEAK_SECONDS

        DebugUtil.Assert(levelTo != null, "LevelUp doesn't know what to level to");

        // 1. Attach the animation, wait a moment
        var flare = (GameObject)Instantiate(main.assets.flareAnimation);
        flare.transform.position = transform.position;

        yield return new WaitForSeconds(Consts.FLARE_ANIMATION_PEAK_SECONDS);
        flare.GetComponent<ParticleSystem>().Play();

        main.game.PlaySound(Sounds.GlobalEvent.LEVELUP, transform.position);

        // 2. Spawn the new player, inherit the physics and positioning of the previous one
        //KAI: here's a weakness - spawning an actor shouldn't be that different from spawning the player (they shouldn't necessarily have two different calls)
        main.game.SpawnPlayer(transform.position, levelTo.name);

        var player = main.game.player;
        player.transform.rotation = transform.rotation;
        player.GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity;

        // 3. nuke old player
        Destroy(gameObject);
    }
}
