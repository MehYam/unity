using UnityEngine;
using System.Collections;

public class WorldObjectType
{
    readonly GameObject prefab;

    public readonly string name;
    public readonly string assetID;  //KAI: enum? int?  not string.
    public readonly float mass;

    public readonly Weapon[] weapons; // the behavior's in charge of choosing a weapon and firing it

    public WorldObjectType(GameObject prefab, string name, string assetID, float mass, Weapon[] weapons)
    {
        this.prefab = prefab;
        this.name = name;
        this.assetID = assetID;
        this.mass = mass;
        this.weapons = weapons;
    }
    public WorldObjectType(WorldObjectType rhs)
    {
        this.prefab = rhs.prefab;
        this.name = rhs.name;
        this.assetID = rhs.assetID;
        this.mass = rhs.mass;
        this.weapons = rhs.weapons;
    }
    public GameObject ToGameObject()
    {
        return (GameObject)GameObject.Instantiate(prefab);
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

        static public Weapon FromString(string str, float offsetY = 0)
        {
            var parts = str.Split(',');
            string type = parts[0];
            int dmg = int.Parse(parts[1]);
            float x = float.Parse(parts[2]) / Consts.PixelsToUnits;
            float y = offsetY + float.Parse(parts[3])/Consts.PixelsToUnits;
            float angle = parts.Length > 4 ? float.Parse(parts[4]) : 0;
            int level = parts.Length > 5 ? int.Parse(parts[5]) : 0;

            return new Weapon(type, dmg, new Vector2(x, y), angle, level);
        }

        public override string ToString()
        {
            return string.Format("Weapon {0} dmg {1} level {2}", type, dmg, level);
        }
    }
}

public class VehicleType : WorldObjectType
{
    public readonly int health;
    public readonly float maxSpeed;
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;

    public VehicleType(GameObject prefab, string name, string assetID, float mass, Weapon[] weapons, int health, float maxSpeed, float acceleration, float inertia, float collDmg) :
        base(prefab, name, assetID, mass, weapons)
    {
        this.health = health;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
    }
    public VehicleType(WorldObjectType baseClass, int health, float maxSpeed, float acceleration, float inertia, float collDmg) :
        base(baseClass)
    {
        this.health = health;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
    }
    public VehicleType(VehicleType rhs) :
        base(rhs)
    {
        this.health = rhs.health;
        this.maxSpeed = rhs.maxSpeed;
        this.acceleration = rhs.acceleration;
        this.inertia = rhs.inertia;
        this.collDmg = rhs.collDmg;
    }
}

public sealed class TankHullType : VehicleType
{
    public readonly float turretPivotY;

    public TankHullType(GameObject prefab, string name, string assetID, float mass, Weapon[] weapons, int health, float maxSpeed, float acceleration, float inertia, float collDmg, float turretPivotY)
        : base(prefab, name, assetID, mass, weapons, health, maxSpeed, acceleration, inertia, collDmg)
    {
        this.turretPivotY = turretPivotY;
    }
    public TankHullType(VehicleType baseClass, float turretPivotY) :
        base(baseClass)
    {
        this.turretPivotY = turretPivotY;
    }
}

public sealed class TankPartType : WorldObjectType
{
    public readonly float hullPivotY;
    public TankPartType(GameObject prefab, string name, string assetID, float mass, Weapon[] weapons, float hullPivotY) :
        base(prefab, name, assetID, mass, weapons)
    {
        this.hullPivotY = hullPivotY;
    }
    public TankPartType(WorldObjectType baseClass, float hullPivotY) : 
        base(baseClass)
    {
        this.hullPivotY = hullPivotY;
    }
}

