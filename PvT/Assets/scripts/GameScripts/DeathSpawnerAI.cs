using UnityEngine;
using System.Collections;

using PvT.Util;

public class DeathSpawnerAI : MonoBehaviour
{
    public string toSpawnOnDeath;
    public int count;
    void Awake()
    {
        GlobalGameEvent.Instance.ActorDeath += OnActorDeath;
    }
    void OnDestroy()
    {
        GlobalGameEvent.Instance.ActorDeath -= OnActorDeath;
    }
    void OnActorDeath(Actor actor)
    {
        if (actor == GetComponent<Actor>())
        {
            if (!string.IsNullOrEmpty(toSpawnOnDeath))
            {
                for (int i = 0; i < count; ++i)
                {
                    var subMob = Main.Instance.game.SpawnMob(toSpawnOnDeath);
                    MobAI.Instance.AttachAI(subMob.GetComponent<Actor>());

                    subMob.transform.position = transform.position + Util.ScatterRandomly(0.5f);
                    subMob.rigidbody2D.velocity = Util.ScatterRandomly(0.5f);
                    subMob.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
                }
            }
        }
    }
}
