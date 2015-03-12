//#define SHOW_STRANGE_PARENTING_BUG
//#define DEBUG_FACEPLANTS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.DOM;
using PvT.Util;

public class Actor : MonoBehaviour
{
    //KAI: these items are evidence of holes in the design.  Separate Tank, Turret, and Plane classes would remove the
    // need for these, as well as using real hinges for the turrets
    public bool FaceMouseOnFire = true;
    public bool AddRigidbody = true;

    public bool reflectsAmmo { get; set; }
    public bool showsHealthBar { get; set; }
    public bool explodesOnDeath { get; set; }
    public bool isCapturable { get; set; }
    public float lastFaceplantTime { get; set; }
    public bool detectFaceplants { get; set; }
    public Vector2 lastThrust { get; private set; }
    public ITarget target { get; set; }

    public float expireTime { get; private set; }

    //KAI: these should eventually go away
    public bool isPlayer { get { return GetComponent<Player>() != null; } }
    public bool isAmmo { get { return GetComponent<Ammo>() != null; } }
    public bool isHero { get { return actorType.name == "HERO"; } }

    public Actor()
    {
        expireTime = EXPIRE_NEVER;
        explodesOnDeath = true;
        showsHealthBar = true;
        firingEnabled = true;
        thrustEnabled = true;
        immortal = false;

        detectFaceplants = false;
        lastFaceplantTime = float.MinValue;

        maxRotationalVelocity = Consts.MAX_MOB_ROTATION_DEG_PER_SEC;

        takenDamageMultiplier = 1;
        _health = 1;  //KAI: unity crashes if we call the setter here, because it fires off a bunch of events out of the constructor, which is bad

        behaviorEnabled = true;
    }

    ActorType _actorType;
    public ActorType actorType
    {
        get { return _actorType; }
        private set
        {
            if (value == null)
            {
                Debug.LogError(string.Format("No actor type found for actor name {0}", name));
            }
            if (_actorType != null)
            {
                Debug.LogWarning("Actor.actorType is being overwritten?");
            }
            _actorType = value;

            health = _actorType.attrs.maxHealth;

            if (!isHero)
            {
                collisionDamage = health / 4;
            }

            if (value.dropShadow)
            {
                gameObject.AddComponent<DropShadow>();
            }

            if (_actorModifiers != null)
            {
                _actorModifiers = null;
            }
        }
    }

    protected virtual void Start()  // KAI: interesting Unity gotcha - must document somewhere
    {
        // look up the stats for this actor by name
        actorType = Main.Instance.game.loader.GetActorTypeFromAssetId(name);

        // hook up the physics
        if (AddRigidbody)
        {
            var body = gameObject.AddComponent<Rigidbody2D>();
            body.mass = float.IsNaN(actorType.mass) ? 0 : actorType.mass;
            body.drag = 0.5f;
            body.angularDrag = 5;
            if (actorType.mass < 0)
            {
                body.isKinematic = true;
            }
        }
        GetComponent<Collider2D>().sharedMaterial = Main.Instance.assets.Bounce;

        // miscellaneous
        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            var ourSprite = GetComponent<SpriteRenderer>();
            trail.sortingLayerID = ourSprite.sortingLayerID;
            trail.sortingOrder = ourSprite.sortingOrder - 1;
        }

