using UnityEngine;
using System.Collections;

public sealed class DamagingHotspot : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var otherActor = other.GetComponent<Actor>();
        if (otherActor != null)
        {
            var damage = GetComponent<Actor>().collisionDamage;

            Debug.Log("DamagingHotspot dealing " + damage);
            otherActor.TakeDamage(damage);
        }
    }
    //void OnTriggerStay2D(Collider2D other)
    //{
    //}
    //void OnTriggerExit2D(Collider2D other)
    //{
    //}
}
