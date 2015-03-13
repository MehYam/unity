using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Player : MonoBehaviour
{
	void Start()
    {
        Debug.Log("Player.Start");

        var actor = GetComponent<Actor>();
        if (actor != null)
        {
            gameObject.layer = (int)Consts.CollisionLayer.FRIENDLY;

            var bf = ActorBehaviorFactory.Instance;
            var behaviors  = new CompositeBehavior(bf.faceForward);

            if (actor.isHero)
            {
                behaviors.Add(bf.CreateHeroAnimator(gameObject));
                behaviors.Add(bf.heroRegen);
            }
            actor.behavior = behaviors;

            // enable any nested player controls
            gameObject.ForEachChildRecursive((go) =>
                {
                    var playerControl = go.GetComponent<PlayerControl>();
                    if (playerControl != null)
                    {
                        playerControl.enabled = true;
                    }
                }
            );

            GlobalGameEvent.Instance.FirePlayerSpawned(actor);
        }
        else
        {
            Debug.LogError("no Actor for Player");
        }
	}

    void PreActorDie(Actor actor)
    {
        //assert(actor == GetComponent<Actor>());
        if (!actor.isHero)
        {
            Debug.Log("Player respawning as HERO");

            // if we're possessing a mob and dying, we need to implement ejection of the hero
            // by respawning it as the player
            var game = Main.Instance.game;

            var newPlayer = game.SpawnPlayer(transform.position);
            newPlayer.AddComponent<PossessionUndoSequence>();
        }
    }

    void OnHerolingCollide(Heroling heroling)
    {
        Destroy(heroling.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var otherActor = collision.gameObject.GetComponent<Actor>();

        if (otherActor != null)
        {
            var mob = otherActor.GetComponent<Mob>();
            if (mob != null && mob.overwhelmPct >= 1)
            {
                // add the possession sequence script and let it run the possession
                if (gameObject.GetComponent<PossessionSequence>() == null)
                {
                    var sequence = gameObject.AddComponent<PossessionSequence>();
                    sequence.hostToPossess = otherActor;
                }
            }
            else
            {
                collision.gameObject.SendMessage("OnDamagingCollision", GetComponent<Actor>(), SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void OnDamagingCollision(Actor other)
    {
        var actor = GetComponent<Actor>();
        actor.GrantInvuln(Consts.POST_DAMAGE_INVULN);
    }
}
