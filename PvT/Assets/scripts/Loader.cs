using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using PvT.Util;

public class Loader
{
    // need ReadOnlyDictionary's here
    readonly Dictionary<string, WorldObjectType> _miscLookup;
    readonly Dictionary<string, VehicleType> _vehicleLookup = new Dictionary<string, VehicleType>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, Tank> _tankLookup = new Dictionary<string, Tank>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, TankHullType> _tankHullLookup = new Dictionary<string, TankHullType>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, TankPartType> _tankTurretLookup = new Dictionary<string, TankPartType>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, AI> _ai;

    public readonly ReadOnlyCollection<Level> levels;

    public Loader(string strVehicles, string strTanks, string strHulls, string strTurrets, string strAmmo, string strLevels, string strAI, string strMisc)
    {
        _miscLookup = LoadMisc(strMisc, "other/");
        LoadVehicles(strVehicles, "planes/", _vehicleLookup);

        //ExportCSV("c:\\source\\unity\\PvT\\planes.csv", _vehicleLookup);

        LoadVehicles(strAmmo, "ammo/", _vehicleLookup);

        LoadTanks(strTanks, _tankLookup);
        LoadTankHulls(strHulls, "tanks/", _tankHullLookup);
        LoadTankTurrets(strTurrets, "tanks/", _tankTurretLookup);

        levels = new ReadOnlyCollection<Level>(LoadLevels(strLevels));

        var s = new StringBuilder();
        foreach (var v in _vehicleLookup)
        {
            foreach (var w in v.Value.weapons)
            {
                s.Append(v.Value.name + ", ");
                s.AppendLine(w.ToCSV());
            }
        }
        Debug.Log("Weapons: \n" + s.ToString());
        _ai = LoadAI(strAI);
    }

    void ExportCSV(string path, Dictionary<string, VehicleType> items)
    {
        var file = File.CreateText(path);
        foreach (var item in items)
        {
            file.WriteLine(item.Value.ToCSV());
        }
        file.Close();
    }

    public WorldObjectType GetMisc(string type)
    {
        WorldObjectType retval = null;
        _miscLookup.TryGetValue(type, out retval);
        return retval;
    }
    public VehicleType GetVehicle(string type)  // planes, ammo
    {
        VehicleType retval = null;
        _vehicleLookup.TryGetValue(type, out retval);
        return retval;
    }
    public Tank GetTank(string tankName)
    {
        Tank retval = null;
        _tankLookup.TryGetValue(tankName, out retval);
        return retval;
    }
    public TankHullType GetTankHull(string type)
    {
        TankHullType retval = null;
        _tankHullLookup.TryGetValue(type, out retval);
        return retval;
    }
    public TankPartType GetTankPart(string type)
    {
        TankPartType retval = null;
        _tankTurretLookup.TryGetValue(type, out retval);
        return retval;
    }
    public AI GetAI(string key)
    {
        AI retval = null;
        _ai.TryGetValue(key, out retval);
        return retval;
    }

