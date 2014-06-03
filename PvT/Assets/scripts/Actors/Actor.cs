using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    public float timeToLive = 0;
    public VehicleType vehicle;
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
        if (timeToLive > 0)
        {
            if (Time.fixedTime > timeToLive)
            {
                Destroy(this.gameObject);
            }
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
