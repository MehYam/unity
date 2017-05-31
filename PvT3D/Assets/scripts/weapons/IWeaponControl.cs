using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeaponControl
{
    void OnFireStart();
    void OnFireEnd();
    void OnFireFrame();
}
