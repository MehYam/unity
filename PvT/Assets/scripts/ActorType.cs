using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using PvT.Util;

public class Asset
{
    public readonly int index;
    public readonly string name;
    public readonly GameObject prefab;

    static int s_instances = 0;
    public Asset(string name, GameObject prefab)
    {
        this.index = ++s_instances;
        this.name = name;
        this.prefab = prefab;
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
    public override string ToString()
    {
        return string.Format("{0} {1}", index, name);
    }
}

public class ActorType
{
    public readonly Asset asset;
    public readonly string name;
    public readonly float mass;
    public readonly float maxSpeed;
    public readonly float sqrMaxSpeed; // useful for comparing vector magnitudes

    public readonly float acceleration;
    public readonly float inertia;
    public readonly bool dropShadow;

    public readonly float health = 1;
    public readonly Weapon[] weapons; // the behavior's in charge of choosing a weapon and firing it

    public ActorType(Asset asset, string name, float mass, Weapon[] weapons, int health, float maxSpeed, float acceleration, float inertia, bool dropShadow)
    {
        this.name = name;
        this.asset = asset;
        this.maxSpeed = maxSpeed;
        this.sqrMaxSpeed = (float)System.Math.Pow(maxSpeed, 2);
        this.mass = mass;
        this.health = health;
        this.weapons = weapons;
        
        this.acceleration = acceleration;
        this.inertia = inertia;
        this.dropShadow = dropShadow;
    }
    public ActorType(ActorType rhs)
    {
        this.name = rhs.name;
        this.asset = rhs.asset;
        this.maxSpeed = rhs.maxSpeed;
        this.sqrMaxSpeed = (float)System.Math.Pow(maxSpeed, 2);
        this.mass = rhs.mass;
        this.health = rhs.health;
        this.weapons = rhs.weapons;

        this.acceleration = rhs.acceleration;
        this.inertia = rhs.inertia;
        this.dropShadow = rhs.dropShadow;
    }
    public bool HasWeapons { get { return weapons != null && weapons.Length > 0; } }
    public virtual GameObject Spawn(Consts.SortingLayer sortingLayer, bool rigidBody)
    {
        var go = asset.ToRawGameObject(sortingLayer);
        if (rigidBody)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.mass = float.IsNaN(mass) ? 0 : mass;
            body.drag = 0.5f;
            body.angularDrag = 5;

            go.GetComponent<Collider2D>().sharedMaterial = Main.Instance.assets.Bounce;
        }

        //KAI: MAJOR CHEESE
        var actor = this.name == "HEROLING" ? go.AddComponent<HerolingActor>() : go.AddComponent<Actor>();
        actor.actorType = this;
        actor.collisionDamage = health / 4;

        if (dropShadow)
        {
            go.AddComponent<DropShadow>();
        }
        return go;
    }

    public sealed class Weapon
    {
        public readonly string vehicleName;
        public readonly int damage;
        public readonly Vector2 offset;
        public readonly float angle;
        public readonly Color32 color;
        public readonly bool lit;
        public readonly float rate;
        public readonly int sequence;
        public readonly float chargeSeconds;
        public readonly float ttl;

        public float severity { get; set; } // calculated later, once we know all the weapons

        public Weapon(string type, int dmg, Vector2 offset, float angle, Color32 color, bool lit, float rate, int sequence, float chargeSeconds, float ttl)
        {
            this.vehicleName = type;
            this.damage = dmg;
            this.offset = offset;
            this.angle = angle;
            this.color = color;
            this.lit = lit;
            this.rate = rate;
            this.sequence = sequence;
            this.chargeSeconds = chargeSeconds;
            this.ttl = ttl;

            this.severity = 0;
        }

        static public Weapon FromString(string str, float offsetY = 0)
        {
            var parts = new Util.CSVParseHelper(str);
            parts.SkipField();

            var vehicle = parts.GetString();
            var dmg = parts.GetInt();
            var x = parts.GetFloat() / Consts.PixelsToUnits;
            var y = offsetY + parts.GetFloat() / Consts.PixelsToUnits;
            var angle = parts.GetFloat();
            var color = parts.GetHexColor();
            var lit = parts.GetBool();
            var rate = parts.GetFloat();
            var sequence = parts.GetInt();
            var charge = parts.GetFloat();
            parts.SkipField();
            var timeToLive = parts.GetFloat();

            return new Weapon(vehicle, dmg, new Vector2(x, y), angle, color, lit, rate, sequence, charge, timeToLive);
        }

