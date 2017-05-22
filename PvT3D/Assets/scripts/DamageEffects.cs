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

    Color _startColor;
    void Start()
    {
        var material = Util.GetMaterialInChildren(gameObject);
        if (material != null && material.HasProperty("_Color"))
        {
            _startColor = material.color;
        }

        var actor = GetComponent<Actor>();
        Debug.AssertFormat(actor != null, "DamageEffects for {0} can't find Actor", name);

        actor.HealthChanged += OnHealthChanged;
        actor.ActorDying += OnActorDying;
    }
    void OnHealthChanged(Actor actor, float oldHealth, float newHealth)
    {
        if (actor.health > 0)
        {
            if (showHits)
            {
                // Flash
                StartCoroutine(DisplayHit());
            }
            if (showDamageSmoke)
            {
                // Injury
                int damageSmokeBeforeHit = Mathf.FloorToInt((actor.startHealth - oldHealth) / HEALTH_PER_DAMAGE_SMOKE);
                int damageSmokeAfterHit = Mathf.FloorToInt((actor.startHealth - newHealth) / HEALTH_PER_DAMAGE_SMOKE);

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
}
