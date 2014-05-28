using UnityEngine;
using System.Collections;

public class ActorBehaviorHost : MonoBehaviour
{
    public IActorBehavior behavior;
	
	void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.FixedUpdate(gameObject);
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
            Main.Instance.gameState.HandleCollision(contact);
        }
        //Debug.Log(collision.relativeVelocity.magnitude);
    }
}
