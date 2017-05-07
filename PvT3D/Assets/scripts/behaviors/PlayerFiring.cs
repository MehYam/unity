using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class PlayerFiring : MonoBehaviour
{
    WeaponSchematic schematic;

	// Use this for initialization
	void Start()
    {
        schematic = GetComponent<WeaponSchematic>();  // 
	}
    bool fireState = false;
    void FixedUpdate()
    {
        var fv = InputUtil.GetFiringVector(transform.position);
        if (fv != Vector3.zero)
        {
            // determine the change in state
            if (!fireState)
            {
                fireState = true;
                schematic.OnFireStart();
            }
            schematic.OnFireFrame();
        }
        else if (fireState)
        {
            schematic.OnFireEnd();
            fireState = false;
        }
    }
}
