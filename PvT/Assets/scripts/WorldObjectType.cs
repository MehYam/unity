using UnityEngine;
using System.Collections;

public class WorldObjectType
{
    public readonly GameObject prefab;

    public readonly string name;
    public readonly string assetID;  //KAI: enum? int?  not string.
    public readonly string behaviorKey;  //KAI: enum? int?  not string.
    public readonly float mass;

    public readonly Weapon[] weapons; // the behavior's in charge of choosing a weapon and firing it

    public WorldObjectType(GameObject prefab, string name, string assetID, string behaviorKey, float mass, Weapon[] weapons)
    {
        this.prefab = prefab;

        this.name = name;
        this.assetID = assetID;
        this.mass = mass;
        this.behaviorKey = behaviorKey;
        this.weapons = weapons;
    }

    public sealed class Weapon
    {
        public readonly string type; // KAI: convert to enum
        public readonly int dmg;
        public readonly Vector2 offset;
        public readonly float angle;
        public readonly int level; // KAI: nuke this.... turn it instead into a new type

        public Weapon(string type, int dmg, Vector2 offset, float angle, int level)
        {
            this.type = type;
            this.dmg = dmg;
            this.offset = offset;
            this.angle = angle;
            this.level = level;
        }

        static public Weapon FromString(string str)
        {
            var parts = str.Split(',');
            string type = parts[0];
            int dmg = int.Parse(parts[1]);
            float x = float.Parse(parts[2]) / Consts.PixelsToUnits;
            float y = -float.Parse(parts[3]) / Consts.PixelsToUnits;
            float angle = parts.Length > 4 ? float.Parse(parts[4]) : 0;
            int level = parts.Length > 5 ? int.Parse(parts[5]) : 0;

            return new Weapon(type, dmg, new Vector2(x, y), angle, level);
        }
    }
}

public sealed class VehicleType
{
    public readonly WorldObjectType worldObject;
    public readonly int health;
    public readonly float maxSpeed;
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;
    public readonly int reward;

    public VehicleType(WorldObjectType worldObject, int health, float maxSpeed, float acceleration, float inertia, float collDmg, int reward)
    {
        this.worldObject = worldObject;
        this.health = health;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
        this.reward = reward;
    }
}

public sealed class TankHullType
{
    public readonly VehicleType vehicle;
    public readonly float turretPivotY;

    public TankHullType(VehicleType vehicle, float turretPivotY)
    {
        this.vehicle = vehicle;
        this.turretPivotY = turretPivotY;
    }
}

public sealed class TankTurretType
{
    public readonly WorldObjectType worldObject;
    public readonly float hullPivotY;

    public TankTurretType(WorldObjectType worldObject, float hullPivotY)
    {
        this.worldObject = worldObject;
        this.hullPivotY = hullPivotY;
    }
}

