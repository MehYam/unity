using UnityEngine;
using System.Collections;

namespace playing1.world.behaviors
{
    public class AutoFire : MonoBehaviour
    {
        public float rate = 1f;
        public GameObject ammo;

        float _lastFire = 0;
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if ((Time.fixedTime - _lastFire) > rate)
                {
                    _lastFire = Time.time;

                    Instantiate(ammo, transform.localPosition, transform.localRotation);
                }
            }
        }
    }
}
