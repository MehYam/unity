using UnityEngine;
using System.Collections;

public sealed class FireWeapon : MonoBehaviour
{
    public GameObject firepoint;
    public GameObject ammo;
    public float rate = 1;

    float _lastFire = 0;
	void FixedUpdate()
    {
	    if (Input.GetButton("Fire1") && (Time.fixedTime - _lastFire) > rate)
        {
            var shot = GameObject.Instantiate(ammo);
            shot.transform.position = firepoint.transform.position;

            var rb = shot.transform.GetComponent<Rigidbody>();
            rb.velocity = gameObject.GetComponent<Rigidbody>().velocity;

            Debug.Log(rb.velocity);

            rb.angularVelocity = Vector3.up * 10;

            _lastFire = Time.fixedTime;
        }
	}
}
