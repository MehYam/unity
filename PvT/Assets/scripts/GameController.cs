using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class GameController
{
    public GameObject player { get; private set; }

    readonly Loader _loader;
    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        _loader = loader;
        GlobalGameEvent.Instance.MapReady += OnMapReady;
    }

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

        SpawnPlayer(_loader.GetVehicle("CYGNUS"));
        Start();
    }

    void Start()
    {
        StartNextLevel();
    }
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        var wave = _loader.levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            VehicleType v = _loader.GetVehicle(squad.enemyID);
            if (v != null)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    SpawnMob(v);
                    ++_liveEnemies;
                }
            }
            else
            {
                Debug.LogError("VehicleType not found for enemy " + squad.enemyID);
            }
        }
    }

    GameObject SpawnVehicle(VehicleType v)
    {
        var go = (GameObject)GameObject.Instantiate(v.worldObject.prefab);
        go.AddComponent<Actor>();
        go.AddComponent<DropShadow>();
        go.name = v.worldObject.name;

        var body = go.AddComponent<Rigidbody2D>();
        body.mass = v.worldObject.mass;
        body.drag = 0.1f;
        return go;
    }

    void SpawnPlayer(VehicleType plane)
    {
        var go = SpawnVehicle(plane);
        go.transform.localPosition = Vector3.zero;

        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput(plane.maxSpeed * 10000, plane.acceleration));
        behaviors.Add(new FaceForward());
        behaviors.Add(new FaceMouseOnFire());

        go.GetComponent<Actor>().behavior = behaviors;
        player = go;

        Camera.main.GetComponent<CameraFollow>().Target = go;

        GlobalGameEvent.Instance.FirePlayerSpawned(go);
    }

    void SpawnMob(VehicleType plane)
    {
        var go = SpawnVehicle(plane);
        go.name += " enemy";
        var actor = go.GetComponent<Actor>();
        actor.vehicle = plane;
        actor.behavior = EnemyActorBehaviors.Instance.Get(actor.vehicle.worldObject.behaviorKey);

        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(WorldBounds);
        bounds.Inflate(-1);

        if (Consts.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Consts.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Consts.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        go.transform.localPosition = spawnLocation;
        go.layer = Consts.MOB_LAYER;
    }

    public VehicleType GetVehicle(string type)
    {
        //KAI: cleanup
        return _loader.GetVehicle(type);
    }

    //kai: this ain't perfect
    public void SpawnMobAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon)
    {
        var go = (GameObject)GameObject.Instantiate(type.worldObject.prefab);

        var body = go.AddComponent<Rigidbody2D>();

        body.mass = 1;
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
        sprite.sortingOrder = Consts.AMMO_SORT_ORDER;

        var ammo = go.AddComponent<Actor>();
        ammo.vehicle = type;
        ammo.timeToLive = 2;
        var rotatedOffset = Consts.RotatePoint(weapon.offset, -launcher.transform.localRotation.eulerAngles.z - Consts.ACTOR_NOSE_OFFSET);

        ammo.transform.localPosition = Consts.Add(launcher.transform.localPosition, rotatedOffset);
        ammo.transform.localRotation = launcher.transform.localRotation;

        ammo.behavior = ActorBehaviorFactory.Instance.thrust;
        go.layer = Consts.MOB_AMMO_LAYER;

        //KAI: not everything should get thrust, some should just get a velocity and hold it
    }

    public void HandleCollision(ContactPoint2D contact)
    {
        var boom = (GameObject)GameObject.Instantiate(Main.Instance.Explosion);
        boom.transform.localPosition = contact.point;

        if (contact.collider.gameObject == Main.Instance.game.player)
        {
            var anim = boom.GetComponent<Animation>();
            anim.Play();
        }

        var go = contact.collider.gameObject;
        if (go.layer == Consts.MOB_LAYER)
        {
            --_liveEnemies;
            GameObject.Destroy(go);
        }

        if (_liveEnemies == 0)
        {
            StartNextWave();
        }
    }

}

public sealed class Level
{
    public sealed class Squad
    {
        public readonly string enemyID;
        public readonly int count;

        public Squad(string enemy, int count) { this.enemyID = enemy; this.count = count; }

    }
    public sealed class Wave
    {
        public readonly IList<Squad> squads;

        public Wave(IList<Squad> squads) { this.squads = squads; }
    }

    readonly IList<Wave> waves;
    int nextWave = 0;
    public Level(IList<Wave> waves) { this.waves = waves; }

    public Wave NextWave()
    {
        return nextWave <= waves.Count ? waves[nextWave++] : null;
    }
    public int numWaves { get { return waves.Count; } }
}
