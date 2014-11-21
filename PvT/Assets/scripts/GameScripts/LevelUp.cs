using UnityEngine;
using System.Collections;

using PvT.Util;

public class LevelUp : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
	    StartCoroutine(Sequence());
	}

    IEnumerator Sequence()
    {
        var main = Main.Instance;
        var actor = GetComponent<Actor>();

        actor.takenDamageMultiplier = 0;  // stop damage until animation's complete
        actor.health = actor.actorType.attrs.maxHealth;

        // 1. Attach the animation, wait a moment
        var flare = (GameObject)Instantiate(main.assets.flareAnimation);
        flare.transform.parent = transform;
        flare.transform.localPosition = Vector2.zero;
        flare.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        yield return new WaitForSeconds(Consts.FLARE_ANIMATION_PEAK_SECONDS);

        main.game.PlaySound(main.sounds.levelUp, transform.position);

        actor.takenDamageMultiplier = 1;

        yield return new WaitForSeconds(5);

        Destroy(this);
    }
}