    static WorldObjectType LoadWorldObject(string name, Hashtable obj, string assetPath)
    {
        var assetID = MJSON.SafeGetValue(obj, "asset");

        // load the asset and extract the firepoints
        var prefab = Resources.Load<GameObject>(assetPath + assetID);
        prefab.transform.localPosition = Vector3.zero;

        WorldObjectType.Weapon[] weapons = null;
        var payload = MJSON.SafeGetArray(obj, "payload");
        if (payload != null)
        {
            // programmatically determine the top of the sprite, since the weapon's Y offset
            // is done in terms of the top of the image.  This is confusing.
            float offsetY = 0;
            var sprite = prefab.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                var bounds = sprite.bounds;
                offsetY = bounds.max.y;

                //Debug.Log(string.Format("{0} bounds {1} offsetY {2}", name, bounds, offsetY));
            }
            weapons = new WorldObjectType.Weapon[payload.Count];

            int i = 0;
            foreach (string ammo in payload)
            {
                var weapon = WorldObjectType.Weapon.FromString(ammo, offsetY);
                weapons[i++] = weapon;
            }
        }
        return new WorldObjectType(
            prefab,
            name,
            assetID,
            MJSON.SafeGetFloat(obj, "mass"),
            MJSON.SafeGetFloat(obj, "maxSpeed"),
            MJSON.SafeGetFloat(obj, "health"),
            weapons
        );
    }
    //KAI: the value of this is quite slim - putting these into WorldObjectTypes doesn't do
    //much except make it easy to call ToGameObject()
    static Dictionary<string, WorldObjectType> LoadMisc(string strJSON, string assetPath)
    {
        var retval = new Dictionary<string, WorldObjectType>();
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var name = (string)entry.Key;
            retval[name] = LoadWorldObject(name, node, assetPath);
        }
        return retval;
    }
    static VehicleType LoadVehicleType(WorldObjectType worldObject, Hashtable node)
    {
        return new VehicleType(
                worldObject,
                MJSON.SafeGetFloat(node, "acceleration"),
                MJSON.SafeGetFloat(node, "inertia"),
                MJSON.SafeGetFloat(node, "collision") * Consts.COLLISION_DAMAGE_MULTIPLIER
        );
    }
    static void LoadVehicles(string strJSON, string assetPath, Dictionary<string, VehicleType> results)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var worldObject = LoadWorldObject((string)entry.Key, node, assetPath);
            results[worldObject.name] = LoadVehicleType(worldObject, node);
        }
    }
    static void LoadTanks(string strJSON, Dictionary<string, Tank> tanks)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var name = (string)entry.Key;
            tanks[name] = new Tank(name, MJSON.SafeGetValue(node, "hull"), MJSON.SafeGetValue(node, "turret"));
        }
    }
    static void LoadTankHulls(string strJSON, string assetPath, Dictionary<string, TankHullType> results)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var worldObject = LoadWorldObject((string)entry.Key, node, assetPath);

            var vehicle = LoadVehicleType(worldObject, node);
            var pivotY = MJSON.SafeGetFloat(node, "pivotY");
            results[worldObject.name] = new TankHullType(vehicle, pivotY);
        }
    }
    static void LoadTankTurrets(string strJSON, string assetPath, Dictionary<string, TankPartType> results)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var worldObject = LoadWorldObject((string)entry.Key, node, assetPath);
            var pivotY = MJSON.SafeGetFloat(node, "pivotY");
            results[worldObject.name] = new TankPartType(worldObject, pivotY);
        }
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
                if (strSquad.Length > 2)
                {
                    var squad = LoadSquad(strSquad);
                    squads.Add(squad);
                }
                else
                {
                    Debug.LogError("Bad wave string: " + strWave);
                }
            }
        }
        return new Level.Wave(squads);
    }
    static Level.Squad LoadSquad(string squad)
    {
        var parts = squad.Split(',');
        return new Level.Squad(parts[0], int.Parse(parts[1]));
    }

    static Dictionary<string, AI> LoadAI(string ai)
    {
        var retval = new Dictionary<string, AI>();
        var json = MJSON.hashtableFromJson(ai);
        foreach (DictionaryEntry entry in json)
        {
            var name = (string)entry.Key;
            var node = (Hashtable)entry.Value;

            retval[name] = new AI(name,
                MJSON.SafeGetValue(node, "vehicle"),
                MJSON.SafeGetValue(node, "behavior"),
                MJSON.SafeGetInt(node, "reward")
            );
        }
        return retval;
    }
}

public sealed class AI
{
    public readonly string name;
    public readonly string vehicleType; //KAI: enum
    public readonly string behavior; //KAI: enum
    public readonly int reward;

    public AI(string name, string vehicleType, string behavior, int reward)
    {
        this.name = name;
        this.vehicleType = vehicleType;
        this.behavior = behavior;
        this.reward = reward;
    }
}

public sealed class Tank
{
    public readonly string name;
    public readonly string hullName;
    public readonly string turretName;

    public Tank(string name, string hull, string turret) { this.name = name; hullName = hull; turretName = turret; }
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
