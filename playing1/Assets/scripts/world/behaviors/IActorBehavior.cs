  using UnityEngine;
using System.Collections;

namespace playing1.world.behaviors
{
    public interface IActorBehavior
    {
        void onFrame(float elapsed, Actor actor);
    }
}
