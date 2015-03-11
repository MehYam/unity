using UnityEngine;
using System.Collections;

public sealed class Turret : MonoBehaviour
{
	void Start()
    {
        //KAI: not quite correct...
        var player = transform.parent.GetComponent<Player>();
        if (player != null)
        {
            var turretActor = GetComponent<Actor>();
            var bf = ActorBehaviorFactory.Instance;
            turretActor.behavior = new CompositeBehavior(
                bf.faceMouse,
                bf.CreatePlayerButton(MasterInput.impl.Primary,
                    bf.CreateAutofire(Consts.CollisionLayer.FRIENDLY_AMMO, turretActor.actorType.weapons))
            );
        }
	}
}
