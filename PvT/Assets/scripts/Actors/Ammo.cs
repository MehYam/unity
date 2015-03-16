using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Ammo : MonoBehaviour
{
    public ActorType.Weapon weapon { private get; set; }

	// Use this for initialization
	void Start()
    {
        if (weapon == null)
        {
            Debug.LogError(string.Format("Bad design is biting you - you have to assign an ActorType.Weapon to the instance of {0}", name));
        }
        Main.Instance.ParentAmmo(transform);

        var actor = gameObject.GetComponent<Actor>();

        actor.SetExpiry(weapon.attrs.ttl);
        actor.collisionDamage = weapon.attrs.damage;
        actor.showsHealthBar = false;

        if (weapon.lit)
        {
            var renderer = actor.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = weapon.color;
            }
        }

        var rigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            rigidbody.drag = 0;
        }

        var type = Main.Instance.game.loader.GetActorType(weapon.actorName);
        if (type.attrs.acceleration == 0)
        {
            // give the ammo instant acceleration
            if (rigidbody != null)
            {
                rigidbody.mass = 0;
                rigidbody.velocity = Util.GetLookAtVector(actor.transform.rotation.eulerAngles.z) * type.attrs.maxSpeed;
            }
        }
        else
        {
            // treat the ammo like a vehicle (i.e. rocket)
            if (rigidbody != null)
            {
                rigidbody.mass = type.mass;
            }
            actor.behavior = ActorBehaviorFactory.Instance.thrust;
        }
        
        Main.Instance.game.PlaySound(actor, Sounds.ActorEvent.SPAWN);
        GlobalGameEvent.Instance.FireAmmoSpawned(actor, weapon);
	}

    GameObject _collisionParticles;
    void PreActorDie(Actor actor)
    {
        if (_collisionParticles == null)
        {
            _collisionParticles = ((GameObject)GameObject.Instantiate(Main.Instance.assets.collisionParticles));
            Main.Instance.ParentEffect(_collisionParticles.transform);
        }
        _collisionParticles.transform.position = transform.position;
        _collisionParticles.GetComponent<ParticleSystem>().Play();

        var expire = _collisionParticles.GetOrAddComponent<Expire>();
        expire.SetExpiry(_collisionParticles.GetComponent<ParticleSystem>().duration);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(string.Format("{0} colliding with {1}", name, collision.gameObject.name));

        var otherActor = collision.gameObject.GetComponent<Actor>();
        if (otherActor != null)
        {
            if (otherActor.reflectsAmmo)
            {
                // bounce off a shield => switch allegiance and damage the other side
                gameObject.layer = gameObject.layer == (int)Consts.CollisionLayer.MOB_AMMO ?
                    (int)Consts.CollisionLayer.FRIENDLY_AMMO :
                    (int)Consts.CollisionLayer.MOB_AMMO;

                // replace the "realistic" collision with one that looks better - otherwise lasers
                // look wonky
                var actor = GetComponent<Actor>();
                GetComponent<Rigidbody2D>().angularVelocity = 0;
                GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity.normalized * actor.actorType.attrs.maxSpeed;
                ActorBehaviorFactory.Instance.faceForward.FixedUpdate(actor);
            }
            else
            {
                collision.contacts[0].collider.gameObject.SendMessage(
                    "OnDamagingCollision", 
                    GetComponent<Actor>(), SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
