using UnityEngine;
using System.Collections;

public sealed class Turret : MonoBehaviour
{
	void Start()
    {
        //KAI: player logic doesn't belong here
        var player = transform.parent.GetComponent<Player>();
        if (player != null)
        {
            var turretActor = GetComponent<Actor>();
            var bf = ActorBehaviorFactory.Instance;
            var behaviors = new CompositeBehavior();

            var fireAhead = bf.CreateAutofire(Consts.CollisionLayer.FRIENDLY_AMMO, turretActor.actorType.weapons);
            behaviors.Add(bf.faceMouse);
            behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Primary, fireAhead));

            turretActor.behavior = behaviors;
        }
	}
}
