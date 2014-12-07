//#define SHOW_STRANGE_PARENTING_BUG
//#define DEBUG_FACEPLANTS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        detectFaceplants = false;
        lastFaceplantTime = float.MinValue;

        maxRotationalVelocity = Consts.MAX_MOB_ROTATION_DEG_PER_SEC;

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

    /// <summary>
    /// UNTIL A BETTER DESIGN CAN BE THOUGHT THROUGH, MOB AND PLAYER ACTORS MUST DIE HERE - ELSE,
    /// LEAKS WILL OCCUR.
    /// </summary>
    void Die()
    {
        GlobalGameEvent.Instance.FireActorDeath(this);
        if (_damageSmoke != null)
        {
            // need to do this here, else the smoke game object will get destroyed before we can detach it
            _damageSmoke.Detach();
        }
        if (_collisionParticles != null)
        {
            var expire = _collisionParticles.GetOrAddComponent<Expire>();
            expire.SetExpiry(_collisionParticles.particleSystem.duration);
        }
        GameObject.Destroy(gameObject);
    }
    void OnDestroy()
    {
        // if object pooling ever becomes necessary, this is probably a good place to start
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

            health = _actorType.attrs.maxHealth;
            if (_actorModifiers != null)
            {
                _actorModifiers = null;
            }
        }
    }
    ActorAttrs _lazyAttrs; // this is determined lazily
    public ActorAttrs attrs
    { 
        get
        {
            if (_lazyAttrs == null)
            {
                if (_actorModifiers != null && _actorModifiers.Count > 0)
                {
                    // KAI: this could be a lot cleaner at the expense of creating
                    // more temporary ActorAttrs objects
                    float maxSpeed = actorType.attrs.maxSpeed;  
                    float acceleration = actorType.attrs.acceleration;
                    float maxHealth = actorType.attrs.maxHealth;
                    foreach(var mod in _actorModifiers)
                    {
                        maxSpeed += mod.maxSpeed;
                        acceleration += mod.acceleration;
                        maxHealth += mod.maxHealth;
                    }
                    _lazyAttrs = new ActorAttrs(maxSpeed, acceleration, health);
                }
                else
                {
                    _lazyAttrs = _actorType.attrs;
                }
            }
            return _lazyAttrs;
        }
    }

    static readonly ActorType.WeaponAttrs IDENTITY = new ActorType.WeaponAttrs(1, 1, 1, 1);

    ActorType.WeaponAttrs _lazyWeaponMods;
    public ActorType.WeaponAttrs weaponMods
    {
        get
        {
            if (_lazyWeaponMods == null)
            {
                if (_weaponModifiers != null && _weaponModifiers.Count > 0)
                {
                    float damage = 1;
                    float rate = 1;
                    float ttl = 1;
                    float chargeSeconds = 1;
                    foreach (var mod in _weaponModifiers)
                    {
                        damage *= mod.damage;
                        rate *= mod.rate;
                        ttl *= mod.ttl;
                        chargeSeconds *= mod.chargeSeconds;
                    }
                    _lazyWeaponMods = new ActorType.WeaponAttrs(damage, rate, ttl, chargeSeconds);
                }
                else
                {
                    _lazyWeaponMods = IDENTITY;
                }
            }
            return _lazyWeaponMods;
        }
    }

    IList<ActorAttrs> _actorModifiers;
    /// <summary>
    /// Adds the modifier to the list of attributes exhibited by this actor.  Modifiers instances are unique;  they cannot be added multiple times.
    /// </summary>
    /// <param name="modifier">The modifier to add</param>
    public void AddActorModifier(ActorAttrs modifier)
    {
        DebugUtil.Assert(modifier != null);
        if (_actorModifiers == null)
        {
            _actorModifiers = new List<ActorAttrs>();
        }
        if (!_actorModifiers.Contains(modifier))
        {
            _actorModifiers.Add(modifier);
            _lazyAttrs = null;
        }
    }
    /// <summary>
    /// Removes the modifier from the list of attributes exhibited by this actor.
    /// </summary>
    /// <param name="modifier">The modifier to remove</param>
    public void RemoveActorModifier(ActorAttrs modifier)
    {
        DebugUtil.Assert(modifier != null);
        if (_actorModifiers != null && _actorModifiers.Contains(modifier))
        {
            _actorModifiers.Remove(modifier);
            _lazyAttrs = null;
        }
    }

    //KAI: so much copy pasta with regular modifiers.  Let this brew for a while, consolidate it when it seems ready.
    IList<ActorType.WeaponAttrs> _weaponModifiers;
    public void AddWeaponModifier(ActorType.WeaponAttrs modifier)
    {
        DebugUtil.Assert(modifier != null);
        if (_weaponModifiers == null)
        {
            _weaponModifiers = new List<ActorType.WeaponAttrs>();
        }
        if (!_weaponModifiers.Contains(modifier))
        {
            _weaponModifiers.Add(modifier);
            _lazyWeaponMods = null;
        }
    }
    public void RemoveWeaponModifier(ActorType.WeaponAttrs modifier)
    {
        DebugUtil.Assert(modifier != null);
        if (_weaponModifiers != null && _weaponModifiers.Contains(modifier))
        {
            _lazyWeaponMods = null;
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
    public float lastFaceplantTime { get; set; }
    public bool detectFaceplants { get; set; }
    public Vector2 lastThrust { get; private set; }
    public ITarget target { get; set; }

    public void AddThrust(Vector2 force)
    {
        rigidbody2D.AddForce(force);
        lastThrust = force;
    }

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
        var rate = new Timer(duration);
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

#if DEBUG_FACEPLANTS
    float _last = 0;
#endif

    IActorBehavior _overwhelmBehavior;  // currently used for overwhelming
    protected virtual void FixedUpdate()
    {
#if DEBUG_FACEPLANTS
        if (Time.fixedTime - _last > 0.25f)
        {
            var lookAt = Util.GetLookAtVector(transform.rotation.eulerAngles.z);
            bool intersect = Physics2D.Raycast(transform.position, lookAt, 1, Consts.ENVIRONMENT_LAYER_MASK);
            Debug.DrawLine(transform.position, transform.position + (Vector3)lookAt, intersect ? Color.red : Color.white, 2);

            _last = Time.fixedTime;
        }
#endif

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

        if (rigidbody2D != null && attrs.maxSpeed > 0 && rigidbody2D.velocity.sqrMagnitude > attrs.sqrMaxSpeed)
        {
            rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, attrs.maxSpeed);
        }
        if (((expireTime != EXPIRY_INFINITE) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            Die();
        }
    }
    static readonly Vector3 HEALTH_BAR_POSITION = new Vector3(0, 0.5f);
    static readonly Vector3 OVERWHELM_BAR_POSITION = new Vector3(0, -0.5f);
    void Update()
    {
        var showHealth = showsHealthBar && _health > 0 && _health < attrs.maxHealth;
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
            _healthBar.percent = health / attrs.maxHealth;
            _healthBar.transform.position = transform.position + HEALTH_BAR_POSITION;

            if (!isHero)
            {
                UpdateDamageSmoke();
            }
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
        var pct = health / attrs.maxHealth;
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
            // i.e. we were hurt, but are now healed and need to stop the smoke
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

            Main.Instance.game.PlaySound(Sounds.GlobalEvent.OVERWHELM, transform.position);
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
            //Debug.Log(string.Format("{0}.OnCollisionEnter2D : {1}, {2}", name, contact.otherCollider.name, contact.collider.name));

            var collidee = contact.otherCollider.gameObject;
            var other = contact.collider.gameObject;
            if (gameObject == collidee && gameObject.layer != other.layer)
            {
                HandleCollision(other, contact.point, contact.normal, collision.relativeVelocity);
                break;
            }
        }
    }

    protected virtual void HandleCollision(GameObject other, Vector2 point, Vector2 normal, Vector2 relativeVelocity)
    {
        var otherActor = other.GetComponent<Actor>();
        if (otherActor == null)
        {
            // might be a head-on wall collision, do our faceplant detection
            if (detectFaceplants && other.name == "Collision")
            {
                var lookAt = Util.GetLookAtVector(transform.rotation.eulerAngles.z);
                if (Physics2D.Raycast(transform.position, lookAt, Consts.FACEPLANT_CHECK_DISTANCE))
                {
                    lastFaceplantTime = Time.fixedTime;
                }
            }
        }
        else
        {
            // give out collision damage, except when capturing a mob.
            var thisIsPlayerCapturing = isPlayer && otherActor.overwhelmPct == 1;
            var thisIsMobBeingCaptured = otherActor.isPlayer && overwhelmPct == 1;
            if (thisIsPlayerCapturing)
            {
                GlobalGameEvent.Instance.FirePlayerCollisionWithOverwhelmed(other.gameObject.GetComponent<Actor>());
            }
            else if (!thisIsMobBeingCaptured)
            {
                //Debug.Log(string.Format("giving {0} damage to {1}", collisionDamage, other.name));
                otherActor.TakeDamage(collisionDamage);
                if (otherActor.isPlayer)
                {
                    otherActor.GrantInvuln(Consts.POST_DAMAGE_INVULN);
                }
            }

            // prevent mutual collision sparks and thumps
            if (other.layer > gameObject.layer)
            {
                Main.Instance.game.PlaySound(Sounds.GlobalEvent.MOBCOLLISION, point);
            }
        }
        if (isAmmo)
        {
            HandleCollisionAsAmmo(other, point);
        }
    }

    GameObject _collisionParticles;
    void HandleCollisionAsAmmo(GameObject other, Vector2 point)
    {
        var otherActor = other.GetComponent<Actor>();
        if (otherActor != null && otherActor.reflectsAmmo)
        {
            // bounce off a shield => switch allegiance and damage the other side
            gameObject.layer = gameObject.layer == (int)Consts.CollisionLayer.MOB_AMMO ?
                (int)Consts.CollisionLayer.FRIENDLY_AMMO :
                (int)Consts.CollisionLayer.MOB_AMMO;

            // replace the "realistic" collision with one that looks better - otherwise lasers
            // look wonky
            rigidbody2D.angularVelocity = 0;
            rigidbody2D.velocity = rigidbody2D.velocity.normalized * attrs.maxSpeed;
            ActorBehaviorFactory.Instance.faceForward.FixedUpdate(this);
        }
        else
        {
            // show sparks and die
            if (_collisionParticles == null)
            {
                _collisionParticles = ((GameObject)GameObject.Instantiate(Main.Instance.assets.collisionParticles));
                _collisionParticles.transform.parent = Main.Instance.EffectParent.transform;
            }
            _collisionParticles.transform.position = point;
            _collisionParticles.particleSystem.Play();

            TakeDamage(1);
        }
    }
}
