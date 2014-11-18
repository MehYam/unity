using UnityEngine;
using System.Collections;

public class MainLighting : MonoBehaviour
{
    public enum LightingMode { NONE, CHEAP, DYNAMIC };
    public LightingMode lightingMode = LightingMode.DYNAMIC;

    public Light SceneLight;
    public Light PointLightPrefab;
    public Shader DynamicLightingShader;
    public GameObject CheapLightPrefab;
    public GameObject CheapLightPrefabFusion;

    // KAI: this is neat, this class works like a filter, attaching functionality when things happen -
    // but it's inconsistent with how the rest of the design works.  This functionality should either
    // be put in Actor* and Ammo* classes, or the structure of the project should follow what we're doing
    // here - but not both.

	// Use this for initialization
	void Start()
    {
        SceneLight.gameObject.SetActive(lightingMode == LightingMode.DYNAMIC);

        if (lightingMode != LightingMode.NONE)
        {
            GlobalGameEvent.Instance.ActorSpawned += OnActorSpawned;
            GlobalGameEvent.Instance.AmmoSpawned += OnAmmoSpawned;
            GlobalGameEvent.Instance.ExplosionSpawned += OnExplosionSpawned;
            GlobalGameEvent.Instance.MapReady += OnMapReady;
        }
	}
    void OnDestroy()
    {
        GlobalGameEvent.Instance.ActorSpawned -= OnActorSpawned;
        GlobalGameEvent.Instance.AmmoSpawned -= OnAmmoSpawned;
        GlobalGameEvent.Instance.ExplosionSpawned -= OnExplosionSpawned;
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
    }

    void OnMapReady(GameObject map, XRect bounds)
    {
        if (lightingMode == LightingMode.DYNAMIC)
        {
            var renderer = map.GetComponentInChildren<MeshRenderer>();
            renderer.material.shader = DynamicLightingShader;
        }
    }

    void OnActorSpawned(Actor actor)
    {
        if (lightingMode == LightingMode.DYNAMIC)
        {
            var renderer = actor.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.material.shader = DynamicLightingShader;
            }
        }
    }

    void OnAmmoSpawned(Actor actor, ActorType.Weapon weapon)
    {
        if (weapon.lit)
        {
            bool fusion = weapon.actorName == "FUSION";
            AddLight(actor.gameObject, weapon.color, fusion);
        }
    }
    void OnExplosionSpawned(GameObject explosion)
    {
        AddLight(explosion, Color.white, false);
    }
    static readonly Vector3 s_lightPosition = new Vector3(0, 0, -1);
    void AddLight(GameObject go, Color color, bool fusion)
    {
        switch (lightingMode)
        {
            case LightingMode.DYNAMIC:
                var pointLight = (Light)GameObject.Instantiate(PointLightPrefab);
                pointLight.color = color;
                pointLight.transform.parent = go.transform;
                pointLight.transform.localPosition = s_lightPosition;
                break;
            case LightingMode.CHEAP:

                var glow = (GameObject)GameObject.Instantiate(fusion ? CheapLightPrefabFusion : CheapLightPrefab);
                glow.transform.parent = go.transform;
                glow.transform.localPosition = Vector3.zero;
                glow.transform.rotation = go.transform.rotation;

                var renderer = glow.GetComponent<SpriteRenderer>();
                color.a = renderer.color.a; // preserve alpha from the glow prefab, since it's essential to our cheap lighting hack

                renderer.color = color;
                break;
        }
    }
}
