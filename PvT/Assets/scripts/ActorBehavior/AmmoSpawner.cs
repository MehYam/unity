using UnityEngine;
using System.Collections;

public sealed class AmmoSpawner : MonoBehaviour
{
    public GameObject Ammo;
    public Vector3 Offset;
    public float Rate = 1;
    float _lastFire;
	void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if ((Time.time - _lastFire) > Rate)
            {
                var ammo = (GameObject)GameObject.Instantiate(Ammo);
                ammo.transform.rotation = transform.rotation;

                var startPoint = Consts.RotatePoint(Offset, -Consts.ACTOR_NOSE_OFFSET - ammo.transform.rotation.eulerAngles.z);
                ammo.transform.localPosition = transform.localPosition + new Vector3(startPoint.x, startPoint.y);

                _lastFire = Time.time;

                Debug.Log(Input.mousePosition);
            }
        }
	}
}
