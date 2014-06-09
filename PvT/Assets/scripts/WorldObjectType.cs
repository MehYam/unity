using UnityEngine;
using System.Collections;

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
    public GameObject ToRawGameObject()
    {
        return (GameObject)GameObject.Instantiate(prefab);
    }
    public virtual GameObject Spawn()
    {
        var go = ToRawGameObject();
        go.name = name;
        var actor = go.AddComponent<Actor>();
        actor.worldObject = this;
        actor.health = Mathf.Max(1, health);
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
        public readonly string type; // KAI: convert to enum
        public readonly int damage;
        public readonly Vector2 offset;
        public readonly float angle;
        public readonly int level; // KAI: nuke this.... turn it instead into a new type

        public Weapon(string type, int dmg, Vector2 offset, float angle, int level)
        {
            this.type = type;
            this.damage = dmg;
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
            return string.Format("Weapon {0} dmg {1} level {2}", type, damage, level);
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
    public override GameObject Spawn()
    {
        var go = base.Spawn();

        go.AddComponent<DropShadow>();

        var body = go.AddComponent<Rigidbody2D>();
        body.mass = float.IsNaN(mass) ? 0 : mass;
        body.drag = 0.5f;

        go.GetComponent<Collider2D>().sharedMaterial = Main.Instance.Bounce;
        go.GetComponent<Actor>().collisionDamage = collDmg;

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
    public override GameObject Spawn()
    {
        var go = base.Spawn();
        go.rigidbody2D.drag = 1;
        return go;
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
}

