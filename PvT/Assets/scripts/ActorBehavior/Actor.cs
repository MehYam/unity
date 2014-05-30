using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    public Vehicle vehicle;
    public IActorBehavior behavior { private get; set; }

    public Actor(Vehicle vehicle) { this.vehicle = vehicle; }

    void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.FixedUpdate(this);
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
