using UnityEngine;
using System.Collections;

public interface IWeaponController
{
    void OnStart(Actor actor);
    void OnFrame(Actor actor);
    void OnEnd(Actor actor);
}
