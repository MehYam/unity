using UnityEngine;
using System.Collections;

public class MainLighting : MonoBehaviour
{
    public bool UseDynamicLighting = true;

    public Light SceneLight;
    public Light PointLight;
    public Shader LightingShader;

    // KAI: this is neat, this class works like a filter, attaching functionality when things happen -
    // but it's inconsistent with how the rest of the design works.  This functionality should either
    // be put in Actor* and Ammo* classes, or the structure of the project should follow what we're doing
    // here - but not both.

	// Use this for initialization
	void Start()
    {
        SceneLight.gameObject.SetActive(UseDynamicLighting);

        if (UseDynamicLighting)
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
        var renderer = map.GetComponentInChildren<MeshRenderer>();
        renderer.material.shader = LightingShader;
    }

    void OnActorSpawned(Actor actor)
    {
        var renderer = actor.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            renderer.material.shader = LightingShader;
        }
    }

    void OnAmmoSpawned(Actor actor, WorldObjectType.Weapon weapon)
    {
        if (weapon.lit)
        {
            AddLight(actor.gameObject, weapon.color);
        }
    }
    void OnExplosionSpawned(GameObject explosion)
    {
        AddLight(explosion, Color.white);
    }
    static readonly Vector3 s_lightPosition = new Vector3(0, 0, -1);
    void AddLight(GameObject go, Color color)
    {
        var pointLight = (Light)GameObject.Instantiate(PointLight);
        pointLight.color = color;
        pointLight.transform.parent = go.transform;
        pointLight.transform.localPosition = s_lightPosition;
    }
}
