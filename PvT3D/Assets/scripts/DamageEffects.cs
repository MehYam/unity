using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PvT3D.Util;

public class DamageEffects : MonoBehaviour
{
    const float HEALTH_PER_DAMAGE_SMOKE = 25;

    public bool showHits = false;
    public bool showDamageSmoke = false;
    public bool showExplosionOnDeath = false;

    Material _material;
    Color _startColor;
    void Start()
    {
        _material = Util.GetMaterialInChildren(gameObject);
        _startColor = Util.GetColor(gameObject);

        var actor = GetComponent<Actor>();
        Debug.AssertFormat(actor != null, "DamageEffects for {0} can't find Actor", name);

        actor.HealthChanged += OnHealthChanged;
        actor.ActorDying += OnActorDying;
    }
    void OnHealthChanged(Actor actor, float oldHealth, float newHealth, bool fromDamage)
    {
        if (fromDamage && actor.health > 0)
        {
            if (showHits)
            {
                // Flash
                StartCoroutine(DisplayHit());
            }
            if (showDamageSmoke)
            {
                // Injury
                int damageSmokeBeforeHit = Mathf.FloorToInt((actor.baseHealth - oldHealth) / HEALTH_PER_DAMAGE_SMOKE);
                int damageSmokeAfterHit = Mathf.FloorToInt((actor.baseHealth - newHealth) / HEALTH_PER_DAMAGE_SMOKE);

                AddDamageSmoke(damageSmokeAfterHit - damageSmokeBeforeHit);
            }
        }
    }
    void OnActorDying(Actor actor)
    {
        // Death
        RemoveDamageSmoke();
        if (showExplosionOnDeath)
        {
            var explosion = GameObject.Instantiate(Main.game.plasmaExplosionPrefab);
            explosion.transform.parent = Main.game.effectParent.transform;
            explosion.transform.position = transform.position;
        }
    }
    Transform damageSmoke;
    void AddDamageSmoke(int num)
    {
        if (num > 0)
        {
            if (damageSmoke == null)
            {
                damageSmoke = new GameObject("damageSmokeParent").transform;
                damageSmoke.parent = transform;
                damageSmoke.transform.localPosition = Vector3.zero;
            }
            for (int i = 0; i < num; ++i)
            {
                var smoke = GameObject.Instantiate(Main.game.damageSmokePrefab);
                smoke.transform.parent = damageSmoke;
                smoke.transform.localPosition = Random.insideUnitSphere * 3;
            }
        }
    }
    void RemoveDamageSmoke()
    {
        // we want damage smoke particles to stick around after death, so parent them to the effects layer
        if (damageSmoke != null)
        {
            damageSmoke.parent = Main.game.effectParent.transform;

            var ttl = damageSmoke.gameObject.AddComponent<TimeToLive>();
            ttl.seconds = 5;

            var particles = damageSmoke.GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                particle.Stop();
            }

            damageSmoke = null;
        }
    }
    //KAI: this won't work for shields because they're fading....
    IEnumerator DisplayHit()
    {
        if (_startColor != Color.black)
        {
            _material.color = new Color(1, .3f, .3f);

            yield return new WaitForSeconds(0.1f);
            _material.color = _startColor;
        }
    }
}
