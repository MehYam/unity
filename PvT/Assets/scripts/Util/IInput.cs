using UnityEngine;
using System.Collections;

public interface IInput
{
    bool Primary();
    bool PrimaryAlt();
    bool Secondary();

    Vector2 CurrentPointer { get; }
    Vector2 CurrentInputVector { get; }
}
