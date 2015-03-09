using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Ammo : MonoBehaviour
{
    public ActorType.Weapon weapon { get; set; }

	// Use this for initialization
	void Start()
    {
        if (Main.Instance.AmmoParent != null)
        {
            gameObject.transform.parent = Main.Instance.AmmoParent.transform;
        }

        var rigidbody = gameObject.GetOrAddComponent<Rigidbody2D>();
        rigidbody.drag = 0;

        var actor = gameObject.GetComponent<Actor>();
        actor.SetExpiry(weapon.attrs.ttl);
        actor.collisionDamage = weapon.attrs.damage;
        actor.showsHealthBar = false;
        actor.isAmmo = true;

        if (weapon.lit)
        {
            var renderer = actor.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = weapon.color;
            }
        }

        var type = Main.Instance.game.loader.GetActorType(weapon.actorName);
        if (type.attrs.acceleration == 0)
        {
            // give the ammo instant acceleration
            rigidbody.mass = 0;
            rigidbody.velocity = Util.GetLookAtVector(actor.transform.rotation.eulerAngles.z) * type.attrs.maxSpeed;
        }
        else
        {
            // treat the ammo like a vehicle (i.e. rocket)
            rigidbody.mass = type.mass;
            actor.behavior = ActorBehaviorFactory.Instance.thrust;
        }
        Debug.Log("Velocity: " + rigidbody.velocity);

        GlobalGameEvent.Instance.FireAmmoSpawned(actor, weapon);
	}
}
