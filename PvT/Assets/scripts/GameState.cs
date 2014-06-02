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

    int _liveEnemies = 0;
    void StartNextWave()
    {
        var wave = _levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            var plane = _vehicleLookup[squad.enemyID];
            for (int i = 0; i < squad.count; ++i)
            {
                SpawnMob(plane);
                ++_liveEnemies;
            }
        }
    }

    void SpawnMob(VehicleType plane)
    {
        var go = (GameObject)GameObject.Instantiate(plane.prefab);
        go.SetActive(true);

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
        go.layer = Consts.MOB_LAYER;
    }

    public VehicleType GetVehicle(string type)
    {
        return _vehicleLookup[type];
    }

    //kai: this ain't perfect
    public void SpawnMobAmmo(Actor launcher, VehicleType type, VehicleType.Weapon weapon)
    {
        var go = (GameObject)GameObject.Instantiate(type.prefab);
        go.SetActive(true);

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

        if (contact.collider.gameObject == Main.Instance.Player)
        {
            var anim = boom.GetComponent<Animation>();
            anim.Play();
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
            var vehicle = (Hashtable)entry.Value;
            var assetID = MJSON.SafeGetValue(vehicle, "asset");

            // load the asset and extract the firepoints
            var prefab = Resources.Load<GameObject>(path + assetID);

            VehicleType.Weapon[] weapons = null;
            var payload = MJSON.SafeGetArray(vehicle, "payload");
            if (payload != null)
            {
                weapons = new VehicleType.Weapon[payload.Count];

                int i = 0;
                foreach (string ammo in payload)
                {
                    weapons[i++] = VehicleType.Weapon.FromString(ammo);
                }
            }
            results[name] = new VehicleType(
                name,
                assetID,
                MJSON.SafeGetInt(vehicle, "health"),
                MJSON.SafeGetFloat(vehicle, "mass"),
                MJSON.SafeGetFloat(vehicle, "maxSpeed"),
                MJSON.SafeGetFloat(vehicle, "acceleration"),
                MJSON.SafeGetFloat(vehicle, "inertia"),
                MJSON.SafeGetFloat(vehicle, "collision"),
                MJSON.SafeGetInt(vehicle, "reward"),
                MJSON.SafeGetValue(vehicle, "behavior"),
                weapons,
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
