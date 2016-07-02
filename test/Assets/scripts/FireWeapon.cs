using UnityEngine;
using System.Collections;

public sealed class FireWeapon : MonoBehaviour
{
    public GameObject firepoint;
    public GameObject ammo;
    public float rate = 1;
    public float ammoSpeed = 1;

    float _lastFire = 0;
	void FixedUpdate()
    {
	    if ((Input.GetButton("Fire1") || Input.GetButton("Jump")) && 
            (Time.fixedTime - _lastFire) > rate)
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
            rb.velocity += gameObject.transform.forward * ammoSpeed;

            // spin it for kicks
            rb.angularVelocity = Vector3.up * 10;

            _lastFire = Time.fixedTime;
        }
	}
}