        public override string ToString()
        {
            return string.Format("Weapon {0} dmg {1} severity {2}", vehicleName, damage, severity);
        }
        public string ToCSV()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", vehicleName, damage, offset.x, offset.y, angle, severity, sequence, chargeSeconds);
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
    public override GameObject Spawn(Consts.SortingLayer sortingLayer, bool rigidBody)
    {
        var go = base.Spawn(sortingLayer, true);
        go.rigidbody2D.drag = 1;
        go.rigidbody2D.angularDrag = 5;
        return go;
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

public sealed class TankSpawnHelper
{
    const string HULL_NAME = "hull";
    const string TURRET_NAME = "turret";
    const string LEFT_TREAD_NAME = "treadLeft";
    const string RIGHT_TREAD_NAME = "treadRight";

    public readonly TankHullType hull;
    public readonly TankTurretType turret;
    public readonly GameObject hullGO;
    public readonly GameObject turretGO;
    public readonly GameObject treadLeft;
    public readonly GameObject treadRight;
    public TankSpawnHelper(GameController game, string tankHull, string tankTurret)
    {
        hull = game.loader.GetTankHull(tankHull);
        turret = game.loader.GetTankPart(tankTurret);
        var tread = game.loader.GetMisc("tanktreadParent");

        hullGO = hull.Spawn(Consts.SortingLayer.TANKBODY, true);
        turretGO = turret.Spawn(Consts.SortingLayer.TANKTURRET, false);

        hullGO.name = HULL_NAME;
        turretGO.name = TURRET_NAME;

        treadLeft = tread.ToRawGameObject(Consts.SortingLayer.TANKTREAD);
        treadRight = tread.ToRawGameObject(Consts.SortingLayer.TANKTREAD);
        treadLeft.name = LEFT_TREAD_NAME;
        treadRight.name = RIGHT_TREAD_NAME;

        turretGO.transform.parent = hullGO.transform;
        treadLeft.transform.parent = hullGO.transform;
        treadRight.transform.parent = hullGO.transform;

        var hullSprite = hullGO.GetComponent<SpriteRenderer>();
        var hullBounds = hullSprite.sprite.bounds;
        var pivotY = hullBounds.min.y + hull.turretPivotY / Consts.PixelsToUnits;
        turretGO.gameObject.transform.localPosition = new Vector3(0, pivotY);

        Debug.Log("TURRET LOCAL POS " + turretGO.gameObject.transform.localPosition + ", " + hull.turretPivotY);

        treadLeft.gameObject.transform.Rotate(0, 0, 180);
        treadRight.gameObject.transform.Rotate(0, 0, 180);
        treadLeft.gameObject.transform.localPosition = new Vector3(hullBounds.min.x, 0);
        treadRight.gameObject.transform.localPosition = new Vector3(hullBounds.max.x, 0);
    }

    //KAI: cheesy - if we had a proper TankActor type, we wouldn't need this...  all this type-aversion is making for scrambled code
    /// <summary>
    /// Re-constructs the spawn helper for an already-spawned tank, useful for the possession case
    /// </summary>
    /// <param name="gameObject">The already spawned tank</param>
    public TankSpawnHelper(GameObject gameObject)
    {
        this.hullGO = gameObject;
        this.turretGO = gameObject.transform.FindChild(TURRET_NAME).gameObject;
        this.treadLeft = gameObject.transform.FindChild(LEFT_TREAD_NAME).gameObject;
        this.treadRight = gameObject.transform.FindChild(RIGHT_TREAD_NAME).gameObject;

        this.hull = (TankHullType)hullGO.GetComponent<Actor>().actorType;
        this.turret = (TankTurretType)turretGO.GetComponent<Actor>().actorType;
    }
}

