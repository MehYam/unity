using UnityEngine;
using System.Collections;

using PvT.Util;

public sealed class DamagingHotspot : MonoBehaviour
{
    //void OnTriggerEnter2D(Collider2D other)
    //{
    //}
    //void OnTriggerExit2D(Collider2D other)
    //{
    //}

    void OnTriggerStay2D(Collider2D other)
    {
        var otherActor = other.GetComponent<Actor>();
        if (otherActor != null)
        {
            var damage = GetComponent<Actor>().collisionDamage;
            otherActor.TakeDamage(damage * Time.fixedDeltaTime);

            if (other.rigidbody2D != null)
            {
                other.rigidbody2D.AddForce(Util.GetLookAtVector(transform.rotation.eulerAngles.z, damage * Consts.FUSION_KNOCKBACK_MULTIPLIER));
            }
        }
    }
}
