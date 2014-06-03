using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using PvT.Util;

public class Loader
{
    readonly Dictionary<string, VehicleType> _vehicleLookup = new Dictionary<string, VehicleType>();  // need ReadOnlyDictionary here

    public readonly ReadOnlyCollection<Level> levels;

    public Loader(string strVehicles, string strAmmo, string strLevels)
    {
        LoadVehicles(strVehicles, "planes/", _vehicleLookup);
        LoadVehicles(strAmmo, "ammo/", _vehicleLookup);

        levels = new ReadOnlyCollection<Level>(LoadLevels(strLevels));
    }

    public VehicleType GetVehicle(string type)
    {
        VehicleType retval = null;
        _vehicleLookup.TryGetValue(type, out retval);
        return retval;
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
                MJSON.SafeGetFloat(vehicle, "acceleration") * 15,
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
