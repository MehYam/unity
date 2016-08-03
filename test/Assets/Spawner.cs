using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT3D.Util;

public sealed class Spawner : MonoBehaviour
{
    public GameObject target;
    public GameObject enemy;
    public float rate = 3;
    public float count = 1;

	// Use this for initialization
	void Start()
    {
        GlobalEvent.Instance.ActorDeath += OnActorDeath;

        StartCoroutine(Spawn());
	}
    void OnDestroy()
    {
        GlobalEvent.Instance.ActorDeath -= OnActorDeath;
    }
    HashSet<Actor> _liveEnemies = new HashSet<Actor>();
    IEnumerator Spawn()
    {
        for (int spawned = 0; spawned < count; ++spawned)
        {
            // create an enemy, give it initial heading and velocity.
            var spawn = GameObject.Instantiate(enemy);
            _liveEnemies.Add(spawn.GetComponent<Actor>());

            spawn.transform.position = transform.position;
            spawn.transform.eulerAngles = new Vector3(0, Util.DegreesRotationInY(target.transform.position - spawn.transform.position), 0);
            spawn.GetComponent<FaceTarget>().target = target;

            var body = spawn.GetComponent<Rigidbody>();
            body.velocity = spawn.transform.right * 30;

            yield return new WaitForSeconds(rate);
        }
    }	
    void OnActorDeath(Actor actor)
    {
        _liveEnemies.Remove(actor);
        if (_liveEnemies.Count == 0)
        {
            StartCoroutine(Spawn());
        }
    }
}
