using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAutofire : MonoBehaviour, IWeaponControl
{
    [SerializeField] float damage;
    [SerializeField] float speed;
    [SerializeField] float duration;
    [SerializeField] float rate;
    [SerializeField] bool inheritShipVelocity;

    [SerializeField] Transform firepoint;
    [SerializeField] GameObject prefab;

    int ammoLayer;
    ParticleSystem _ps;
    void Start()
    {
        if (gameObject.layer == LayerMask.NameToLayer("enemy"))
        {
            ammoLayer = LayerMask.NameToLayer("enemyAmmo");
        }
        else if (gameObject.layer == LayerMask.NameToLayer("friendly"))
        {
            ammoLayer = LayerMask.NameToLayer("friendlyAmmo");
        }
        _ps = firepoint.GetComponent<ParticleSystem>();

        SendMessage("OnWeaponControlStart", this);
    }

    float _lastFire = 0;
    float _delay = 0;
    public void OnFireStart()
    {
        _delay = 1/rate;
    }
    public void OnFireEnd()
    {
    }
    public void OnFireFrame()
    {
        if ((Time.fixedTime - _lastFire) > _delay)
        {
            Launch();
            _lastFire = Time.fixedTime;
        }
    }
    void OrientAmmo(GameObject ammo)
    {
        ammo.transform.parent = Main.game.ammoParent.transform;
        ammo.transform.position = firepoint.transform.position;
        ammo.transform.rotation = firepoint.transform.rotation;
        ammo.layer = ammoLayer;
    }
    void Launch()
    {
        // line the shot up
        var ammo = GameObject.Instantiate(prefab);
        OrientAmmo(ammo);

        // duration  KAI: replace this with Destroy(, t)!!!!!!
        var ttl = ammo.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = duration;
        }

        // inherit the ship's velocity
        var rb = ammo.transform.GetComponent<Rigidbody>();
        if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            //KAI: a bug, turret ammo needs to pick up launcher velocity as well
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        }

        // impart ammo velocity in the direction of the firer
        rb.velocity += gameObject.transform.forward * speed;

        // spin it for kicks
        rb.angularVelocity = Vector3.up * 10;

        // this is a chargable shot, scale it by the power
        //ammo.transform.localScale = new Vector3(product.power, product.power, product.power);

        // particles
        if (_ps != null)
        {
            _ps.Play();
        }
    }

}
