using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public sealed class GameState
{
    readonly Dictionary<string, Vehicle> _planeLookup;  // need ReadOnlyDictionary here
    readonly IList<Level> _levels;
    public GameState(string strEnemies, string strLevels)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        _planeLookup = LoadEnemies(strEnemies);
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
            var plane = _planeLookup[squad.enemyID];
            Debug.Log("planes/" + plane.assetID);

            var cachedPrefab = GetCachedPlanePrefab(plane.assetID);
            for (int i = 0; i < squad.count; ++i)
            {
                SpawnEnemyPlane(cachedPrefab.prefab, plane);
                ++_liveEnemies;
            }
        }
    }

    readonly Dictionary<string, CachedVehiclePrefab> _planePrefabs = new Dictionary<string, CachedVehiclePrefab>();
    CachedVehiclePrefab GetCachedPlanePrefab(string id)
    {
        CachedVehiclePrefab retval;
        if (!_planePrefabs.TryGetValue(id, out retval))
        {
            var prefab = Resources.Load<GameObject>("planes/" + id);

            // extract the FirePoints, cache them and the processed fire points
            var firePoints = prefab.GetComponentsInChildren<FirePoint>();
            var vehicleFirePoints = new List<VehicleFirePoint>(firePoints.Length);
            foreach (var point in firePoints)
            {
                vehicleFirePoints.Add(new VehicleFirePoint(
                    new Vector2(point.transform.localPosition.x, point.transform.localPosition.y),
                    point.transform.localEulerAngles.z)
                );
            }

            retval = new CachedVehiclePrefab(prefab, new List<VehicleFirePoint>(vehicleFirePoints));
            _planePrefabs[id] = retval;

            Debug.LogWarning("created " + id);
        }
        return retval;
    }
    
    void SpawnEnemyPlane(GameObject prefab, Vehicle plane)
    {
        var go = (GameObject)GameObject.Instantiate(prefab);
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
        Debug.Log(bounds);
        bounds.Inflate(-1);
        Debug.Log(bounds);

        if (Consts.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Consts.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Consts.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        Debug.Log(spawnLocation);
        go.transform.localPosition = spawnLocation;

        go.layer = ENEMY_LAYER;
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

    static Dictionary<string, Vehicle> LoadEnemies(string enemyJSON)
    {
        var retval = new Dictionary<string, Vehicle>();

        var json = MJSON.hashtableFromJson(enemyJSON);
        foreach (DictionaryEntry entry in json)
        {
            var name = (string)entry.Key;
            var enemy = (Hashtable)entry.Value;

            retval[name] = new Vehicle(
                name,
                MJSON.SafeGetValue(enemy, "asset"),
                MJSON.SafeGetInt(enemy, "health"),
                MJSON.SafeGetFloat(enemy, "mass"),
                MJSON.SafeGetFloat(enemy, "maxSpeed"),
                MJSON.SafeGetFloat(enemy, "acceleration"),
                MJSON.SafeGetFloat(enemy, "inertia"),
                MJSON.SafeGetFloat(enemy, "collision"),
                MJSON.SafeGetInt(enemy, "reward"),
                MJSON.SafeGetValue(enemy, "behavior")
            );
        }
        return retval;
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

public sealed class Vehicle
{
    public readonly string name;
    public readonly string assetID;
    public readonly int health;
    public readonly float mass;
    public readonly float maxSpeed;
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;
    public readonly int reward;
    public readonly string behaviorKey;

    public Vehicle(string name, string assetID, int health, float mass, float maxSpeed, float acceleration, float inertia, float collDmg, int reward, string behaviorKey) 
    {
        this.name = name;
        this.assetID = assetID;
        this.health = health;
        this.mass = mass;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
        this.reward = reward;
        this.behaviorKey = behaviorKey;
    }
}

public sealed class VehicleFirePoint
{
    public readonly Vector2 point;
    public readonly float angle;

    public VehicleFirePoint(Vector2 point, float angle) { this.point = point; this.angle = angle; }
}
public sealed class CachedVehiclePrefab
{
    public readonly GameObject prefab;  // this is a prefab with the FirePoints removed
    public readonly ReadOnlyCollection<VehicleFirePoint> firePoints;

    public CachedVehiclePrefab(GameObject prefab, IList<VehicleFirePoint> firePoints)
    {
        this.prefab = prefab;
        this.firePoints = new ReadOnlyCollection<VehicleFirePoint>(firePoints);
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
