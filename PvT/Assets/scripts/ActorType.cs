using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class Asset
{
    public readonly int index;
    public readonly string id;
    public readonly GameObject prefab;

    static int s_instances = 0;
    public Asset(string id, GameObject prefab)
    {
        this.index = ++s_instances;
        this.id = id;
        this.prefab = prefab;
    }
    public GameObject CreateInstance()
    {
        return (GameObject)GameObject.Instantiate(prefab);
    }
    public override string ToString()
    {
        return string.Format("{0} {1}", index, id);
    }
}

public class ActorAttrs
{
    public readonly float maxSpeed;
    public readonly float sqrMaxSpeed; // pre-calculated, useful for comparing vector magnitudes
    public readonly float acceleration;
    public readonly float maxHealth;

    public ActorAttrs(float maxSpeed, float acceleration, float health)
    {
        this.maxSpeed = maxSpeed;
        this.sqrMaxSpeed = Mathf.Pow(maxSpeed, 2);
        this.acceleration = acceleration;
        this.maxHealth = health;
    }
    public override string ToString()
    {
        return string.Format("maxSpeed {0}, acceleration {1}, maxHealth {2}", maxSpeed, acceleration, maxHealth);
    }
}

public class ActorType
{
    public readonly Asset asset;
    public readonly string name;

    public readonly ActorAttrs attrs;
    public readonly float mass;
    public readonly float inertia;

    public readonly int baseLevel;
    public readonly string nextTierUpgrade;

    public readonly Weapon[] weapons; // the behavior's in charge of choosing a weapon and firing it
    public readonly bool dropShadow;

    public readonly float AIrepel;  // belongs somewhere else, later

    public ActorType(Asset asset, string name, float mass, Weapon[] weapons, int health, float maxSpeed, float acceleration, float inertia, bool dropShadow, int level, string upgradesTo, float AIrepel)
    {
        this.asset = asset;
        this.name = name;

        // For ease of config, the listed acceleration is absolute, unaffected by mass.  To
        // make this compatible with the physics engine, we'll pre-multiply our raw acceleration
        // by mass to get true acceleration.
        //
        // We'll have to do something similar with inertia, if we ever start using it.
        var trueAcceleration = acceleration * mass;
        this.attrs = new ActorAttrs(maxSpeed, trueAcceleration, health);

        this.mass = mass;
        this.weapons = weapons;
        
        this.inertia = inertia;
        this.dropShadow = dropShadow;

        this.baseLevel = level;
        this.nextTierUpgrade = upgradesTo;

        this.AIrepel = AIrepel;
    }
    public ActorType(ActorType rhs)
    {
        this.name = rhs.name;
        this.asset = rhs.asset;
        this.attrs = rhs.attrs;
        this.mass = rhs.mass;
        this.weapons = rhs.weapons;

        this.inertia = rhs.inertia;
        this.dropShadow = rhs.dropShadow;

        this.baseLevel = rhs.baseLevel;
        this.nextTierUpgrade = rhs.nextTierUpgrade;

        this.AIrepel = rhs.AIrepel;
    }
    public virtual GameObject Spawn()
    {
        var go = asset.CreateInstance();
        go.name = asset.id;
        return go;
    }
    public override string ToString()
    {
        return string.Format("{0}, asset {1}", name, asset.id);
    }

    public bool HasWeapons { get { return weapons != null && weapons.Length > 0; } }
    public bool HasChargeWeapon
    {
        get { return HasWeapons && weapons[0].attrs.chargeSeconds > 0; }
    }
    public bool HasShieldWeapon
    {
        get { return HasWeapons && weapons[0].actorName == "SHIELD"; }
    }

    public sealed class WeaponAttrs
    {
        public readonly float damage;
        public readonly float rate;
        public readonly float ttl;
        public readonly float chargeSeconds;

        public WeaponAttrs(float damage, float rate, float ttl, float chargeSeconds)
        {
            this.damage = damage;
            this.rate = rate;
            this.ttl = ttl;
            this.chargeSeconds = chargeSeconds;
        }
    }
    public sealed class Weapon
    {
        public readonly string actorName;
        public readonly WeaponAttrs attrs;

        public readonly Vector2 offset;
        public readonly int sequence;
        public readonly float angle;
        public readonly Color32 color;
        public readonly bool lit;

        public float severity { get; set; } // calculated later, once we know all the weapons

        public Weapon(string type, int dmg, Vector2 offset, float angle, Color32 color, bool lit, float rate, int sequence, float chargeSeconds, float ttl)
        {
            this.actorName = type;
            this.attrs = new WeaponAttrs(dmg, rate, ttl, chargeSeconds);

            this.offset = offset;
            this.sequence = sequence;
            this.angle = angle;
            this.color = color;
            this.lit = lit;

            this.severity = 0;
        }

        static public Weapon FromString(string str, float offsetY = 0)
        {
            var parts = Util.CSVLineParser(str);
            parts.NextString();

            var vehicle = parts.NextString();
            var dmg = parts.NextInt();
            var x = parts.NextFloat() / Consts.DEFAULT_PIXELS_PER_UNIT;
            var y = offsetY + parts.NextFloat() / Consts.DEFAULT_PIXELS_PER_UNIT;
            var angle = parts.NextFloat();
            var color = parts.NextHexColor();
            var lit = parts.NextBool();
            var rate = parts.NextFloat();
            var sequence = parts.NextInt();
            var charge = parts.NextFloat();
            parts.NextString();

            var timeToLive = parts.NextFloat();

            return new Weapon(vehicle, dmg, new Vector2(x, y), angle, color, lit, rate, sequence, charge, timeToLive);
        }

        public override string ToString()
        {
            return string.Format("Weapon {0} dmg {1} severity {2}", actorName, attrs.damage, severity);
        }
        public string ToCSV()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", actorName, attrs.damage, offset.x, offset.y, angle, severity, sequence, attrs.chargeSeconds);
        }
    }
}

public sealed class TankHullType : ActorType
{
    public readonly float turretPivotY;
    public TankHullType(ActorType baseType, float turretPivotY) : base(baseType)
    {
        this.turretPivotY = turretPivotY;
    }
}

public sealed class TankTurretType : ActorType
{
    public readonly float hullPivotY;
    public TankTurretType(ActorType baseType, float hullPivotY) : base(baseType)
    {
        this.hullPivotY = hullPivotY;
    }
}


