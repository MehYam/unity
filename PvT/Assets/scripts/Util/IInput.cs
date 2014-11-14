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

    /// <summary>
    /// Returns the cursor or touch location in screen coordinates
    /// </summary>
    Vector2 CurrentCursor { get; }

    /// <summary>
    /// Returns the movement vector, in a rectangle from (-1,-1) to (1, 1).
    /// </summary>
    Vector2 CurrentMovementVector { get; }
}
