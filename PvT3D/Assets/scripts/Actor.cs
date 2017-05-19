using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class Actor : MonoBehaviour
{
    const float HEALTH_PER_DAMAGE_SMOKE = 25;

    [Tooltip("Starting health value.  Zero health for invulnerability")]
    public float health = 0;
    public float collisionDamage = 0;
    public float acceleration = 10;
    public float maxSpeed = 10;
    public float rotationalAcceleration = 0.5f;
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;
    public bool lockedY = true;

    public bool explosionOnDeath = false;

    int _collisions = 0;
    Color _startColor;
    float _startHealth = 0;
    float _startY = 0;
    void Start()
    {
        var material = Util.GetMaterialInChildren(gameObject);
        if (material != null && material.HasProperty("_Color"))
        {
            _startColor = material.color;
        }
        _startHealth = health;
        _startY = transform.position.y;
    }
    void OnCollisionEnter(Collision col)
    {
        ++_collisions;

        var otherActor = col.collider.GetComponent<Actor>();
        var takingDamage = 
            otherActor != null && 
            gameObject.layer != otherActor.gameObject.layer && 
            _startHealth > 0;

        Debug.Log(string.Format("{0}. {1} hit by {2}, does damage: {3}", _collisions, name, col.collider.name, takingDamage));
        if (takingDamage)
        {
            int damageSmokeBeforeHit = Mathf.FloorToInt((_startHealth - health) / HEALTH_PER_DAMAGE_SMOKE);
            health -= otherActor.collisionDamage;

            if (health > 0)
            {
                // Injury
                int damageSmokeAfterHit = Mathf.FloorToInt((_startHealth - health) / HEALTH_PER_DAMAGE_SMOKE);
                AddDamageSmoke(damageSmokeAfterHit - damageSmokeBeforeHit);

                StartCoroutine(DisplayHit());
            }
            else
            {
                // Death
                GlobalEvent.Instance.FireActorDeath(this);
                RemoveDamageSmoke();
                if (explosionOnDeath)
                {
                    var explosion = GameObject.Instantiate(Main.game.plasmaExplosionPrefab);
                    explosion.transform.parent = Main.game.effectParent.transform;
                    explosion.transform.position = transform.position;
                }
                GameObject.Destroy(gameObject);
            }
        }
    }
    Transform damageSmoke;
    void AddDamageSmoke(int num)
    {
        if (num > 0)
        {
            if (damageSmoke == null)
            {
                damageSmoke = new GameObject("damageSmokeParent").transform;
                damageSmoke.parent = transform;
                damageSmoke.transform.localPosition = Vector3.zero;
            }
            for (int i = 0; i < num; ++i)
            {
                var smoke = GameObject.Instantiate(Main.game.damageSmokePrefab);
                smoke.transform.parent = damageSmoke;
                smoke.transform.localPosition = Random.insideUnitSphere * 3;
            }
        }
    }
    void RemoveDamageSmoke()
    {
        // we want damage smoke particles to stick around after death, so parent them to the effects layer
        if (damageSmoke != null)
        {
            damageSmoke.parent = Main.game.effectParent.transform;

            var ttl = damageSmoke.gameObject.AddComponent<TimeToLive>();
            ttl.seconds = 5;

            var particles = damageSmoke.GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                particle.Stop();
            }
        }
    }
    IEnumerator DisplayHit()
    {
        //KAI: seems like this doesn't belong in Actor, but what do I know
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = new Color(1, .7f, .7f);
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = _startColor;
        }
    }
    void FixedUpdate()
    {
        //NOTE: Actor should be set to run after all other scripts in the script execution order, unless we start using the 
        // roll-your-own composited behaviors from PvT
        var body = GetComponent<Rigidbody>();
        if (body != null)
        {
            var clamped = Vector3.ClampMagnitude(body.velocity, maxSpeed);
            clamped.y = 0;
            body.velocity = clamped;
        }

        // lock the actor's height for now.  This is a hack, partly because the freeze constraints options on the rigidbody fail upon collision
        if (lockedY)
        {
            var position = transform.position;
            if (position.y != _startY)
            {
                position.y = _startY;
                transform.position = position;
            }
        }
    }

    public SimpleRoom room { get; private set; }
    void OnRoomEnter(SimpleRoom room)
    {
        if (this.room != room)
        {
            //Debug.LogFormat("Actor {0} enters room {1}", name, room.name);

            this.room = room;
        }
    }
}
