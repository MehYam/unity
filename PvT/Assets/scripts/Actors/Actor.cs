using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    public Actor()
    {
        expireTime = EXPIRY_INFINITE;
        explodesOnDeath = true;
        showsHealthBar = true;
    }

    WorldObjectType _worldObject;
    public WorldObjectType worldObject 
    {
        get { return _worldObject; }
        set
        {
            health = (float.IsNaN(value.health) || value.health == 0) ? 1 : value.health;
   
            _worldObject = value;
        }
    }

    public float expireTime { get; private set; }

    static public readonly float EXPIRY_INFINITE = 0;
    public void SetExpiry(float secondsFromNow)
    {
        expireTime = secondsFromNow == EXPIRY_INFINITE ? EXPIRY_INFINITE : Time.fixedTime + secondsFromNow;
    }

    public bool showsHealthBar{ get; set; }
    public bool explodesOnDeath { get; set; }
    public float health{ get; set; }
    public float maxSpeed
    {
        get
        {
            return (_modifier != null) ? worldObject.maxSpeed + _modifier.maxSpeed : worldObject.maxSpeed;
        }
    }
    public float acceleration
    {
        get
        {
            //KAI: cheese - sort this out
            var v = (VehicleType) worldObject;
            return (_modifier != null) ? v.acceleration + _modifier.acceleration : v.acceleration;
        }
    }
    public float collisionDamage;
    public IActorBehavior behavior { protected get; set; }

    ActorModifier _modifier;
    public void AddModifier(ActorModifier modifier)
    {
        Debug.Log("Adding modifier " + modifier);
        _modifier = modifier;
    }

    float _lastHealthUpdate = 0;
    ProgressBar _healthBar;
    public void TakeDamage(float dmg)
    {
        if (dmg > 0)
        {
            this.health -= dmg;

            if (showsHealthBar && this.health > 0)
            {
                if (_healthBar == null)
                {
                    var bar = (GameObject)GameObject.Instantiate(Main.Instance.ProgressBar);
                    _healthBar = bar.GetComponent<ProgressBar>();
                    bar.transform.parent = transform;
                }
                _healthBar.percent = health / worldObject.health;
                _healthBar.gameObject.SetActive(true);
            }
            _lastHealthUpdate = Time.time;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.FixedUpdate(this);
        }
        if (rigidbody2D != null && worldObject.maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > worldObject.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, maxSpeed);
        }
        if (((expireTime != EXPIRY_INFINITE) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            Main.Instance.game.HandleActorDeath(this);
        }
        if (_modifier != null && Time.fixedTime > _modifier.expiry)
        {
            Debug.Log("removing modifier");
            _modifier = null;
        }
    }
    void Update()
    {
        if (_healthBar != null && _healthBar.gameObject.activeSelf)
        {
            if ((Time.time - _lastHealthUpdate) > 300)
            {
                _healthBar.gameObject.SetActive(false);
            }
            else
            {
                var level = new Quaternion();
                level.eulerAngles = Vector3.zero;

                _healthBar.transform.position = transform.position + new Vector3(0, 0.5f);
                _healthBar.transform.rotation = level;
            }
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("collide " + worldObject.name);
        //Debug.Log(collision.relativeVelocity.magnitude);
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider.gameObject.layer != contact.otherCollider.gameObject.layer)
            {
                HandleCollision(contact);

                // KAI: check that we only need to handle one
                break;
            }
        }
    }

    protected virtual void HandleCollision(ContactPoint2D contact)
    {
        //Debug.Log(string.Format("HandleCollision in {0}, between {1} and {2}", name, contact.collider.name, contact.otherCollider.name));

        var collider = contact.collider;
        var other = contact.otherCollider;
        if (collider.gameObject.layer > other.gameObject.layer) // prevent duplicate collision sparks
        {
            var boom = Main.Instance.game.effects.GetRandomSmallExplosion().ToRawGameObject();
            boom.transform.localPosition = contact.point;
        }
        TakeDamage(collider.GetComponent<Actor>().collisionDamage * Random.Range(0.9f, 1.1f));
    }
}
