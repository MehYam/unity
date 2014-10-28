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
    readonly Dictionary<string, VehicleType> _vehicleLookup;
    readonly Dictionary<string, Tank> _tankLookup;
    readonly Dictionary<string, TankHullType> _tankHullLookup;
    readonly Dictionary<string, TankPartType> _tankTurretLookup;
    readonly Dictionary<string, AI> _ai;

    public readonly ReadOnlyCollection<Level> levels;

    public Loader(string strVehicles, string strTanks, string strHulls, string strTurrets, string strWeapons, string strLevels, string strAI, string strMisc)
    {
        _miscLookup = LoadMisc(strMisc, "other/");

        // LoadWeapons....

        // Chicken/egg - we load the weapon CSV first, then process it later.  WorldObjectType needs an
        // array of weapons to be constructed, but the weapons take the world object's dimensions into
        // account, so they both need to be dealth with simultaneously.
        var weaponStrings = LoadWeaponStrings(strWeapons);

        _vehicleLookup = LoadVehicles(strVehicles, "planes/", weaponStrings);
        _tankLookup = LoadTanks(strTanks);
        _tankHullLookup = LoadTankHulls(strHulls, "tanks/", weaponStrings);
        _tankTurretLookup = LoadTankTurrets(strTurrets, "tanks/", weaponStrings);

        levels = new ReadOnlyCollection<Level>(LoadLevels(strLevels));
        _ai = LoadAI(strAI);

        FixupWeaponLevels();
    }

    void FixupWeaponLevels()
    {
        // scan through all the weapon severities to find the min/max damage per weapon type.
        // we'll use this info later to give the more menacing weapons an appropriate effect
        var damageRanges = new Dictionary<string, Pair<float, float>>();
        foreach (var vehicle in _vehicleLookup)
        {
            // 1. find all the weapon damage ranges
            foreach (var weapon in vehicle.Value.weapons)
            {
                Pair<float, float> damageRange = null;
                damageRanges.TryGetValue(weapon.vehicleName, out damageRange);
                if (damageRange == null)
                {
                    damageRange = new Pair<float, float>(float.MaxValue, float.MinValue);
                    damageRanges[weapon.vehicleName] = damageRange;
                }

                if (weapon.damage < damageRange.first)
                {
                    damageRange.first = weapon.damage;
                }
                if (weapon.damage > damageRange.second)
                {
                    damageRange.second = weapon.damage;
                }
            }
        }
        // 2. do the fix up
        foreach (var vehicle in _vehicleLookup)
        {
            foreach (var weapon in vehicle.Value.weapons)
            {
                var damageRange = damageRanges[weapon.vehicleName];
                var magnitude = damageRange.second - damageRange.first;
                weapon.severity = magnitude == 0 ? 1 : (weapon.damage - damageRange.first) / magnitude;
            }
        }
    }

#if !UNITY_WEBPLAYER
    void ExportCSV(string path, Dictionary<string, VehicleType> items)
    {
        var file = File.CreateText(path);
        foreach (var item in items)
        {
            file.WriteLine(item.Value.ToCSV());
        }
        file.Close();
    }
