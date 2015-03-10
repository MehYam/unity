using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class Player : MonoBehaviour
{
	// Use this for initialization
    Actor _actor;
	void Start()
    {
        Debug.Log("Player.Start");

        _actor = GetComponent<Actor>();
        if (_actor != null)
        {
            gameObject.layer = (int)Consts.CollisionLayer.FRIENDLY;

            CreateBehaviors(_actor);
            GlobalGameEvent.Instance.FirePlayerSpawned(_actor);
        }
        else
        {
            Debug.LogError("no Actor for Player");
        }
	}

    static void CreateBehaviors(Actor actor)
    {
        // set up the primary and secondary fire buttons
        var bf = ActorBehaviorFactory.Instance;
        var layer = Consts.CollisionLayer.HEROLINGS;

        var behaviors = new CompositeBehavior();

        var fireAhead = bf.CreateAutofire(layer, actor.actorType.weapons);
        behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.PrimaryAlt, fireAhead));

        // hero doesn't point to the mouse when firing
        behaviors.Add(bf.CreatePlayerButton(MasterInput.impl.Primary, new CompositeBehavior(bf.faceMouse, fireAhead)));

        actor.behavior = behaviors;
    }

    public void FixedUpdate()
    {
        if (_actor != null)
        {
            var current = MasterInput.impl.CurrentMovementVector;
            if (current != Vector2.zero)
            {
                if (_actor.thrustEnabled)
                {
                    _actor.AddThrust(current * _actor.attrs.acceleration);
                }
            }
            ActorBehaviorFactory.Instance.faceForward.FixedUpdate(_actor);
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
                collision.gameObject.SendMessage("OnDamagingCollision", GetComponent<Actor>());
            }
        }
    }
}
