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

        Consts.Log("local {0}, world {1}", transform.localPosition, transform.position);
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
        }
        GameState.Instance.HandleCollision(transform.localPosition);
        Debug.Log(collision.relativeVelocity.magnitude);
    }
}
