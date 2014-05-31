using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public sealed class GameState
{
    readonly Dictionary<string, VehicleType> _vehicleLookup = new Dictionary<string,VehicleType>();  // need ReadOnlyDictionary here
    readonly IList<Level> _levels;
    public GameState(string strVehicles, string strAmmo, string strLevels)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        LoadVehicles(strVehicles, "planes/", _vehicleLookup);
        LoadVehicles(strAmmo, "ammo/", _vehicleLookup);

        _levels = LoadLevels(strLevels);

        GlobalGameEvent.Instance.MapReady += OnMapReady;
    }

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

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

    const int ENEMY_LAYER = 8;
    int _liveEnemies = 0;
    void StartNextWave()
    {
        var wave = _levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            var plane = _vehicleLookup[squad.enemyID];
            Debug.Log("planes/" + plane.assetID);

            for (int i = 0; i < squad.count; ++i)
            {
                SpawnEnemyPlane(plane);
                ++_liveEnemies;
            }
        }
    }

    void SpawnEnemyPlane(VehicleType plane)
    {
        var go = (GameObject)GameObject.Instantiate(plane.prefab);
        go.AddComponent<DropShadow>();
        var body = go.AddComponent<Rigidbody2D>();

        body.mass = plane.mass;
        body.drag = 1;

        var actor = go.AddComponent<Actor>();
        actor.vehicle = plane;
        actor.behavior = EnemyActorBehaviors.Instance.Get(actor.vehicle.behaviorKey);

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
        go.layer = ENEMY_LAYER;
    }

    public VehicleType GetVehicle(string type)
    {
        return _vehicleLookup[type];
    }

    //kai: this ain't perfect
    public void SpawnAmmo(Actor launcher, VehicleType type)
    {
        var go = (GameObject)GameObject.Instantiate(type.prefab);
        var body = go.AddComponent<Rigidbody2D>();

        body.mass = 1;
        body.drag = 0;

        var actor = go.AddComponent<Actor>();
        actor.vehicle = type;
        actor.timeToLive = 2;
        actor.transform.localPosition = launcher.transform.localPosition;
        actor.behavior = ActorBehaviorFactory.Instance.thrust;

        //KAI: not everything should get thrust, some should just get a velocity and hold it
    }

    public void HandleCollision(ContactPoint2D contact)
    {
        var boom = (GameObject)GameObject.Instantiate(Main.Instance.Explosion);
        boom.transform.localPosition = contact.point;

        if (contact.collider.gameObject == Main.Instance.Player)
        {
            var anim = boom.GetComponent<Animation>();
            anim.Play();

            Debug.Log("player collision");
        }

        //var go = contact.collider.gameObject;
        //if (go.layer == ENEMY_LAYER)
        //{
        //    --_liveEnemies;
        //}
        //GameObject.Destroy(go);

        //if (_liveEnemies == 0)
        //{
        //    StartNextWave();
        //}
    }

    static void LoadVehicles(string enemyJSON, string path, Dictionary<string, VehicleType> results)
    {
        var json = MJSON.hashtableFromJson(enemyJSON);
        foreach (DictionaryEntry entry in json)
        {
            var name = (string)entry.Key;
            var enemy = (Hashtable)entry.Value;
            var assetID = MJSON.SafeGetValue(enemy, "asset");

            Debug.Log("loading " + assetID);

            // load the asset and extract the firepoints
            var prefab = Resources.Load<GameObject>(path + assetID);
            VehicleType.FirePoint[] firePoints = null;
            if (prefab != null)
            {
                var firePointGameObjects = prefab.GetComponentsInChildren<FirePoint>();
                var tmp = new List<VehicleType.FirePoint>(firePointGameObjects.Length);
                foreach (var point in firePointGameObjects)
                {
                    tmp.Add(new VehicleType.FirePoint(
                        new Vector2(point.transform.localPosition.x, point.transform.localPosition.y),
                        point.transform.localEulerAngles.z)
                    );

                    GameObject.Destroy(point.gameObject);
                }
                firePoints = tmp.ToArray();
            }

            results[name] = new VehicleType(
                name,
                assetID,
                MJSON.SafeGetInt(enemy, "health"),
                MJSON.SafeGetFloat(enemy, "mass"),
                MJSON.SafeGetFloat(enemy, "maxSpeed"),
                MJSON.SafeGetFloat(enemy, "acceleration"),
                MJSON.SafeGetFloat(enemy, "inertia"),
                MJSON.SafeGetFloat(enemy, "collision"),
                MJSON.SafeGetInt(enemy, "reward"),
                MJSON.SafeGetValue(enemy, "behavior"),
                firePoints,
                prefab
            );
        }
    }

    static IList<Level> LoadLevels(string strLevels)
    {
        var retval = new List<Level>();

        var levelStrings = strLevels.Split('#');
        foreach (var strLevel in levelStrings)
        {
            var level = LoadLevel(strLevel);
            if (level.numWaves > 0)
            {
                retval.Add(level);
            }
        }
        return retval;
    }
    static Level LoadLevel(string strLevel)
    {
        var waves = new List<Level.Wave>();
        var waveStrings = strLevel.Split('\n');
        foreach (var strWave in waveStrings)
        {
            var wave = LoadWave(strWave);
            if (wave.squads.Count > 0)
            {
                waves.Add(wave);
            }
        }
        return new Level(waves);
    }
    static Level.Wave LoadWave(string strWave)
    {
        var squads = new List<Level.Squad>();
        if (strWave.Contains(","))
        {
            var squadStrings = strWave.Split(';');
            foreach (var strSquad in squadStrings)
            {
                var squad = LoadSquad(strSquad);
                squads.Add(squad);
            }
        }
        return new Level.Wave(squads);
    }
    static Level.Squad LoadSquad(string squad)
    {
        var parts = squad.Split(',');
        return new Level.Squad(parts[0], int.Parse(parts[1]));
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
