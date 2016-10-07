using UnityEngine;
using System.Collections;

using PvT3D.Util;

public static class InputHelper
{
    public static Vector3 MovementVector
    {
        get { return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")); }
    }
    public static Vector3 GetFiringVector(Vector3 relativeTo)
    {
        // test the mouse first
        if (Input.GetMouseButton(0))
        {
            return Util.MouseVector(relativeTo);
        }
        // next, the right thumbstick
        return new Vector3(Input.GetAxis("RightStickHorz"), 0, -Input.GetAxis("RightStickVert"));
    }
}
