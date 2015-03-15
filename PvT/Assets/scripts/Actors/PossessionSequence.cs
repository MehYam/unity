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

            // KAI: lame
            playerActor.SetExpiry(0.5f);
            playerActor.behaviorEnabled = false;
            Util.ForEachChildRecursive(gameObject, (go) =>
                {
                    var actor = go.GetComponent<Actor>();
                    if (actor != null)
                    {
                        actor.behaviorEnabled = false;
                    }
                    var playerControl = go.GetComponent<PlayerControl>();
                    if (playerControl != null)
                    {
                        playerControl.enabled = false;
                    }
                }
            );
            playerActor.gameObject.layer = (int)Consts.CollisionLayer.MOB;

            // spawn player is asynchronous, so just attach a new possession sequence to it that fires up when it's ready
            var newPlayer = Main.Instance.game.SpawnPlayer(playerActor.transform.position);
            var newSequence = newPlayer.AddComponent<PossessionSequence>();
            newSequence.hostToPossess = hostToPossess;
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
