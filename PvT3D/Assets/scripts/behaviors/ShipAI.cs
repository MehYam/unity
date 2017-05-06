using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    Actor _actor;
    void Start()
    {
        _actor = transform.parent.GetComponent<Actor>();
        StartCoroutine(ShipBehavior());
    }
    IEnumerator ShipBehavior()
    {
        yield return new WaitForSeconds(2); //KAI: because Main.game.player isn't ready yet - need to think about the proper solution to this

        _currentBehavior = new CompositeBehavior(
            new GravitateToTargetBehavior(Main.game.player.gameObject),
            //new FaceForwardBehavior()
            new FaceTargetBehavior(Main.game.player.gameObject)
        );
        yield return null;
    }

    IBehavior _currentBehavior;
    void FixedUpdate()
    {
        if (_currentBehavior != null && _actor != null)
        {
            _currentBehavior.FixedUpdate(_actor);
        }
    }
}
