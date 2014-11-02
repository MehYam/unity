//#define SHOW_STRANGE_PARENTING_BUG

using UnityEngine;
using System.Collections;

using PvT.DOM;
using PvT.Util;

//KAI: this class is starting to get bloated into a MobActor.  Should probably
// delineate into subclasses, or find a composition strategy
public class Actor : MonoBehaviour
{
    public Actor()
    {
        expireTime = EXPIRY_INFINITE;
        explodesOnDeath = true;
        showsHealthBar = true;
        takenDamageMultiplier = 1;
        firingEnabled = true;
        thrustEnabled = true;
        immortal = false;

        speedModifier = ActorModifier.IDENTITY;
    }

    protected virtual void Start()  // KAI: interesting Unity gotcha - must document somewhere
    {
        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
        }
        GlobalGameEvent.Instance.FireActorSpawned(this);
    }

    public bool isPlayer { get { return Main.Instance.game.player == gameObject; } }
    public bool isHero { get { return worldObjectType.name == "HERO"; } }
    void OnDestroy()
    {
        if (_trackingArrow != null)
        {
            GameObject.Destroy(_trackingArrow);
        }
        if (_healthBar != null)
        {
            GameObject.Destroy(_healthBar.gameObject);
        }
        if (_overwhelmBar != null)
        {
            GameObject.Destroy(_overwhelmBar.gameObject);
        }
    }

    WorldObjectType _worldObjectType;
    public WorldObjectType worldObjectType 
    {
        get { return _worldObjectType; }
        set
        {
            _worldObjectType = value;
            health = (float.IsNaN(value.health) || value.health == 0) ? 1 : value.health;
        }
    }

    public float expireTime { get; private set; }

    static public readonly float EXPIRY_IMMEDIATE = -1;
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
            var prevHealth = health;
            _health = immortal ? Mathf.Max(1, value) : value;

            GlobalGameEvent.Instance.FireHealthChange(this, prevHealth - _health);
        }
    }
    ActorModifier _speedModifier;
    public ActorModifier speedModifier
    { 
        get { return _speedModifier; }
        set
        {
            _speedModifier = value == null ? ActorModifier.IDENTITY : value;
        }
    }
    public float maxSpeed
    {
        get
        {
            var speed = worldObjectType.maxSpeed * speedModifier.speedMultiplier;
            return isPlayer ? speed * Consts.PLAYER_SPEED_MULTIPLIER : speed;
        }
    }
    public float acceleration
    {
        get
        {
            var v = (VehicleType)worldObjectType; //KAI: cheese

            // Our config wants acceleration to be absolute, without being slowed by mass.  Therefore,
            // derive the force required by multiplying it by mass.  If we start using drag more, 
            // that will have to compensate for as well.
            var accel = v.acceleration * speedModifier.accelerationMultiplier * worldObjectType.mass;
            return isPlayer ? accel * Consts.PLAYER_ACCEL_MULTIPLIER : accel;
        }
    }
    public bool firingEnabled { get; set; }
    public bool trackingArrowEnabled { get; set; }
    public bool thrustEnabled { get; set; }
    public bool immortal { get; set; }

    public float takenDamageMultiplier { get; set; }
    public float collisionDamage;
    public IActorBehavior behavior { get; set; }
    public IActorVisualBehavior visualBehavior { get; set; }

    public int attachedHerolings { get; set; }
    public float overwhelmPct
    {
        get { return health == 0 ? 0 : Mathf.Min(attachedHerolings * Consts.HEROLING_HEALTH_OVERWHELM / health, 1); }
    }

    ProgressBar _overwhelmBar;
    ProgressBar _healthBar;
    public void TakeDamage(float damage)
    {
        var effectiveDamage = takenDamageMultiplier * damage;
        //Debug.Log(string.Format("{0} takes damage {1} effective {2} health {3}", name, damage, effectiveDamage, health));
        
        if (effectiveDamage > 0)
        {
            this.health -= effectiveDamage;

            if (isPlayer)
            {
                //GrantInvuln(Consts.POST_DAMAGE_INVULN);
            }
        }
    }

    public void GrantInvuln(float duration)
    {
        visualBehavior = new PostDamageInvuln(this, duration);
    }

    IActorBehavior _overwhelmBehavior;  // currently used for overwhelming
    protected virtual void FixedUpdate()
    {
        DetectChangeInOverwhelm();
        if (_overwhelmBehavior != null)
        {
            _overwhelmBehavior.FixedUpdate(this);
        }
        else if (behavior != null)
        {
            behavior.FixedUpdate(this);
        }

        if (rigidbody2D != null && maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > worldObjectType.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, maxSpeed);
        }
        if (((expireTime != EXPIRY_INFINITE) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            GlobalGameEvent.Instance.FireActorDeath(this);
        }
    }
    static readonly Vector3 HEALTH_BAR_POSITION = new Vector3(0, 0.5f);
    static readonly Vector3 OVERWHELM_BAR_POSITION = new Vector3(0, -0.5f);
    void Update()
    {
        if (visualBehavior != null)
        {
            visualBehavior.Update(this);
        }

        var showHealth = showsHealthBar && _health > 0 && _health < worldObjectType.health;
        if (showHealth)
        {
            if (_healthBar == null)
            {
                var bar = (GameObject)GameObject.Instantiate(Main.Instance.HealthProgressBar);
                _healthBar = bar.GetComponent<ProgressBar>();

                //STRANGE UNITY BUG?
                // parenting objects on our Actors seems to skew or stretch them a bit.  You can see this
                // in the editor by manually dragging child objects on and off the Actor - putting them on
                // the Actor seems to generate a strange skew that I can't account for in the transforms.
                // For now, health bars will be parented elsewhere
#if SHOW_STRANGE_PARENTING_BUG
                    bar.transform   .parent = transform;
#else
                bar.transform.parent = Main.Instance.EffectParent.transform;
#endif
            }
            _healthBar.gameObject.SetActive(true);
            _healthBar.percent = health / worldObjectType.health;
            _healthBar.transform.position = transform.position + HEALTH_BAR_POSITION;
        }
        else if (_healthBar != null)
        {
            _healthBar.gameObject.SetActive(false);
        }

        //KAI: copy pasta +1 w/ health bar, might be worth generalizing
        if (attachedHerolings > 0)
        {
            if (_overwhelmBar == null)
            {
                var bar = (GameObject)GameObject.Instantiate(Main.Instance.OverwhelmProgressBar);
                _overwhelmBar = bar.GetComponent<ProgressBar>();
                //_overwhelmBar.transform.Rotate(0, 0, 90);

                bar.transform.parent = Main.Instance.EffectParent.transform;
            }
            _overwhelmBar.gameObject.SetActive(true);
            _overwhelmBar.percent = overwhelmPct;
            _overwhelmBar.transform.position = transform.position + OVERWHELM_BAR_POSITION;
        }
        else if (_overwhelmBar != null)
        {
            _overwhelmBar.gameObject.SetActive(false);
        }
    }
    void LateUpdate()
    {
        if (trackingArrowEnabled)
        {
            UpdateTrackingArrow();
        }
    }

    void DetectChangeInOverwhelm()
    {
        var wasOverwhelmed = _overwhelmBehavior != null;
        if (wasOverwhelmed && overwhelmPct < 1)
        {
            // un-overwhelm
            _overwhelmBehavior = null;

            RemoveBlinker(transform);
            var blinker = transform.FindChild(Consts.BLINKER_TAG);
            if (blinker != null)
            {
                GameObject.Destroy(blinker.gameObject);
            }
        }
        else if (!wasOverwhelmed && overwhelmPct == 1f)
        {
            // act overwhelmed
            _overwhelmBehavior = ActorBehaviorFactory.Instance.CreateHerolingOverwhelmBehavior();

            var blinker = (GameObject)GameObject.Instantiate(Main.Instance.OverwhelmedIndicator);
            blinker.transform.parent = transform;
            blinker.transform.localPosition = Vector3.zero;
            blinker.name = Consts.BLINKER_TAG;

            AudioSource.PlayClipAtPoint(Main.Instance.sounds.HerolingCapture, transform.position);
        }
    }

    static void RemoveBlinker(Transform host)
    {
        var blinker = host.FindChild(Consts.BLINKER_TAG);
        if (blinker != null)
        {
            GameObject.Destroy(blinker.gameObject);
        }
    }


    const float INDICATOR_MARGIN = 0.06f;
    GameObject _trackingArrow;
    void UpdateTrackingArrow()
    {
        var rect = Util.GetScreenRectInWorldCoords(Camera.main);
        var pos = transform.position;

        if (!rect.Contains(pos))
        {
            if (_trackingArrow == null)
            {
                //KAI: stick to one way of creating assets?
                _trackingArrow = (GameObject)GameObject.Instantiate(Main.Instance.TrackingArrow);
            }
            Vector2 indPos = new Vector2(0, 0);
            indPos.x = Mathf.Max(rect.left + INDICATOR_MARGIN, Mathf.Min(rect.right - INDICATOR_MARGIN, pos.x));
            indPos.y = Mathf.Max(rect.bottom + INDICATOR_MARGIN, Mathf.Min(rect.top - INDICATOR_MARGIN, pos.y));

            _trackingArrow.GetComponent<SpriteRenderer>().color = overwhelmPct > 0 ?
                Consts.TRACKING_ARROW_COLOR_OVERWHELMED :
                Consts.TRACKING_ARROW_COLOR_NORMAL;

            _trackingArrow.transform.position = indPos;
            Util.LookAt2D(_trackingArrow.transform, transform);
            _trackingArrow.SetActive(true);
        }
        else
        {
            if (_trackingArrow != null)
            {
                _trackingArrow.SetActive(false);
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            //Debug.Log(string.Format("{0} sees ContactPoint2D.collider {1} <=> ContactPoint2D.otherCollider {2}", name, contact.collider.name, contact.otherCollider.name));

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
        var other = contact.collider;
        var self = contact.otherCollider;
        //Debug.Log(string.Format("Collision {0} to {1}, me {2}", collider.name, other.name, name));

        DebugUtil.Assert(self.gameObject == gameObject);

        var otherActor = other.GetComponent<Actor>();
        var thisIsHeroCapturingOverwhelmedMob = otherActor != null && otherActor.overwhelmPct == 1 && isPlayer;
        var thisIsOverwhelmedMobBeingCaptured = otherActor != null && overwhelmPct            == 1 && otherActor.isPlayer;

        if (thisIsHeroCapturingOverwhelmedMob)
        {
            // fire an event signalling that capture should take place
            GlobalGameEvent.Instance.FireCollisionWithOverwhelmed(other.gameObject.GetComponent<Actor>());
        }
        else if (!thisIsOverwhelmedMobBeingCaptured)
        {
            // give collision damage
            if (otherActor != null)
            {
                var damage = otherActor.collisionDamage * Random.Range(0.9f, 1.1f);
                if (damage > 0)
                {
                    if (other.gameObject.layer > self.gameObject.layer) // prevent duplicate collision sparks and damage sounds
                    {
                        var boom = Main.Instance.game.effects.GetRandomSmallExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
                        boom.transform.localPosition = contact.point;

                        GlobalGameEvent.Instance.FireExplosionSpawned(boom);

                        if (otherActor != null && otherActor.worldObjectType.health > 0 && worldObjectType.health > 0)
                        {
                            AudioSource.PlayClipAtPoint(Main.Instance.sounds.SmallCollision, contact.point);
                        }

                        //KAI: move this to a MainParticles class like MainLighting
                        var particlesContainer = transform.FindChild("collisionParticlesParent");
                        if (!particlesContainer)
                        {
                            particlesContainer = ((GameObject)GameObject.Instantiate(Main.Instance.collisionParticles)).transform;
                            particlesContainer.parent = transform;
                        }
                        particlesContainer.transform.position = contact.point;

                        var particles = particlesContainer.GetComponentInChildren<ParticleSystem>();
                        particles.Play();
                    }
                    TakeDamage(damage);
                }
            }
        }
    }
}

public sealed class PostDamageInvuln : IActorVisualBehavior
{
    readonly RateLimiter rate;
    readonly RateLimiter duration;
    readonly SpriteRenderer[] dropShadows;

    public PostDamageInvuln(Actor actor, float duration)
    {
        this.rate = new RateLimiter(0.1f);
        this.duration = new RateLimiter(duration);

        var shadows = actor.gameObject.GetComponentsInChildren<SpriteRenderer>();
        dropShadows = shadows.Length > 0 ? shadows : null;

        actor.takenDamageMultiplier = 0;
    }
    public void Update(Actor actor)
    {
        if (rate.reached)
        {
            rate.Start();

            var alpha = (rate.numStarts % 2) == 0 ? 0.25f : 1;
            SetAlpha(actor, alpha);
        }
        if (duration.reached)
        {
            // remove self
            DebugUtil.Assert(actor.visualBehavior == this);

            actor.visualBehavior = null;
            actor.takenDamageMultiplier = 1;

            SetAlpha(actor, 1);
        }
    }
    void SetAlpha(Actor actor, float alpha)
    {
        var sprite = actor.gameObject.GetComponent<SpriteRenderer>();
        Util.SetAlpha(sprite, alpha);

        if (dropShadows != null)
        {
            foreach (var shadow in dropShadows)
            {
                Util.SetAlpha(shadow, alpha);
            }
        }
    }
}
