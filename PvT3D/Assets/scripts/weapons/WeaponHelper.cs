using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WeaponHelper
{ 
    public readonly int ammoLayer;
    public readonly ParticleSystem particles;
    readonly Transform firepoint;

    public WeaponHelper(GameObject gameObject, Transform firepoint)
    {
        if (gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            ammoLayer = LayerMask.NameToLayer("enemyAmmo");
        }
        else if (gameObject.layer == LayerMask.NameToLayer("friendly"))
        {
            ammoLayer = LayerMask.NameToLayer("friendlyAmmo");
        }
        particles = firepoint.GetComponent<ParticleSystem>();

        this.firepoint = firepoint;
    }
    void OrientAmmo(GameObject ammo)
    {
        ammo.transform.parent = Main.game.ammoParent.transform;
        ammo.transform.position = firepoint.transform.position;
        ammo.transform.rotation = firepoint.transform.rotation;
        ammo.layer = ammoLayer;
    }
    public void Launch(
        GameObject launcher, 
        GameObject ammoPrefab, 
        float damage,
        float duration,
        float speed,
        Vector3 launcherVelocity, 
        float ammoScale)
    {
        // line the shot up
        var ammo = GameObject.Instantiate(ammoPrefab);
        OrientAmmo(ammo);

        // duration  KAI: replace this with Destroy(, t)!!!!!!
        var ttl = ammo.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = duration;
        }

        // inherit the ship's velocity
        var rb = ammo.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = launcherVelocity;

            // impart ammo velocity in the direction of the firer
            rb.velocity += launcher.transform.forward * speed;
        }

        // this is a chargable shot, scale it by the power
        var ammoActor = ammo.GetComponent<Actor>();
        ammoActor.collisionDamage = damage;
        ammo.transform.localScale = new Vector3(ammoScale, ammoScale, ammoScale);

        // particles
        if (particles != null)
        {
            particles.Play();
        }
    }
}
