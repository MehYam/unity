using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class PossessionSequence : MonoBehaviour
{
    public Actor hostToPossess;

	// Use this for initialization
	void Start()
    {
        DebugUtil.Assert(hostToPossess != null, "because MonoBehavior can't have constructors, you must set PossessionSequence.hostToPossess manually");

        var playerActor = GetComponent<Actor>();
        if (playerActor.isHero)
        {
            StartCoroutine(Sequence(hostToPossess));
        }
        else
        {
            // already possessing, eject from the previous possessee first, then possess the new one
            var prevHost = playerActor;

            Main.Instance.game.SpawnPlayer(playerActor.transform.position);

            prevHost.behavior = null;
            prevHost.gameObject.layer = (int)Consts.CollisionLayer.MOB;

            StartCoroutine(Sequence(hostToPossess));
        }
	}

    IEnumerator Sequence(Actor host)
    {
        GlobalGameEvent.Instance.FirePossessionInitiated(host);

        var game = Main.Instance.game;
        game.PlaySound(Sounds.GlobalEvent.POSSESSION, transform.position);

        // 1. Stop all activity and pause
        var timeScale = Time.timeScale;
        Time.timeScale = 0;

        // 2. Remove physics from the hero, pause for a minute
        Util.DisablePhysics(gameObject);

        // 3. Tween it to the host
        const float clipLength = 2f;
        var start = transform.position;
        var endTime = Time.realtimeSinceStartup + clipLength;
        var sprite = GetComponent<SpriteRenderer>();
        while (Time.realtimeSinceStartup < endTime)
        {
            var progress = 1 - (endTime - Time.realtimeSinceStartup) / clipLength;
            var lerped = Vector3.Lerp(start, host.transform.position, progress);
            transform.position = lerped;

            var color = sprite.color;
            color.a = 1 - progress;
            sprite.color = color;

            yield return new WaitForEndOfFrame();
        }

        // 4. Host becomes the new player
        Destroy(host.GetComponent<Mob>());
        host.gameObject.AddComponent<Player>();

        Heroling.ReturnAll();

        // 6. Resume all activity
        Time.timeScale = timeScale;
        Destroy(gameObject);

        host.GrantInvuln(Consts.POST_POSSESSION_INVULN);

        GlobalGameEvent.Instance.FirePossessionComplete(GetComponent<Actor>());
        GlobalGameEvent.Instance.FireMobDeath(host);

        Destroy(this);

        yield return null;
    }
}
