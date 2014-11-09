using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class DeathHopperAI : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
	    StartCoroutine(HopSequence());
        GlobalGameEvent.Instance.PossessionStart += OnPossessionStart;
	}
	void OnDestroy()
    {
        GlobalGameEvent.Instance.PossessionStart -= OnPossessionStart;
    }
    void OnPossessionStart(Actor host)
    {
        if (host.gameObject == this.gameObject)
        {
            DestroyObject(this);
        }
    }
    IEnumerator HopSequence()
    {
        var game = Main.Instance.game;
        var actor = GetComponent<Actor>();
        while (true)
        {
            // 2. swivel until we're close to facing the actor
            while (!Util.IsLookingAt(actor.transform, game.player.transform.position, 30))
            {
                var angle = Util.GetLookAtAngle(actor.transform.position, game.player.transform.position);

                ICompletableActorBehavior swivel = new TweenRotationBehavior(angle, 0.25f);
                actor.behavior = swivel;

                yield return StartCoroutine(Util.YieldUntil(() => swivel.IsComplete(actor) ));

                actor.behavior = null;
            }

            // 3. hop
            var hop = gameObject.GetComponent<HopBehavior>();
            if (hop == null)
            {
                hop = gameObject.AddComponent<HopBehavior>();
            }
            hop.Hop(Consts.CollisionLayer.MOB_AMMO);

            var lookAt = Util.GetLookAtVector(actor.transform.position, game.player.transform.position);
            actor.rigidbody2D.velocity = lookAt * actor.maxSpeed;

            yield return StartCoroutine(Util.YieldUntil(() => hop.complete ));

            // let the end of the hop vibrate for a bit
            yield return new WaitForSeconds(0.25f);
        }
    }
}
