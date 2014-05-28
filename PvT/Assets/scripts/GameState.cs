using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public sealed class GameState
{
    readonly Dictionary<string, Enemy> _enemyLookup;  // need ReadOnlyDictionary here
    readonly IList<Level> _levels;
    public GameState(string strEnemies, string strLevels)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        _enemyLookup = LoadEnemies(strEnemies);
        _levels = LoadLevels(strLevels);
    }
    
    public void Start()
    {
        StartNextLevel();
    }
    void StartNextLevel()
    {
        StartNextWave();
    }
    void StartNextWave()
    {
        var wave = _levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            var enemy = _enemyLookup[squad.enemyID];
            Debug.Log("planes/" + enemy.assetID);
            var prefab = Resources.Load<GameObject>("planes/" + enemy.assetID);
            for (int i = 0; i < squad.count; ++i)
            {
                var go = (GameObject)GameObject.Instantiate(prefab);
                go.transform.localPosition = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5));
            }
        }
    }

    public void HandleCollision(ContactPoint2D contact)
    {
        var boom = (GameObject)GameObject.Instantiate(Main.Instance.Explosion);
        boom.transform.localPosition = contact.point;

        var anim = boom.GetComponent<Animation>();
        anim.Play();

        GameObject.Destroy(contact.collider.gameObject);
    }

    static Dictionary<string, Enemy> LoadEnemies(string enemyJSON)
    {
        var retval = new Dictionary<string, Enemy>();

        var json = MJSON.hashtableFromJson(enemyJSON);
        foreach (DictionaryEntry entry in json)
        {
            var name = (string)entry.Key;
            var enemy = (Hashtable)entry.Value;

            retval[name] = new Enemy(
                name,
                MJSON.SafeGetValue(enemy, "asset"),
                MJSON.SafeGetInt(enemy, "health"),
                MJSON.SafeGetFloat(enemy, "maxSpeed"),
                MJSON.SafeGetFloat(enemy, "acceleration"),
                MJSON.SafeGetFloat(enemy, "inertia"),
                MJSON.SafeGetFloat(enemy, "collision"),
                MJSON.SafeGetInt(enemy, "reward")
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

public sealed class Enemy
{
    public readonly string name;
    public readonly string assetID;
    public readonly int health;
    public readonly float maxSpeed;
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;
    public readonly int reward;

    public Enemy(string name, string assetID, int health, float maxSpeed, float acceleration, float inertia, float collDmg, int reward) 
    {
        this.name = name;
        this.assetID = assetID;
        this.health = health;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
        this.reward = reward;
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