#endif

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

    static Dictionary<string, IList<string>> LoadWeaponStrings(string csv)
    {
        var lookup = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

        var lines = Util.SplitLines(csv, true);
        foreach (var line in lines)
        {
            var items = Util.SplitCSVLine(line);
            var owner = items[0];

            if (!lookup.ContainsKey(owner))
            {
                lookup[owner] = new List<string>();
            }
            lookup[owner].Add(line);
        }
        return lookup;
    }

    static WorldObjectType LoadWorldObject(Util.CSVParseHelper csv, Dictionary<string, IList<string>> weaponLookup, string assetPath)
    {
        var name = csv.GetString();
        var assetID = csv.GetString();
        var mass = csv.GetFloat();
        var speed = csv.GetFloat();
        var health = csv.GetInt();

        // load the asset and extract the firepoints
        var prefab = Resources.Load<GameObject>(assetPath + assetID);
        prefab.transform.localPosition = Vector3.zero;

        // parse the weapons, if there are any
        IList<string> weaponStrings = null;
        weaponLookup.TryGetValue(name, out weaponStrings);

        WorldObjectType.Weapon[] weapons = new WorldObjectType.Weapon[ weaponStrings != null ? weaponStrings.Count : 0];
        if (weapons.Length > 0)
        {
            // programmatically determine the top of the sprite, since the weapon's Y offset
            // is relative to the top of the image.  This is nonideal...
            float offsetY = 0;
            var sprite = prefab.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                var bounds = sprite.bounds;
                offsetY = bounds.max.y;

                //Debug.Log(string.Format("{0} bounds {1} offsetY {2}", name, bounds, offsetY));
            }

            // now add each weapon
            int i = 0;
            foreach (var weaponString in weaponStrings)
            {
                var weapon = WorldObjectType.Weapon.FromString(weaponString, offsetY);
                weapons[i++] = weapon;
            }
        }
        return new WorldObjectType(
            prefab,
            name,
            assetID,
            mass,
            speed,
            health,
            weapons
        );
    }
    static WorldObjectType LoadWorldObject(string name, Hashtable obj, string assetPath)
    {
        var assetID = MJSON.SafeGetValue(obj, "asset");

        var prefab = Resources.Load<GameObject>(assetPath + assetID);
        prefab.transform.localPosition = Vector3.zero;

        return new WorldObjectType(
            prefab,
            name,
            assetID,
            MJSON.SafeGetFloat(obj, "mass"),
            MJSON.SafeGetFloat(obj, "maxSpeed"),
            MJSON.SafeGetFloat(obj, "health"),
            null
        );
    }
    //KAI: the value of this is quite slim - putting these into WorldObjectTypes doesn't do
    //much except make it easy to call ToGameObject()
    static Dictionary<string, WorldObjectType> LoadMisc(string strJSON, string assetPath)
    {
        var retval = new Dictionary<string, WorldObjectType>(StringComparer.OrdinalIgnoreCase);
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var name = (string)entry.Key;
            retval[name] = LoadWorldObject(name, node, assetPath);
        }
        return retval;
    }
    static VehicleType LoadVehicleType(WorldObjectType worldObject, Util.CSVParseHelper csvHelper)
    {
        var accel = csvHelper.GetFloat();
        var inertia = csvHelper.GetFloat();

        return new VehicleType(
                worldObject,
                accel,
                inertia
        );
    }
    static Dictionary<string, VehicleType> LoadVehicles(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, VehicleType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var worldObject = LoadWorldObject(csvHelper, weaponStrings, assetPath);
            results[worldObject.name] = LoadVehicleType(worldObject, csvHelper);
        }

        return results;
    }
    static Dictionary<string, Tank> LoadTanks(string strJSON)
    {
        var tanks = new Dictionary<string, Tank>(StringComparer.OrdinalIgnoreCase);
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var name = (string)entry.Key;
            tanks[name] = new Tank(name, MJSON.SafeGetValue(node, "hull"), MJSON.SafeGetValue(node, "turret"));
        }
        return tanks;
    }
    static Dictionary<string, TankHullType> LoadTankHulls(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, TankHullType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var worldObject = LoadWorldObject(csvHelper, weaponStrings, assetPath);
            var vehicle = LoadVehicleType(worldObject, csvHelper);

            results[worldObject.name] = new TankHullType(vehicle, csvHelper.GetFloat());
        }
        return results;
    }
    static Dictionary<string, TankPartType> LoadTankTurrets(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, TankPartType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var worldObject = LoadWorldObject(csvHelper, weaponStrings, assetPath);

            results[worldObject.name] = new TankPartType(worldObject, csvHelper.GetFloat());
        }
        return results;
    }

    static IList<Level> LoadLevels(string strLevels)
    {
        var retval = new List<Level>();

        var levelStrings = strLevels.Split('#');
        foreach (var strLevel in levelStrings)
        {
            var level = LoadLevel(strLevel);
            if (level.events.Count > 0)
            {
                retval.Add(level);
            }
        }
        return retval;
    }
    static Level LoadLevel(string strLevel)
    {
        var events = new List<Level.IScriptEvent>();
        var eventLines = strLevel.Split('\n');
        foreach (var line in eventLines)
        {
            if (line.Contains("EVENT_"))
            {
                events.Add(LoadScriptEvent(line));
            }
            if (line.Contains(","))
            {
                events.Add(LoadMobSpawnEvent(line));
            }
        }
        return new Level(events);
    }
    static Level.IScriptEvent LoadMobSpawnEvent(string strLine)
    {
        var mobs = new List<Level.Mobs>();
        var mobSpecs = strLine.Split(';');
        foreach (var mobSpec in mobSpecs)
        {
            if (mobSpec.Length > 2)
            {
                var m = LoadMobs(mobSpec);
                mobs.Add(m);
            }
            else
            {
                Debug.LogError("Bad mob string: " + mobSpec);
            }
        }
        return new Level.MobSpawnEvent(mobs);
    }
    static Level.IScriptEvent LoadScriptEvent(string strLine)
    {
        return new Level.ScriptEvent(strLine.Trim());
    }
    static Level.Mobs LoadMobs(string str)
    {
        var parts = str.Split(',');
        return new Level.Mobs(parts[0], int.Parse(parts[1]));
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
    public sealed class Mobs
    {
        public readonly string mobID;
        public readonly int count;

        public Mobs(string mobID, int count) { this.mobID = mobID; this.count = count; }

    }
    public interface IScriptEvent {}
    public sealed class ScriptEvent : IScriptEvent
    {
        public readonly string id;

        public ScriptEvent(string id) { this.id = id; }
    }
    public sealed class MobSpawnEvent : IScriptEvent
    {
        public readonly IList<Mobs> mobs;

        public MobSpawnEvent(IList<Mobs> mobs) { this.mobs = mobs; }
    }

    public readonly IList<IScriptEvent> events;
    public Level(IList<IScriptEvent> events) { this.events = events; }
}
