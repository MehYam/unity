using UnityEngine;
using System.Collections;

public sealed class FireWeapon : MonoBehaviour
{
    public GameObject firepoint;
    public GameObject ammo;
    public float rate = 1;

    float _lastFire = 0;
    Actor _actor;
    ParticleSystem _ps;
    void Start()
    {
        _ps = firepoint.GetComponent<ParticleSystem>();
        _actor = GetComponent<Actor>();
    }
	void FixedUpdate()
    {
	    if (Time.fixedTime - _lastFire > rate)
        {
            // line the shot up
            var shot = GameObject.Instantiate(ammo);
            shot.transform.position = firepoint.transform.position;

            // inherit the ship's velocity
            var rb = shot.transform.GetComponent<Rigidbody>();
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                //KAI: a bug, turret ammo needs to pick up launcher velocity as well
                rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            }
            // impart extra velocity in the direction of the firer
            rb.velocity += gameObject.transform.forward * _actor.maxSpeed;

            // spin it for kicks
            rb.angularVelocity = Vector3.up * 10;

            // particles
            _ps.Play();

            _lastFire = Time.fixedTime;
        }
	}
}
