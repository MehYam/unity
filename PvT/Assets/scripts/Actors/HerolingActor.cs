using UnityEngine;
using System.Collections;

public class HerolingActor : Actor
{
    Actor _currentMob;
    protected void OnCollide(Actor mob)
    {
        // attach to mob
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // choose one of:
        //
        // launch behavior
        //
        // attach behavior
        //
        // rest (no) behavior
    }
}
