using UnityEngine;
using System.Collections;

public sealed class Actor : MonoBehaviour
{
    public float health = 0;
    public float collisionDamage = 0;
    public float acceleration = 10;
    public float maxSpeed = 10;

    static int _collisions = 0;
    void OnCollisionEnter(Collision col)
    {
        ++_collisions;
        //Debug.Log(string.Format("{0}. {1} hit by {2}", _collisions, name, col.collider.name));

        var otherActor = col.collider.GetComponent<Actor>();
        if (otherActor != null)
        {
            health -= otherActor.collisionDamage;

            if (health <= 0)
            {
                //Debug.Log("Taken lethal damage DESTROY=====");
                GameObject.Destroy(gameObject);
            }
        }
    }
    void FixedUpdate()
    {
        //NOTE: Actor should be set to run after all other scripts in the script execution order, unless we start using the 
        // roll-your-own composited behaviors from PvT
        var body = GetComponent<Rigidbody>();
        body.velocity = Vector3.ClampMagnitude(body.velocity, maxSpeed);
    }
    void OnDestroy()
    {
        GlobalEvent.Instance.FireActorDeath(this);
    }
}
