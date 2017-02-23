using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class PlayerFiring : MonoBehaviour
{
    MonoBehaviour firing;

    //KAI: this potentially lags the firing for one frame...

	// Use this for initialization
	void Start()
    {
        firing = GetComponent<FireWeapon>();
        firing.enabled = false;
	}
    void FixedUpdate()
    {
        if (firing != null)
        {
            firing.enabled = InputUtil.GetFiringVector(transform.position) != Vector3.zero;
        }
    }
}
