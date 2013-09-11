using UnityEngine;
using System.Collections;

using playing1.world.behaviors;
namespace playing1.world
{
    public sealed class Actor
    {
        public readonly int id;
        public readonly Transform transform;     // position and rotation live here

        public Vector3 speed;

        IActorBehavior behavior;
        public Actor(int id, Transform transform)
        {
            this.transform = transform;
            this.id = id;
        }


        // premature optimization here:  we could instead just attach behaviors as scripts to each actor's
        // GameObject, but this gives us finer-grained control and prevents the efficiency drain I've heard about
        // when too many Update() call receivers are in a scene.
        public void onFrame(float elapsed)
        {
            behavior.onFrame(elapsed, this);
        }
    }
}
