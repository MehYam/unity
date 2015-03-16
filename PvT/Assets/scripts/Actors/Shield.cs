using UnityEngine;
using System.Collections;

using PvT.Util;

// The only reason this code is not in ShieldWeaponController is that it needs to run after Ammo.Start()
public sealed class Shield : MonoBehaviour
{
    public Actor launcher { get; set; }

	// Use this for initialization
    Vector2 _localShieldPos;
	void Start()
    {
        DebugUtil.Assert(launcher != null);
        DebugUtil.Assert(launcher.actorType.HasShieldWeapon);

        // init the Actor
        var shieldActor = GetComponent<Actor>();

        // shield inherits the health of the launcher, effectively doubling the launcher's health for a time.
        shieldActor.health = launcher.actorType.attrs.maxHealth;
        shieldActor.SetExpiry(Actor.EXPIRE_NEVER);
        shieldActor.explodesOnDeath = false;
        shieldActor.showsHealthBar = false;
        shieldActor.reflectsAmmo = true;

        shieldActor.behavior = ActorBehaviorFactory.Instance.CreateFadeWithHealthAndExpiry(shieldActor.health);

        shieldActor.collisionDamage = launcher.actorType.weapons[0].attrs.damage;

        // position the shield
        var weapon = launcher.actorType.weapons[0];

        Util.PrepareLaunch(launcher.transform, transform, weapon.offset, weapon.angle);
        transform.parent = launcher.transform;
        _localShieldPos = transform.localPosition;

        GlobalGameEvent.Instance.FireAmmoSpawned(shieldActor, weapon);
	}

    void LateUpdate()
    {
        transform.localPosition = _localShieldPos;
    }

    void OnDamagingCollision(Actor a)
    {
        Debug.Log("shield.OnDamagingCollision");
    }

    void OnEndFiring()
    {
return;
        // release shield
        var shieldActor = GetComponent<Actor>();
        shieldActor.SetExpiry(launcher.actorType.weapons[0].attrs.ttl);

        transform.parent = null;
        gameObject.AddComponent<Rigidbody2D>();
        GetComponent<Rigidbody2D>().drag = 1;
        GetComponent<Rigidbody2D>().mass = 50;

        var shieldVelocity = launcher.GetComponent<Rigidbody2D>().velocity;

        shieldVelocity += Util.GetLookAtVector(transform.rotation.eulerAngles.z) * Consts.SHIELD_BOOST;

        GetComponent<Rigidbody2D>().velocity = shieldVelocity;

        enabled = false;
    }
}
