using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCharge : MonoBehaviour, IWeaponControl
{
    [SerializeField] float damage;
    [SerializeField] float chargeTime;
    [SerializeField] float delay;
    [SerializeField] float speed;
    [SerializeField] float duration;
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

    enum FireState { Idle, WaitingToCharge, Charging }

    float _lastChargeStart = 0;
    float _lastFireEnd = 0;
    FireState _state = FireState.Idle;
    public void OnFireStart()
    {
        _state = FireState.WaitingToCharge;
        Debug.Log("Waiting");
    }
    public void OnFireFrame()
    {
        if (_state == FireState.WaitingToCharge && (Time.fixedTime - _lastFireEnd) >= delay)
        {
            Debug.Log("Charging");

            _state = FireState.Charging;
            _lastChargeStart = Time.fixedTime;
        }
    }
    public void OnFireEnd()
    {
        if (_state == FireState.Charging)
        {
            float elapsedChargeTime = Time.fixedTime - _lastChargeStart;
            float chargePct = Mathf.Min(elapsedChargeTime, chargeTime) / chargeTime;
            float totalDamage = damage * chargePct;

            Launch(totalDamage, chargePct);

            Debug.LogFormat("Launch {0} damage", totalDamage);
            _lastFireEnd = Time.fixedTime;
        }
        _state = FireState.Idle;
    }
    void OrientAmmo(GameObject ammo)
    {
        ammo.transform.parent = Main.game.ammoParent.transform;
        ammo.transform.position = firepoint.transform.position;
        ammo.transform.rotation = firepoint.transform.rotation;
        ammo.layer = ammoLayer;
    }
    void Launch(float totalDamage, float intensity)
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

        // this is a chargable shot, scale it by the power
        var ammoActor = ammo.GetComponent<Actor>();
        ammoActor.collisionDamage = totalDamage;
        ammo.transform.localScale = new Vector3(2 * intensity, 2 * intensity, 2 * intensity);

        // particles
        if (_ps != null)
        {
            _ps.Play();
        }
    }
}
