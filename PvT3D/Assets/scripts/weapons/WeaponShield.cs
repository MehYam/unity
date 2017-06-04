using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PvT3D.Util;

public class WeaponShield : MonoBehaviour, IWeaponControl
{
    [SerializeField] float maxHealth;
    [SerializeField] float damage;
    [SerializeField] float healthPerSecond;
    [SerializeField] float delay;
    [SerializeField] float speed;
    [SerializeField] float duration;
    [SerializeField] bool inheritShipVelocity;

    [SerializeField] Transform firepoint;
    [SerializeField] GameObject prefab;

	void Start()
    {
        SendMessage("OnWeaponControlStart", this);
	}
    enum FireState { Idle, WaitingToCharge, Charging }

    float _lastChargeStart = 0;
    float _lastFireEnd = 0;
    FireState _state = FireState.Idle;
    public void OnFireStart()
    {
        _state = FireState.WaitingToCharge;
    }
    Actor _currentShield;
    public void OnFireFrame()
    {
        if (_state == FireState.WaitingToCharge && (Time.fixedTime - _lastFireEnd) >= delay)
        {
            _state = FireState.Charging;
            _lastChargeStart = Time.fixedTime;

            Debug.Assert(_currentShield == null, "starting to charge a new shield when we already have one");

            // create a new shield
            _currentShield = GameObject.Instantiate(prefab).GetComponent<Actor>();
            _currentShield.baseHealth = maxHealth;
            _currentShield.collisionDamage = damage;

            // attach it to the ship
            _currentShield.transform.parent = transform;
            _currentShield.transform.position = firepoint.position;
            _currentShield.transform.rotation = firepoint.rotation;
            _currentShield.gameObject.layer = LayerMask.NameToLayer("friendlyShield");

            // use a joint to attach the shield.  You don't strictly need one, but physics and collisions with walls work more realistically this way
            var joint = gameObject.GetOrAddComponent<FixedJoint>();
            joint.connectedBody = _currentShield.gameObject.GetComponent<Rigidbody>();
        }

        if (_currentShield != null)
        {
            var newHealth = Mathf.Min(maxHealth, _currentShield.health + Time.fixedDeltaTime * healthPerSecond);
            _currentShield.SetHealth(newHealth);
        }
    }
    public void OnFireEnd()
    {
        if (_state == FireState.Charging && _currentShield != null)
        {
            // asynchronous operations in the physics system require us to use a coroutine
            StartCoroutine(DetachShield(_currentShield));

            _currentShield.transform.parent = Main.game.ammoParent.transform;
            _currentShield = null;
        }
        _state = FireState.Idle;
    }
    IEnumerator DetachShield(Actor shield)
    {
        // disconnect the joint
        var joint = GetComponent<FixedJoint>();
        Destroy(GetComponent<FixedJoint>());

        // Destroy is asynchronous, so wait until end of frame for this to actually take effect.  
        // Otherwise, the ammo velocity we impart below will affect the ship
        yield return new WaitForEndOfFrame();

        // duration
        var ttl = shield.GetComponent<TimeToLive>();
        if (ttl != null)
        {
            ttl.seconds = duration;
            ttl.enabled = true;
        }

        // inherit the ship's velocity
        var rb = shield.gameObject.GetComponent<Rigidbody>();
        if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
        {
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
        }

        // impart ammo velocity in the direction of the firer
        rb.velocity += gameObject.transform.forward * speed;
    }
}
