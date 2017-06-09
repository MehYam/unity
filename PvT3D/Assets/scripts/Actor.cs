using UnityEngine;
using System;

using PvT3D.Util;

public sealed class Actor : MonoBehaviour
{
    [Tooltip("Starting health value.  Zero health for invulnerability")]
    public float health = 0;
    public float collisionDamage = 0;
    public float acceleration = 10;
    public float maxSpeed = 10;
    public float rotationalAcceleration = 0.5f;
    [Tooltip("Maximum rotation speed in Rotations Per Second")]
    public float maxRPS = 1;
    public bool lockedY = true;

    /// <summary>
    /// HealthChanged(Actor this, float oldHealth, float newHealth, bool fromDamage)
    /// </summary>
    public Action<Actor, float, float, bool> HealthChanged = delegate { };
    public Action<Actor> ActorDying = delegate { };

    int _collisions = 0;
    Color _startColor;
    float _startY = 0;
    float _startHealth = 0;
    void Start()
    {
        _startY = transform.position.y;
        _startHealth = Mathf.Max(_startHealth, health);

        // for now, freeze rotation on all actors
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }
    public float baseHealth { get { return _startHealth; } set { _startHealth = value; } }
    public float healthPct { get { return _startHealth > 0 ? (health/_startHealth) : 0; } }
    public void SetHealth(float newHealth, bool fromDamage = false)
    {
        var old = health;

        health = newHealth;
        HealthChanged(this, old, newHealth, fromDamage);

        if (health <= 0)
        {
            ActorDying(this);

            GlobalEvent.Instance.FireActorDeath(this);
            GameObject.Destroy(gameObject);
        }
    }
    public void SetHealthPct(float newPct, bool countsAsDamage = false)
    {
        SetHealth(_startHealth * newPct, countsAsDamage);
    }
    void OnCollisionEnter(Collision other)
    {
        HandleActorCollision(other.collider.GetComponent<Actor>());
    }
    void OnTriggerEnter(Collider other)
    {
        HandleActorCollision(other.GetComponent<Actor>());
    }
    public void HandleActorCollision(Actor otherActor)
    {
        ++_collisions;
        var takingDamage = 
            otherActor != null && 
            gameObject.layer != otherActor.gameObject.layer && 
            health > 0;

        //Debug.Log(string.Format("{0}. {1} hit by {2}, does damage: {3}", _collisions, name, col.collider.name, takingDamage));
        if (takingDamage)
        {
            SetHealth(health - otherActor.collisionDamage, true);
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
