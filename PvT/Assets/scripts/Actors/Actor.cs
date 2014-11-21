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

        maxRotationalVelocity = Consts.MAX_MOB_ROTATION_DEG_PER_SEC;
        speedModifier = ActorMovementModifier.IDENTITY;

        behaviorEnabled = true;
    }

    protected virtual void Start()  // KAI: interesting Unity gotcha - must document somewhere
    {
        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            var ourSprite = GetComponent<SpriteRenderer>();
            trail.sortingLayerID = ourSprite.sortingLayerID;
            trail.sortingOrder = ourSprite.sortingOrder - 1;
        }
        GlobalGameEvent.Instance.FireActorSpawned(this);
    }

    public bool isPlayer { get { return Main.Instance.game.player == gameObject; } }
    public bool isHero { get { return actorType.name == "HERO"; } }
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
        if (_overwhelm.bar != null)
        {
            GameObject.Destroy(_overwhelm.bar.gameObject);
        }
    }

    ActorType _actorType;
    public ActorType actorType 
    {
        get { return _actorType; }
        set
        {
            _actorType = value;
            health = value.attrs.maxHealth;
        }
    }

    public float expireTime { get; private set; }

    static public readonly float EXPIRY_IMMEDIATE = -1;
    static public readonly float EXPIRY_INFINITE = 0;
    public void SetExpiry(float secondsFromNow)
    {
        expireTime = secondsFromNow == EXPIRY_INFINITE ? EXPIRY_INFINITE : Time.fixedTime + secondsFromNow;
    }

    public bool isAmmo { get; set; }
    public bool reflectsAmmo { get; set; }
    public bool showsHealthBar{ get; set; }
    public bool explodesOnDeath { get; set; }
    public bool isCapturable { get; set; }

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
    ActorMovementModifier _speedModifier;
    public ActorMovementModifier speedModifier
    { 
        get { return _speedModifier; }
        set
        {   
            _speedModifier = value == null ? ActorMovementModifier.IDENTITY : value;
        }
    }
    public float maxSpeed
    {
        get
        {
            var speed = actorType.attrs.maxSpeed * speedModifier.speedMultiplier;
            return isPlayer ? speed * Consts.PLAYER_SPEED_MULTIPLIER : speed;
        }
    }
    public float acceleration
    {
        get
        {
            var v = actorType;

            // Our config wants acceleration to be absolute, without being slowed by mass.  Therefore,
            // derive the force required by multiplying it by mass.  If we start using drag more, 
            // that will have to compensate for as well.
            var accel = v.attrs.acceleration * speedModifier.accelerationMultiplier * actorType.mass;
            return isPlayer ? accel * Consts.PLAYER_ACCEL_MULTIPLIER : accel;
        }
    }
    public IActorBehavior behavior { get; set; }
    public bool behaviorEnabled { get; set; }
    
    public float maxRotationalVelocity { get; set; }
    public bool firingEnabled { get; set; }
    public bool trackingArrowEnabled { get; set; }
    public bool thrustEnabled { get; set; }
    public bool immortal { get; set; }

    public float takenDamageMultiplier { get; set; }
    public float collisionDamage { get; set; }

    public int attachedHerolings { get; set; }
    public float overwhelmPct
    {
        get { return health == 0 ? 0 : Mathf.Min(attachedHerolings * Consts.HEROLING_HEALTH_OVERWHELM / health, 1); }
    }

    struct OverwhelmBarState
    {
        // this allows us to not generate strings every frame
        public ProgressBar bar;
        public int lastNumerator;
        public int lastDenominator;
        public bool Update(int num, int denom)
        {
            if (lastNumerator != num || lastDenominator != denom)
            {
                lastNumerator = num;
                lastDenominator = denom;
                return true;
            }
            return false;
        }
    }
    OverwhelmBarState _overwhelm;
    ProgressBar _healthBar;
    public void TakeDamage(float damage)
    {
        var effectiveDamage = takenDamageMultiplier * damage;
        //Debug.Log(string.Format("{0} takes damage {1} effective {2} health {3}", name, damage, effectiveDamage, health));
        
        if (effectiveDamage > 0)
        {
            this.health -= effectiveDamage;
        }
    }

    Coroutine _currentInvulnerability;
    public void GrantInvuln(float duration)
    {
        //KAI: unity currently crashes really easily on StopCoroutine, so this logic is a little messed up.
        // Our previous Invuln IActorBehavior worked better, but when the bug's fixed, this is the better
        // way to do it.
        if (_currentInvulnerability != null)
        {
            //StopCoroutine(_currentInvulnerability);
        }
        else
        {
            _currentInvulnerability = StartCoroutine(PostDamageInvulnerability(duration));
        }
    }
    IEnumerator PostDamageInvulnerability(float duration)
    {
        takenDamageMultiplier = 0;

        const float PULSE_SECONDS = 0.1f;
        var rate = new RateLimiter(duration);
        var fader = gameObject.GetOrAddComponent<Fader>();
        while (!rate.reached)
        {
            float targetAlpha = (fader.alpha < 1) ? 1 : 0.25f;
            fader.Fade(targetAlpha, PULSE_SECONDS, false);

            yield return new WaitForSeconds(PULSE_SECONDS);
        }

        fader.Fade(1, 0, false);
        takenDamageMultiplier = 1;

        yield return new WaitForEndOfFrame();
        _currentInvulnerability = null;
    }

    IActorBehavior _overwhelmBehavior;  // currently used for overwhelming
    protected virtual void FixedUpdate()
    {
        DetectChangeInOverwhelm();
        if (behaviorEnabled)
        {
            if (_overwhelmBehavior != null)
            {
                _overwhelmBehavior.FixedUpdate(this);
            }
            else if (behavior != null)
            {
                behavior.FixedUpdate(this);
            }
        }

        if (rigidbody2D != null && maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > actorType.attrs.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, maxSpeed);
        }
        if (((expireTime != EXPIRY_INFINITE) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            //KAI: THIS HAS TO BE THE AUTHORITATIVE PLACE WHERE AN ACTOR DIES
            // - think about this design some.
            GlobalGameEvent.Instance.FireActorDeath(this);
            if (_damageSmoke != null)
            {
                _damageSmoke.Detach();
            }
            GameObject.Destroy(gameObject);
        }
    }
    static readonly Vector3 HEALTH_BAR_POSITION = new Vector3(0, 0.5f);
    static readonly Vector3 OVERWHELM_BAR_POSITION = new Vector3(0, -0.5f);
    void Update()
    {
        var showHealth = showsHealthBar && _health > 0 && _health < actorType.attrs.maxHealth;
        if (showHealth)
        {
            if (_healthBar == null)
            {
                var bar = (GameObject)GameObject.Instantiate(Main.Instance.assets.HealthProgressBar);
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
            _healthBar.percent = health / actorType.attrs.maxHealth;
            _healthBar.transform.position = transform.position + HEALTH_BAR_POSITION;

            UpdateDamageSmoke();
        }
        else if (_healthBar != null)
        {
            _healthBar.gameObject.SetActive(false);
        }

        //KAI: copy pasta +1 w/ health bar, might be worth generalizing
        if (attachedHerolings > 0)
        {
            if (_overwhelm.bar == null)
            {
                var bar = (GameObject)GameObject.Instantiate(Main.Instance.assets.OverwhelmProgressBar);
                _overwhelm.bar = bar.GetComponent<ProgressBar>();

                bar.transform.parent = Main.Instance.EffectParent.transform;
            }
            _overwhelm.bar.gameObject.SetActive(true);
            _overwhelm.bar.percent = overwhelmPct;
            if (_overwhelm.Update(attachedHerolings, Mathf.CeilToInt(health / Consts.HEROLING_HEALTH_OVERWHELM)))
            {
                _overwhelm.bar.text = string.Format("{0}/{1}", _overwhelm.lastNumerator, _overwhelm.lastDenominator);
            }
            _overwhelm.bar.transform.position = transform.position + OVERWHELM_BAR_POSITION;
        }
        else if (_overwhelm.bar != null)
        {
            _overwhelm.bar.gameObject.SetActive(false);
        }
    }
    sealed class DamageSmoke
    {
        public readonly GameObject go;
        public DamageSmoke(Actor host)
        {
            go = ((GameObject)GameObject.Instantiate(Main.Instance.assets.damageSmokeParticles));
            go.transform.parent = host.transform;
            go.transform.localPosition = Util.ScatterRandomly(0.4f);
            go.particleSystem.Play();
        }
        public void Detach()
        {
            if (go != null)
            {
                go.transform.parent = Main.Instance.EffectParent.transform;
                go.particleSystem.Stop();

                var expiry = go.AddComponent<Expire>();
                expiry.SetExpiry(5);
            }
        }
    }
    DamageSmoke _damageSmoke;
    void UpdateDamageSmoke()
    {
        var pct = health / actorType.attrs.maxHealth;
        if (pct < 0.33)
        {
            /////// PARTICLE STUFF
            //KAI: move this to a MainParticles class like MainLighting
            if (_damageSmoke == null)
            {
                _damageSmoke = new DamageSmoke(this);
            }
        }
        else if (_damageSmoke != null)
        {
            _damageSmoke.Detach();
            _damageSmoke = null;
        }
        /////// END PARTICLE STUFF
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

            var blinker = (GameObject)GameObject.Instantiate(Main.Instance.assets.OverwhelmedIndicator);
            blinker.transform.parent = transform;
            blinker.transform.localPosition = Vector3.zero;
            blinker.name = Consts.BLINKER_TAG;

            Main.Instance.game.PlaySound(Main.Instance.sounds.HerolingCapture, transform.position);
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
                _trackingArrow = (GameObject)GameObject.Instantiate(Main.Instance.assets.TrackingArrow);
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

    GameObject _collisionParticles;
    protected virtual void HandleCollision(ContactPoint2D contact)
    {
        var self = contact.otherCollider;
        var other = contact.collider;
        
        DebugUtil.Assert(self.gameObject == gameObject, "Confusion of self != self in HandleCollision");
        
        //Debug.Log(string.Format("Collision {0} to {1}, me {2}", collider.name, other.name, name));

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
            // take collision damage from the other
            if (otherActor != null)
            {
                if (isAmmo && otherActor.reflectsAmmo)
                {
                    // we're ammo being bounced by a shield - switch allegiance
                    gameObject.layer = gameObject.layer == (int)Consts.CollisionLayer.MOB_AMMO ? 
                        (int)Consts.CollisionLayer.FRIENDLY_AMMO :
                        (int)Consts.CollisionLayer.MOB_AMMO;

                    // replace the "realistic" collision with one that looks better - otherwise lasers
                    // look wonky
                    rigidbody2D.angularVelocity = 0;
                    rigidbody2D.velocity = rigidbody2D.velocity.normalized * actorType.attrs.maxSpeed;
                    ActorBehaviorFactory.Instance.faceForward.FixedUpdate(this);
                }
                else
                {
                    TakeCollisionDamage(contact);
                }
            }
        }
    }
    void TakeCollisionDamage(ContactPoint2D contact)
    {
        var self = contact.otherCollider;
        var other = contact.collider;
        var otherActor = other.GetComponent<Actor>();

        var damage = otherActor.collisionDamage * Random.Range(0.9f, 1.1f);
        if (isAmmo && damage == 0)
        {
            // if we're ammo, ensure that we take at least one point of collision damage (this is a hack to prevent
            // the hero from reflecting ammo like lasers, since the hero imparts no collision damage)
            damage = 1;
        }
        if (damage > 0)
        {
            if (other.gameObject.layer > self.gameObject.layer) // prevent duplicate collision sparks and damage sounds
            {
                var boom = Main.Instance.game.effects.GetRandomSmallExplosion().ToRawGameObject(Consts.SortingLayer.EXPLOSIONS);
                boom.transform.localPosition = contact.point;

                GlobalGameEvent.Instance.FireExplosionSpawned(boom);

                if (otherActor != null)
                {
                    Main.Instance.game.PlaySound(Main.Instance.sounds.SmallCollision, contact.point);
                }
            }
            //Debug.Log(string.Format("{0} (health {3}) taking {1} damage from {2}", name, damage, other.name, health));

            /////// PARTICLE STUFF
            //KAI: move this to a MainParticles class like MainLighting
            if (_collisionParticles == null)
            {
                _collisionParticles = ((GameObject)GameObject.Instantiate(Main.Instance.assets.collisionParticles));
                _collisionParticles.transform.parent = transform;
            }
            _collisionParticles.transform.position = contact.point;
            _collisionParticles.particleSystem.Play();
            /////// END PARTICLE STUFF

            TakeDamage(damage);
            if (isPlayer)
            {
                GrantInvuln(Consts.POST_DAMAGE_INVULN);
            }
        }
    }
}