        GlobalGameEvent.Instance.FireActorSpawned(this);
    }

    /// <summary>
    /// UNTIL A BETTER DESIGN CAN BE THOUGHT THROUGH, MOB AND PLAYER ACTORS MUST DIE HERE - ELSE,
    /// LEAKS WILL OCCUR.
    /// </summary>
    void Die()
    {
        SendMessage("PreActorDie", this, SendMessageOptions.DontRequireReceiver);

        GlobalGameEvent.Instance.FireActorDeath(this);
        if (_damageSmoke != null)
        {
            // need to do this here, else the smoke game object will get destroyed before we can detach it
            _damageSmoke.Detach();
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

    static public readonly float EXPIRE_NOW = 0;
    static public readonly float EXPIRE_NEVER = -1;
    public void SetExpiry(float secondsFromNow)
    {
        expireTime = secondsFromNow == EXPIRE_NEVER ? EXPIRE_NEVER : Time.fixedTime + secondsFromNow;
    }

    public void AddThrust(Vector2 force)
    {
        GetComponent<Rigidbody2D>().AddForce(force);
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
    public IActorBehavior behaviorOverride { get; set; }  // currently used for overwhelming
    public bool behaviorEnabled { get; set; }
    
    public float maxRotationalVelocity { get; set; }
    public bool firingEnabled { get; set; }
    public bool trackingArrowEnabled { get; set; }
    public bool thrustEnabled { get; set; }
    public bool immortal { get; set; }

    public float takenDamageMultiplier { get; set; }
    public float collisionDamage { get; set; }

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

    void FixedUpdate()
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
        if (behaviorEnabled)
        {
            if (behaviorOverride != null)
            {
                behaviorOverride.FixedUpdate(this);
            }
            else if (behavior != null)
            {
                behavior.FixedUpdate(this);
            }
        }

        if (GetComponent<Rigidbody2D>() != null && attrs.maxSpeed > 0 && GetComponent<Rigidbody2D>().velocity.sqrMagnitude > attrs.sqrMaxSpeed)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, attrs.maxSpeed);
        }
        if (((expireTime != EXPIRE_NEVER) && Time.fixedTime >= expireTime) || (health <= 0))
        {
            Die();
        }
    }
    static readonly Vector3 HEALTH_BAR_POSITION = new Vector3(0, 0.5f);
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
                Main.Instance.ParentEffect(bar.transform);
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
    }
    sealed class DamageSmoke
    {
        public readonly GameObject go;
        public DamageSmoke(Actor host)
        {
            go = ((GameObject)GameObject.Instantiate(Main.Instance.assets.damageSmokeParticles));
            go.transform.parent = host.transform;
            go.transform.localPosition = Util.ScatterRandomly(0.4f);
            go.GetComponent<ParticleSystem>().Play();
        }
        public void Detach()
        {
            if (go != null)
            {
                Main.Instance.ParentEffect(go.transform);
                go.GetComponent<ParticleSystem>().Stop();

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

            //KAI: need to add back this functionality somehow
            //_trackingArrow.GetComponent<SpriteRenderer>().color = overwhelmPct > 0 ?
            //    Consts.TRACKING_ARROW_COLOR_OVERWHELMED :
            //    Consts.TRACKING_ARROW_COLOR_NORMAL;

            _trackingArrow.GetComponent<SpriteRenderer>().color = Consts.TRACKING_ARROW_COLOR_NORMAL;

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
#if FOO
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
#endif

    static RaycastHit2D[] s_raycastResults = new RaycastHit2D[1];
    protected virtual void HandleCollision(GameObject other, Vector2 point, Vector2 normal, Vector2 relativeVelocity)
    {
        var otherActor = other.GetComponent<Actor>();
        if (otherActor == null)
        {
            // might be a head-on wall collision, do our faceplant detection
            //KAI: still necessary?  MobAI already handles this... the only difference here is that
            // we're casting out the nose - but maybe it makes sense to centralize how all this is done,
            // and clean up the code a bit
            if (detectFaceplants && other.name == "Collision")
            {
                var lookAt = Util.GetLookAtVector(transform.rotation.eulerAngles.z);
                int results = Physics2D.RaycastNonAlloc(transform.position, lookAt, s_raycastResults, Consts.FACEPLANT_CHECK_DISTANCE);
                if (results > 0)
                {
                    lastFaceplantTime = Time.fixedTime;
                }
            }
        }
        else
        {
            // give out collision damage, except when capturing a mob.
            var thisIsPlayerCapturing = isPlayer;// && otherActor.overwhelmPct == 1;
            var thisIsMobBeingCaptured = otherActor.isPlayer;// && overwhelmPct == 1;
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
    }

    void OnDamagingCollision(Actor other)
    {
        TakeDamage(other.collisionDamage);
    }
}
