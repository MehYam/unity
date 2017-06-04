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

    WeaponHelper _helper;
    void Start()
    {
        _helper = new WeaponHelper(gameObject, firepoint);
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
    void Launch()
    {
        Vector3 inheritedVelocity = inheritShipVelocity ? gameObject.GetComponent<Rigidbody>().velocity : Vector3.zero;

        _helper.Launch(
            gameObject,
            prefab,
            damage,
            duration,
            speed,
            inheritedVelocity,
            1);
    }
}
