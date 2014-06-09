using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    public float health;
    public float collisionDamage;

    public float timeToLive = 0;
    public WorldObjectType worldObject;
    public IActorBehavior behavior { private get; set; }

    void Start()
    {
        if (timeToLive > 0)
        {
            timeToLive += Time.fixedTime;
        }
    }

    void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.FixedUpdate(this);
        }
        if (worldObject.maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > worldObject.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, worldObject.maxSpeed);
        }
        
        if (((timeToLive > 0) && Time.fixedTime > timeToLive) ||
            (health <= 0))
        {
            Main.Instance.game.HandleActorDeath(this);
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            Main.Instance.game.HandleCollision(contact);
        }
        //Debug.Log(collision.relativeVelocity.magnitude);
    }
}
