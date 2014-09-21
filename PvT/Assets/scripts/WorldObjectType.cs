using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class WorldObjectType
{
    readonly GameObject prefab;

    public readonly int index;
    public readonly string name;
    public readonly string assetID;  //KAI: enum? int?  not string.

    public readonly float mass;
    public readonly float maxSpeed;
    public readonly float sqrMaxSpeed; // useful for comparing vector magnitudes

    public readonly float health = 1;
    public readonly Weapon[] weapons; // the behavior's in charge of choosing a weapon and firing it

    static int s_instances = 0;
    public WorldObjectType(GameObject prefab, string name, string assetID, float mass, float maxSpeed, float health, Weapon[] weapons)
    {
        this.index = ++s_instances;
        this.prefab = prefab;
        this.name = name;
        this.assetID = assetID;
        this.maxSpeed = maxSpeed;
        this.sqrMaxSpeed = (float)System.Math.Pow(maxSpeed, 2);
        this.mass = mass;
        this.health = health;
        this.weapons = weapons;
    }
    public WorldObjectType(WorldObjectType rhs)
    {
        this.index = rhs.index;
        this.prefab = rhs.prefab;
        this.name = rhs.name;
        this.assetID = rhs.assetID;
        this.mass = rhs.mass;
        this.maxSpeed = rhs.maxSpeed;
        this.sqrMaxSpeed = rhs.sqrMaxSpeed;
        this.health = rhs.health;
        this.weapons = rhs.weapons;
    }
    public GameObject ToRawGameObject(Consts.SortingLayer sortingLayer)
    {
        var retval = (GameObject)GameObject.Instantiate(prefab);
        if (retval.renderer != null)
        {
            retval.renderer.sortingLayerID = (int)sortingLayer;
        }
        return retval;
    }
    public virtual GameObject Spawn(Consts.SortingLayer sortingLayer)
    {
        var go = ToRawGameObject(sortingLayer);
        go.name = name;

        //KAI: MAJOR CHEESE
        var actor = this.name == "HEROLING" ? go.AddComponent<HerolingActor>() : go.AddComponent<Actor>();
        actor.worldObject = this;
        return go;
    }

    public override string ToString()
    {
        return string.Format("{0} {1} mass {2} maxSpeed {3}", name, assetID, mass, maxSpeed);
    }
    public virtual string ToCSV()
    {
        return string.Format("{4},{0},{1},{2},{3}", name, assetID, mass, maxSpeed, index);
    }
    public sealed class Weapon
    {
        public readonly string vehicleName;
        public readonly int damage;
        public readonly Vector2 offset;
        public readonly float angle;
        public readonly float rate;
        public readonly int sequence;

        public float severity { get; set; } // calculated later, once we know all the weapons

        public Weapon(string type, int dmg, Vector2 offset, float angle, float rate, int sequence)
        {
            this.vehicleName = type;
            this.damage = dmg;
            this.offset = offset;
            this.angle = angle;
            this.rate = rate;
            this.sequence = sequence;

            this.severity = 0;
        }

        static public Weapon FromString(string str, float offsetY = 0)
        {
            var parts = new Util.CSVParseHelper(str);
            parts.SetIndex(1);

            var vehicle = parts.GetString();
            var dmg = parts.GetInt();
            var x = parts.GetFloat() / Consts.PixelsToUnits;
            var y = offsetY + parts.GetFloat()/Consts.PixelsToUnits;
            var angle = parts.GetFloat();
            
            parts.GetInt(); // level, not currently used
            var rate = parts.GetFloat();
            var sequence = parts.GetInt();

            return new Weapon(vehicle, dmg, new Vector2(x, y), angle, rate, sequence);
        }

        public override string ToString()
        {
            return string.Format("Weapon {0} dmg {1} severity {2}", vehicleName, damage, severity);
        }
        public string ToCSV()
        {
            return string.Format("{0},{1},{2},{3},{4},{5}", vehicleName, damage, offset.x, offset.y, angle, severity);
        }
    }
}

public class VehicleType : WorldObjectType
{
    public readonly float acceleration;
    public readonly float inertia;
    public readonly float collDmg;

    public VehicleType(GameObject prefab, string name, string assetID, float mass, Weapon[] weapons, int health, float maxSpeed, float acceleration, float inertia, float collDmg) :
        base(prefab, name, assetID, mass, maxSpeed, health, weapons)
    {
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
    }
    public VehicleType(WorldObjectType baseClass, float acceleration, float inertia, float collDmg) :
        base(baseClass)
    {
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.collDmg = collDmg;
    }
    public VehicleType(VehicleType rhs) :
        base(rhs)
    {
        this.acceleration = rhs.acceleration;
        this.inertia = rhs.inertia;
        this.collDmg = rhs.collDmg;
    }
    public override GameObject Spawn(Consts.SortingLayer sortingLayer)
    {
        var go = SpawnNoRigidbody(sortingLayer);

        go.AddComponent<DropShadow>();

        var body = go.AddComponent<Rigidbody2D>();
        body.mass = float.IsNaN(mass) ? 0 : mass;
        body.drag = 0.5f;
        body.angularDrag = 5;

        go.GetComponent<Collider2D>().sharedMaterial = Main.Instance.Bounce;
        go.GetComponent<Actor>().collisionDamage = collDmg;
        return go;
    }
    public GameObject SpawnNoRigidbody(Consts.SortingLayer sortingLayer)
    {
        // KAI: this is weak, and only here to support the fusion hack.  Something more thought-out and/or generalized will be simpler
        var go = base.Spawn(sortingLayer);

        go.AddComponent<DropShadow>();
        return go;
    }
    public override string ToCSV()
    {
        return base.ToCSV() + string.Format(",{0},{1},{2},{3}", health, acceleration, inertia, collDmg);
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
    public override GameObject Spawn(Consts.SortingLayer sortingLayer)
    {
        var go = base.Spawn(sortingLayer);
        go.rigidbody2D.drag = 1;
        go.rigidbody2D.angularDrag = 5;
        return go;
    }
    public override string ToCSV()
    {
        return base.ToCSV() + ", " + turretPivotY;
    }
}

public sealed class TankPartType : WorldObjectType
{
    public readonly float hullPivotY;
    public TankPartType(GameObject prefab, string name, string assetID, float mass, float health, Weapon[] weapons, float hullPivotY) :
        base(prefab, name, assetID, mass, float.NaN, health, weapons)
    {
        this.hullPivotY = hullPivotY;
    }
    public TankPartType(WorldObjectType baseClass, float hullPivotY) : 
        base(baseClass)
    {
        this.hullPivotY = hullPivotY;
    }
    public override string ToCSV()
    {
        return base.ToCSV() + ", " + hullPivotY;
    }
}

