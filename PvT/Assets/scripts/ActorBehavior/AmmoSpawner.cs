using UnityEngine;
using System.Collections;

public sealed class AmmoSpawner : MonoBehaviour
{
    public GameObject Ammo;
    public Vector3 Offset;
    public float Rate = 1;
    float _lastFire;
	void FixedUpdate()
    {
        if ((Time.time - _lastFire) > Rate && (Input.GetButton("Fire1") || Input.GetButton("Jump")))
        {
            var ammo = (GameObject)GameObject.Instantiate(Ammo);
            ammo.transform.rotation = transform.rotation;

            var startPoint = Consts.RotatePoint(Offset, -Consts.ACTOR_NOSE_OFFSET - ammo.transform.rotation.eulerAngles.z);
            ammo.transform.localPosition = transform.localPosition + new Vector3(startPoint.x, startPoint.y);

            _lastFire = Time.time;
        }
	}
}
