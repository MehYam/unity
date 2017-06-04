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

    WeaponHelper _helper;
    void Start()
    {
        _helper = new WeaponHelper(gameObject, firepoint);
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
    void Launch(float damage, float intensity)
    {
        Vector3 inheritedVelocity = inheritShipVelocity ? gameObject.GetComponent<Rigidbody>().velocity : Vector3.zero;
        _helper.Launch(
            gameObject,
            prefab,
            damage, 
            duration,
            speed,
            inheritedVelocity,
            2 * intensity
            );
    }
}
