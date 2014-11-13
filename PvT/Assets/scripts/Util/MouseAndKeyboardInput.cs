using UnityEngine;
using System.Collections;

public sealed class MouseAndKeyboardInput : IInput
{
    public bool Primary()
    {
        return Input.GetButton("Fire1");
    }
    public bool PrimaryAlt()
    {
        return Input.GetButton("Jump");
    }
    public bool Secondary()
    {
        return Input.GetButton("Fire2");
    }
    public Vector2 CurrentPointer
    {
        get { return Input.mousePosition; }
    }
    public Vector2 CurrentInputVector
    {
        get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); }
    }
}
