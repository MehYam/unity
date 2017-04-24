using UnityEngine;
using System.Collections;

using PvT3D.Util;

public sealed class Actor : MonoBehaviour
{
    public float health = 0;
    public float collisionDamage = 0;
    public float acceleration = 10;
    public float rotationalAcceleration = 0.5f;
    public float maxSpeed = 10;

    static int _collisions = 0;

    Color _startColor;
    void Start()
    {
        var material = Util.GetMaterialInChildren(gameObject);
        if (material != null && material.HasProperty("_Color"))
        {
            _startColor = material.color;
        }

        // Detect our room - KAI: this looks expensive, esp. for things like ammo?  Also, there may be an easier way to do this with raycasts/collisions?
        //var rooms = FindObjectsOfType<SimpleRoom>();
        //foreach (var room in rooms)
        //{

        //}
    }
    void OnCollisionEnter(Collision col)
    {
        ++_collisions;

        var otherActor = col.collider.GetComponent<Actor>();
        var doesDamage = otherActor != null && gameObject.layer != otherActor.gameObject.layer;

        //Debug.Log(string.Format("{0}. {1} hit by {2}, does damage: {3}", _collisions, name, col.collider.name, doesDamage));
        if (doesDamage)
        {
            health -= otherActor.collisionDamage;
            if (health <= 0)
            {
                //Debug.Log("Taken lethal damage DESTROY=====");
                GlobalEvent.Instance.FireActorDeath(this);
                GameObject.Destroy(gameObject);
            }
            else
            {
                StartCoroutine(DisplayHit());
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
        body.velocity = Vector3.ClampMagnitude(body.velocity, maxSpeed);
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
