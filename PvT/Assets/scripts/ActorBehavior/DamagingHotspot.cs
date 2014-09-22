using UnityEngine;
using System.Collections;

public sealed class DamagingHotspot : MonoBehaviour
{
    public WorldObjectType.Weapon weapon { get; set; }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (weapon != null)
        {
            var otherActor = other.GetComponent<Actor>();
            if (otherActor != null)
            {
                otherActor.TakeDamage(weapon.damage);
            }
        }
    }
    //void OnTriggerStay2D(Collider2D other)
    //{
    //}
    void OnTriggerExit2D(Collider2D other)
    {
    }
}
