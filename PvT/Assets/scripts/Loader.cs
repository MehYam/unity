using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public class Loader
{
    // need ReadOnlyDictionary's here
    readonly Dictionary<string, VehicleType> _vehicleLookup = new Dictionary<string, VehicleType>();
    readonly Dictionary<string, TankHullType> _tankHullLookup = new Dictionary<string, TankHullType>();
    readonly Dictionary<string, TankTurretType> _tankTurretLookup = new Dictionary<string, TankTurretType>();

    public readonly ReadOnlyCollection<Level> levels;

    public Loader(string strVehicles, string strAmmo, string strHulls, string strTurrets, string strLevels)
    {
        LoadVehicles(strVehicles, "planes/", _vehicleLookup);
        LoadVehicles(strAmmo, "ammo/", _vehicleLookup);

        LoadTankHulls(strHulls, "tanks/", _tankHullLookup);
        LoadTankTurrets(strTurrets, "tanks/", _tankTurretLookup);

        levels = new ReadOnlyCollection<Level>(LoadLevels(strLevels));
    }

    public VehicleType GetVehicle(string type)  // planes, ammo
    {
        VehicleType retval = null;
        _vehicleLookup.TryGetValue(type, out retval);
        return retval;
    }
    public TankHullType GetTankHull(string type)
    {
        TankHullType retval = null;
        _tankHullLookup.TryGetValue(type, out retval);
        return retval;
    }
    public TankTurretType GetTankTurret(string type)
    {
        TankTurretType retval = null;
        _tankTurretLookup.TryGetValue(type, out retval);
        return retval;
    }

    static WorldObjectType LoadWorldObject(string name, Hashtable obj, string assetPath)
    {
        var assetID = MJSON.SafeGetValue(obj, "asset");

        // load the asset and extract the firepoints
        var prefab = Resources.Load<GameObject>(assetPath + assetID);

        WorldObjectType.Weapon[] weapons = null;
        var payload = MJSON.SafeGetArray(obj, "payload");
        if (payload != null)
        {
            weapons = new WorldObjectType.Weapon[payload.Count];

            int i = 0;
            foreach (string ammo in payload)
            {
                weapons[i++] = WorldObjectType.Weapon.FromString(ammo);
            }
        }
        return new WorldObjectType(
            prefab,
            name,
            assetID,
            MJSON.SafeGetValue(obj, "behavior"),
            MJSON.SafeGetFloat(obj, "mass"),
            weapons
        );
    }
    static VehicleType LoadVehicleType(WorldObjectType worldObject, Hashtable node)
    {
        return new VehicleType(
                worldObject,
                MJSON.SafeGetInt(node, "health"),
                MJSON.SafeGetFloat(node, "maxSpeed"),
                MJSON.SafeGetFloat(node, "acceleration") * 15,
                MJSON.SafeGetFloat(node, "inertia"),
                MJSON.SafeGetFloat(node, "collision"),
                MJSON.SafeGetInt(node, "reward")
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
    static void LoadTankHulls(string strJSON, string assetPath, Dictionary<string, TankHullType> results)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var worldObject = LoadWorldObject((string)entry.Key, node, assetPath);

            Debug.Log(worldObject.name);
            var vehicle = LoadVehicleType(worldObject, node);
            var pivotY = MJSON.SafeGetFloat(node, "pivotY");
            results[worldObject.name] = new TankHullType(vehicle, pivotY);
        }
    }
    static void LoadTankTurrets(string strJSON, string assetPath, Dictionary<string, TankTurretType> results)
    {
        var json = MJSON.hashtableFromJson(strJSON);
        foreach (DictionaryEntry entry in json)
        {
            var node = (Hashtable)entry.Value;
            var worldObject = LoadWorldObject((string)entry.Key, node, assetPath);
            var pivotY = MJSON.SafeGetFloat(node, "pivotY");
            results[worldObject.name] = new TankTurretType(worldObject, pivotY);
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
