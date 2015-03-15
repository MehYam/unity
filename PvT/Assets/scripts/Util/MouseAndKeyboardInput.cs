using UnityEngine;
using System.Collections;

public sealed class MouseAndKeyboardInput : IInput
{
    public bool Primary()
    {
        return Input.GetButton("Fire1") || Input.GetButton("Jump");
    }
    public bool PrimaryAlt()
    {
        return Input.GetButton("Jump");
    }
    public bool Secondary()
    {
        return Input.GetButton("Fire2");
    }
    public Vector2 CurrentCursor
    {
        get { return Input.mousePosition; }
    }
    public Vector2 CurrentMovementVector
    {
        get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); }
    }
}
