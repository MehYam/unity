using UnityEngine;
using System.Collections;

/// <summary>
/// This class is in charge of consolidating input between the 
/// </summary>
public sealed class MasterInput
{
    MasterInput() { }

    static public readonly MasterInput Instance = new MasterInput();

    static public bool Click()
    {
        return Input.GetButton("Fire1");
    }
    static public bool Click2()
    {
        return Input.GetButton("Fire2");
    }
    static public bool Fire()
    {
        return Input.GetButton("Jump");
    }
    static public Vector2 CurrentInputVector
    {
        get { return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); }
    }

}
