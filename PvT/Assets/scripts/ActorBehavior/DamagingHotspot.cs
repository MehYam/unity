using UnityEngine;
using System.Collections;

public sealed class DamagingHotspot : MonoBehaviour
{
    public WorldObjectType.Weapon weapon { get; set; }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (weapon != null)
        {
            var actor = other.GetComponent<Actor>();
            if (actor != null)
            {
                actor.TakeDamage(weapon.damage);
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
