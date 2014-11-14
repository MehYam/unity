using UnityEngine;
using System.Collections;

/// <summary>
/// This interface provides an identical way to access touch and mouse and keyboard inputs
/// </summary>
public interface IInput
{
    bool Primary();
    bool PrimaryAlt();
    bool Secondary();

    Vector2 CurrentCursor { get; }
    Vector2 CurrentMovementVector { get; }
}
