using UnityEngine;
using System.Collections;

public sealed class FireWeapon : MonoBehaviour
{
    public GameObject firepoint;
    public GameObject ammo;
    public float rate = 1;
    public float speed = 25;
    public bool inheritShipVelocity = false;

    float _lastFire = 0;
    ParticleSystem _ps;
    void Start()
    {
        _ps = firepoint.GetComponent<ParticleSystem>();
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
            if (inheritShipVelocity && gameObject.GetComponent<Rigidbody>() != null)
            {
                //KAI: a bug, turret ammo needs to pick up launcher velocity as well
                rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            }
            // impart ammo velocity in the direction of the firer
            rb.velocity += gameObject.transform.forward * speed;

            // spin it for kicks
            rb.angularVelocity = Vector3.up * 10;

            // particles
            _ps.Play();

            _lastFire = Time.fixedTime;
        }
	}
}
