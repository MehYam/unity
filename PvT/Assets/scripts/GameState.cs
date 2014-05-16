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
        Debug.LogWarning("GameState constructor " + GetHashCode());

        _enemyLookup = LoadEnemies(strEnemies);
        _levels = LoadLevels(strLevels);
    }

    public void HandleCollision(Vector2 where)
    {
        var boom = (GameObject)GameObject.Instantiate(Main.Instance.Explosion);
        boom.transform.localPosition = where;

        var anim = boom.GetComponent<Animation>();
        anim.Play();
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
                MJSON.SafeGetValue(enemy, "assetID"),
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
            if (level.waves.Count > 0)
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

    public readonly IList<Wave> waves;
    public Level(IList<Wave> waves) { this.waves = waves; }
}
