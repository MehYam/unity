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
    readonly Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

    readonly Dictionary<string, Asset> _miscLookup;
    readonly Dictionary<string, ActorType> _actorTypeLookup;
    readonly Dictionary<string, Tank> _tankLookup;
    readonly Dictionary<string, TankHullType> _tankHullLookup;
    readonly Dictionary<string, TankTurretType> _tankTurretLookup;
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

        _actorTypeLookup = LoadActorTypes(strVehicles, "planes/", weaponStrings);

        _tankLookup = LoadTanks(strTanks);
        _tankHullLookup = LoadTankHullTypes(strHulls, "tanks/", weaponStrings);
        _tankTurretLookup = LoadTankTurretTypes(strTurrets, "tanks/", weaponStrings);

        levels = new ReadOnlyCollection<Level>(LoadLevels(strLevels));
        _ai = LoadAI(strAI);

        FixupWeaponLevels();
    }

    void FixupWeaponLevels()
    {
        // scan through all the weapon severities to find the min/max damage per weapon type.
        // we'll use this info later to give the more menacing weapons an appropriate effect
        var damageRanges = new Dictionary<string, Pair<float, float>>();
        foreach (var vehicle in _actorTypeLookup)
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
        foreach (var vehicle in _actorTypeLookup)
        {
            foreach (var weapon in vehicle.Value.weapons)
            {
                var damageRange = damageRanges[weapon.vehicleName];
                var magnitude = damageRange.second - damageRange.first;
                weapon.severity = magnitude == 0 ? 1 : (weapon.damage - damageRange.first) / magnitude;
            }
        }
    }

    public Asset GetMisc(string type)
    {
        Asset retval = null;
        _miscLookup.TryGetValue(type, out retval);
        return retval;
    }
    public ActorType GetVehicle(string type)  // planes, ammo
    {
        ActorType retval = null;
        _actorTypeLookup.TryGetValue(type, out retval);
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
    public TankTurretType GetTankPart(string type)
    {
        TankTurretType retval = null;
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

    ActorType LoadActorType(Util.CSVParseHelper csv, Dictionary<string, IList<string>> weaponLookup, string assetPath)
    {
        var name = csv.GetString();
        var assetID = csv.GetString();
        var mass = csv.GetFloat();
        var speed = csv.GetFloat();
        var health = csv.GetInt();
        var accel = csv.GetFloat();
        var inertia = csv.GetFloat();
        var dropShadow = csv.GetBool();

        var asset = LoadAsset(assetID, assetPath);

        // parse the weapons, if there are any
        IList<string> weaponStrings = null;
        weaponLookup.TryGetValue(name, out weaponStrings);

        ActorType.Weapon[] weapons = new ActorType.Weapon[ weaponStrings != null ? weaponStrings.Count : 0];
        if (weapons.Length > 0)
        {
            // programmatically determine the top of the sprite, since the weapon's Y offset
            // is relative to the top of the image.  It's not perfect, but it's simple because
            // weapons usually fire from the front
            float offsetY = 0;
            var sprite = asset.prefab.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                var bounds = sprite.bounds;
                offsetY = bounds.max.y;
            }

            // now add each weapon
            int i = 0;
            foreach (var weaponString in weaponStrings)
            {
                var weapon = ActorType.Weapon.FromString(weaponString, offsetY);
                weapons[i++] = weapon;
            }
        }
        return new ActorType(
            asset: asset,
            name: name,
            mass: mass,
            weapons: weapons,
            health: health,
            maxSpeed: speed,
            acceleration: accel,
            inertia: inertia,
            dropShadow: dropShadow
        );
    }
    TankHullType LoadTankHullType(Util.CSVParseHelper csv, Dictionary<string, IList<string>> weaponLookup, string assetPath)
    {
        var baseActorType = LoadActorType(csv, weaponLookup, assetPath);
        var turretPivotY = csv.GetFloat();
        return new TankHullType(baseActorType, turretPivotY);
    }
    TankTurretType LoadTankTurretType(Util.CSVParseHelper csv, Dictionary<string, IList<string>> weaponLookup, string assetPath)
    {
        var baseActorType = LoadActorType(csv, weaponLookup, assetPath);
        var hullPivotY = csv.GetFloat();
        return new TankTurretType(baseActorType, hullPivotY);
    }
    Asset LoadAsset(string name, string assetPath)
    {
        Asset retval = null;
        if (!_assets.TryGetValue(name, out retval))
        {
            var prefab = Resources.Load<GameObject>(assetPath + name);
            prefab.transform.localPosition = Vector3.zero;
            retval = new Asset(name, prefab);
        }
        return retval;
    }
    //KAI: the value of this is quite slim - putting these into WorldObjectTypes doesn't do
    //much except make it easy to call ToGameObject()
    Dictionary<string, Asset> LoadMisc(string strJSON, string assetPath)
    {
        var retval = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var name = (string)entry.Key;
            retval[name] = LoadAsset(name, assetPath);
        }
        return retval;
    }
    Dictionary<string, ActorType> LoadActorTypes(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, ActorType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var actorType = LoadActorType(csvHelper, weaponStrings, assetPath);
            results[actorType.name] = actorType;
        }

        return results;
    }
    Dictionary<string, TankHullType> LoadTankHullTypes(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, TankHullType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var actorType = LoadTankHullType(csvHelper, weaponStrings, assetPath);
            results[actorType.name] = actorType;
        }
        return results;
    }
    Dictionary<string, TankTurretType> LoadTankTurretTypes(string csv, string assetPath, Dictionary<string, IList<string>> weaponStrings)
    {
        var results = new Dictionary<string, TankTurretType>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Util.SplitLines(csv, true))
        {
            var csvHelper = new Util.CSVParseHelper(line);
            var actorType = LoadTankTurretType(csvHelper, weaponStrings, assetPath);
            results[actorType.name] = actorType;
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
