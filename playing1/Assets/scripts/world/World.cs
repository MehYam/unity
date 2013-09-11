using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Sfs2X.Entities.Data; //KAI: doesn't necessarily belong here

namespace playing1.world
{
    public sealed class World
    {
        readonly ActorStore mobs = new ActorStore();
        readonly ActorStore players = new ActorStore();
        readonly ActorStore ammo = new ActorStore();

        //KAI: this doesn't really belong in the model, but in a controller that plugs these into the model.  This
        // controller knows 1) how to manipulate a world, 2) how to manipulate GameObjects, etc, keeping the model a
        // little cleaner
        readonly PrefabLookup prefabs;
        public World(PrefabLookup prefabs)
        {
            this.prefabs = prefabs;
        }
        public void SpawnActor(ISFSObject param)
        {
            var gameObject = (GameObject)GameObject.Instantiate(prefabs.Enemy, new Vector3(param.GetFloat("x"), param.GetFloat("y")), new Quaternion(0, 0, param.GetFloat("r"), 0));
            var actor = new Actor(param.GetInt("id"), gameObject.transform);
            actor.speed = new Vector3(param.GetFloat("dx"), param.GetFloat("dy"));

            players.Add(actor);
        }
        public void MoveActor(ISFSObject param)
        {
            var actor = players.Get(param.GetInt("id"));

            actor.transform.localPosition = new Vector3(param.GetFloat("x"), param.GetFloat("y"));
            actor.speed = new Vector3(param.GetFloat("dx"), param.GetFloat("dy"));
        }

        void OnUpdate()
        {
            float elapsed = float.NaN;
            foreach (var mob in mobs.list)
            {
                mob.onFrame(elapsed);
            }
            foreach (var player in players.list)
            {
                player.onFrame(elapsed);
            }
            foreach (var shot in ammo.list)
            {
                shot.onFrame(elapsed);
            }
        }
    }
}
