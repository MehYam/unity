using UnityEngine;
using System.Collections;

public sealed class Actor : MonoBehaviour
{
    public float health = 0;
    public float collisionDamage = 0;

    static int _count = 0;
    void OnCollisionEnter(Collision col)
    {
        ++_count;
        //Debug.Log(string.Format("{0}. {1} hit by {2}", _count, name, col.collider.name));

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

    void OnDestroy()
    {
        GlobalGameEvent.Instance.FireActorDeath(this);
    }
}
