using UnityEngine;
using System.Collections;

using PvT.Util;

public class Actor : MonoBehaviour
{
    public Actor()
    {
        expireTime = EXPIRY_INFINITE;
        explodesOnDeath = true;
        showsHealthBar = true;
    }

    void OnDestroy()
    {
        if (_indicator != null)
        {
            GameObject.Destroy(_indicator);
        }
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

    float _health;
    public float health
    {
        get
        {
            return _health;
        }
        set
        {
            var delta = value - health;
            _health = value;

            GlobalGameEvent.Instance.FireHealthChange(this, delta);
        }
    }
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
    public IActorBehavior behavior { get; set; }

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
            GlobalGameEvent.Instance.FireActorDeath(this);
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
            if ((Time.time - _lastHealthUpdate) > Consts.HEALTH_BAR_TIMEOUT)
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
        if (gameObject.layer == (int)Consts.Layer.MOB)
        {
            UpdateIndicator();
        }
    }
    const float INDICATOR_MARGIN = 0.06f;
    GameObject _indicator;
    void UpdateIndicator()
    {
        var rect = Consts.GetScreenRectInWorldCoords(Camera.main);
        var pos = transform.position;

        if (!rect.Contains(pos))
        {
            if (_indicator == null)
            {
                //KAI: stick to one way of creating assets?
                _indicator = (GameObject)GameObject.Instantiate(Main.Instance.Indicator);
            }
            Vector2 indPos = new Vector2(0, 0);
            indPos.x = Mathf.Max(rect.left + INDICATOR_MARGIN, Mathf.Min(rect.right - INDICATOR_MARGIN, pos.x));
            indPos.y = Mathf.Max(rect.bottom + INDICATOR_MARGIN, Mathf.Min(rect.top - INDICATOR_MARGIN, pos.y));

            _indicator.transform.position = indPos;
            Consts.LookAt2D(_indicator.transform, transform);
            _indicator.SetActive(true);
        }
        else
        {
            if (_indicator != null)
            {
                _indicator.SetActive(false);
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
            if (contact.otherCollider.gameObject == gameObject &&
                contact.collider.gameObject.layer != contact.otherCollider.gameObject.layer)
            {
                HandleCollision(contact);

                // KAI: check that we only need to handle one
                break;
            }
        }
    }

    protected virtual void HandleCollision(ContactPoint2D contact)
    {
        var collider = contact.collider;
        var other = contact.otherCollider;
        //Debug.Log(string.Format("Collision {0} to {1}, me {2}", collider.name, other.name, name));

        DebugUtil.Assert(other.gameObject == gameObject);

        var game = Main.Instance.game;
        
        // if a possessed ship is being hit by the hero, run the possession
        if (game.currentlyPossessed == collider.gameObject &&
            game.player == other.gameObject)
        {
            GlobalGameEvent.Instance.FirePossessionContact(collider.gameObject.GetComponent<Actor>());
        }
        else
        {
            if (collider.gameObject.layer > other.gameObject.layer) // prevent duplicate collision sparks
            {
                var boom = Main.Instance.game.effects.GetRandomSmallExplosion().ToRawGameObject();
                boom.transform.localPosition = contact.point;
            }
            var actor = collider.GetComponent<Actor>();
            if (actor != null)
            {
                TakeDamage(actor.collisionDamage * Random.Range(0.9f, 1.1f));
            }
        }
    }
}
