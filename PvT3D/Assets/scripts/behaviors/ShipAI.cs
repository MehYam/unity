using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PvT3D.WeaponComponents;

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

        var player = Main.game.player.gameObject;
        var facePlayer = new FaceTargetBehavior(player);

        for (int i = 0; i < 10; ++i)
        {
            // strafe/follow player
            _currentBehavior = new CompositeBehavior(
                new GravitateToTargetBehavior(player),
                //new FaceForwardBehavior()
                facePlayer
            );

            yield return new WaitForSeconds(5);

            // fire at player
            var schematic = transform.parent.GetComponent<WeaponSchematic>();

            Debug.AssertFormat(schematic != null, "schematic's not found for {0}", transform.parent.name);

            var fire = new FireAtTargetBehavior(player, schematic);
            _currentBehavior = new CompositeBehavior(
                fire,
                facePlayer
            );

            fire.firing = true;

            yield return new WaitForSeconds(5);
        }
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
