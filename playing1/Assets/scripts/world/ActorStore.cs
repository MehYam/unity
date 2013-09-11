using UnityEngine;
using System.Collections.Generic;

namespace playing1.world
{
    public sealed class ActorStore
    {
        //KAI: simpler perhaps faster alternative:  a sorted array that's purged of dead actors every few frames
        public readonly LinkedList<Actor> list = new LinkedList<Actor>();
        public readonly Dictionary<int, Actor> lookup = new Dictionary<int, Actor>();

        public void Add(Actor actor)
        {
            if (lookup.ContainsKey(actor.id))
            {
                Debug.LogWarning("Trying to add already existent actor " + actor.id);
            }
            else
            {
                list.AddLast(actor);
                lookup[actor.id] = actor;
            }
        }
        public Actor Get(int id)
        {
            Actor retval = null;
            lookup.TryGetValue(id, out retval);
            return retval;
        }
    }
}
