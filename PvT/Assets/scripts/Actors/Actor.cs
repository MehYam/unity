using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour
{
    public bool explodesOnDeath { get; set; }
    public float health { get; private set; }
    public float collisionDamage;
    public IActorBehavior behavior { private get; set; }

    public Actor()
    {
        expireTime = EXPIRY_INFINITE;
        explodesOnDeath = true;
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

    static public readonly float EXPIRY_INFINITE = float.NaN;
    public void SetExpiry(float secondsFromNow)
    {
        expireTime = Time.fixedTime + secondsFromNow;
    }

    float _lastHealthUpdate = 0;
    ProgressBar _healthBar;
    public void TakeDamage(float dmg)
    {
        this.health -= dmg;

        if (this.health > 0)
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

    void FixedUpdate()
    {
        if (behavior != null)
        {
            behavior.FixedUpdate(this);
        }
        if (worldObject.maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > worldObject.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, worldObject.maxSpeed);
        }
        if (((expireTime != EXPIRY_INFINITE) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            Main.Instance.game.HandleActorDeath(this);
        }
    }
    void Update()
    {
        if (_healthBar && _healthBar.gameObject.activeSelf)
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
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Main.Instance.game.HandleCollision(contact);
        }
        //Debug.Log(collision.relativeVelocity.magnitude);
    }
}
