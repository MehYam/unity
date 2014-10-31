using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

public class DeathHopperAI : MonoBehaviour
{
	// Use this for initialization
	void Start()
    {
	    StartCoroutine(AI());
	}
	
    IEnumerator AI()
    {
        var game = Main.Instance.game;
        var actor = GetComponent<Actor>();
        while (true)
        {
            // 2. swivel until we're close to facing the actor
            while (!Util.IsLookingAt(actor.transform, game.player.transform.position, 30))
            {
                var angle = Util.GetLookAtAngle(actor.transform.position, game.player.transform.position);

                ICompletableActorBehavior swivel = new TweenRotationBehavior(angle, 0.5f);
                actor.behavior = swivel;

                yield return StartCoroutine(Util.YieldUntil(() => swivel.IsComplete(actor) ));
            }

            // 3. hop
            var hop = new HopBehavior(PlayerTarget.Instance);
            actor.behavior = hop;

            var lookAt = Util.GetLookAtVector(actor.transform.position, game.player.transform.position);
            actor.rigidbody2D.velocity = lookAt * actor.maxSpeed;

            yield return StartCoroutine(Util.YieldUntil(() => hop.IsComplete(actor) ));

            // 4. land with some fanfare and wait
            actor.gameObject.rigidbody2D.velocity = Vector2.zero;

            var impact = game.loader.GetMisc("landingImpact").ToRawGameObject(Consts.SortingLayer.TANKBODY);
            impact.transform.position = actor.gameObject.transform.position;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
