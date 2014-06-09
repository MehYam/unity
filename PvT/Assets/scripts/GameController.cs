//#define DEBUG_AMMO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class GameController
{
    public GameObject player { get; private set; }

    public Loader loader { get; private set; }

    readonly Effects effects;
    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        GlobalGameEvent.Instance.MapReady += OnMapReady;

        effects = new Effects(loader);
    }

    //KAI: some nice way to mark this as dev only?
    public void Debug_Respawn(Loader loader)
    {
        if (player != null)
        {
            GameObject.Destroy(player);
        }
        this.loader = loader;
        SpawnPlayer();
    }

    public XRect WorldBounds { get; private set; }
    void OnMapReady(TileMap map, XRect bounds)
    {
        GlobalGameEvent.Instance.MapReady -= OnMapReady;
        WorldBounds = bounds;

        Start();
    }

    void Start()
    {
        SpawnPlayer();
        StartNextLevel();
    }
    void SpawnPlayer()
    {
        var main = Main.Instance;
        if (main.defaultIsPlane)
        {
            Debug.Log("spawning " + main.defaultPlane);
            var playerVehicle = loader.GetVehicle(main.defaultPlane);
            var player = SpawnWorldObject(playerVehicle);
            InitPlayer(player, playerVehicle);
            AddPlayerPlaneBehaviors(player, playerVehicle);
        }
        else
        {
            var tankHelper = new TankSpawnHelper(this, main.defaultTank, main.defaultTurret);
            InitPlayer(tankHelper.hullGO, tankHelper.hull);
            AddPlayerTankBehaviors(tankHelper);
        }
    }
    void StartNextLevel()
    {
        StartNextWave();
    }

    int _liveEnemies = 0;
    void StartNextWave()
    {
        var wave = loader.levels[0].NextWave();
        foreach (var squad in wave.squads)
        {
            VehicleType v = loader.GetVehicle(squad.enemyID);
            if (v != null)
            {
                for (int i = 0; i < squad.count; ++i)
                {
                    SpawnMob(v);
                    ++_liveEnemies;
                }
            }
            else
            {
                Debug.LogError("VehicleType not found for enemy " + squad.enemyID);
            }
        }
    }

    GameObject SpawnWorldObject(WorldObjectType worldObject, bool physics = true)
    {
        var go = worldObject.ToGameObject();
        var actor = go.AddComponent<Actor>();
        actor.worldObject = worldObject;

        //KAI: this whole hierarchy seems messy and in need of simplification.  It would be easier if it had no types...
        actor.health = Mathf.Max(1, worldObject.health);
        Debug.Log(worldObject.name + " " + actor.health);

        go.AddComponent<DropShadow>();
        go.name = worldObject.name;

        if (physics)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.mass = float.IsNaN(worldObject.mass) ? 0 : worldObject.mass;
            body.drag = 0.5f;
        }
        return go;
    }

    void SpawnMob(VehicleType vehicle)
    {
        var go = SpawnWorldObject(vehicle);
        go.name += " enemy";

        var ai = loader.GetAI(vehicle.name);
        if (ai != null)
        {
            go.GetComponent<Actor>().behavior = ActorBehaviorScripts.Instance.Get(ai.behavior);
        }

        // put the actor at the edge
        Vector3 spawnLocation;
        var bounds = new XRect(WorldBounds);
        bounds.Inflate(-1);

        if (Consts.CoinFlip())
        {
            spawnLocation = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Consts.CoinFlip() ? bounds.min.y : bounds.max.y);
        }
        else
        {
            spawnLocation = new Vector3(Consts.CoinFlip() ? bounds.min.x : bounds.max.x, Random.Range(bounds.min.y, bounds.max.y));
        }
        go.transform.localPosition = spawnLocation;
        go.layer = (int)Consts.Layer.MOB;
    }

    //kai: this ain't perfect
    public void SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer)
    {
        var go = type.ToGameObject();
        var body = go.AddComponent<Rigidbody2D>();
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
#if DEBUG_AMMO
        sprite.sortingOrder = 10;
#else
        sprite.sortingOrder = Consts.AMMO_SORT_ORDER;
#endif
        var ammo = go.AddComponent<Actor>();
        ammo.worldObject = type;
        ammo.timeToLive = 2;
        ammo.health = Mathf.Max(1, type.health);


        var scale = launcher.transform.localScale;
        var scaledOffset = new Vector2(weapon.offset.x, weapon.offset.y);
        scaledOffset.Scale(scale);
        scaledOffset.y = weapon.offset.y;  //KAI: not sure why....

        //Debug.Log(string.Format("{0} -> {1}, scale {2}", weapon.offset, scaledOffset, scale));

        ammo.transform.localPosition = Consts.Add(launcher.transform.position, scaledOffset);
        ammo.transform.RotateAround(launcher.transform.position, Vector3.forward, launcher.transform.rotation.eulerAngles.z);

        ammo.transform.Rotate(0, 0, -weapon.angle);
        if (type.acceleration == 0)
        {
            // give the ammo instant acceleration
            body.mass = 0;
            body.velocity = Consts.GetLookAtVector(ammo.transform.rotation.eulerAngles.z, type.maxSpeed);
        }
        else
        {
            // treat the ammo like a vehicle (i.e. rocket)
            body.mass = type.mass;
            ammo.behavior = ActorBehaviorFactory.Instance.thrust;
        }

        go.layer = (int)layer;
    }

    sealed class TankSpawnHelper
    {
        public readonly TankHullType hull;
        public readonly TankPartType turret;
        public readonly GameObject hullGO;
        public readonly GameObject turretGO;
        public readonly GameObject treadLeft;
        public readonly GameObject treadRight;
        public TankSpawnHelper(GameController game, string tankHull, string tankTurret)
        {
            hull = game.loader.GetTankHull(tankHull);
            turret = game.loader.GetTankPart(tankTurret);
            var tread = game.loader.GetTankPart("tanktreadParent");

            hullGO = game.SpawnWorldObject(hull);
            turretGO = game.SpawnWorldObject(turret, false);

            hullGO.rigidbody2D.drag = 1;

            treadLeft = tread.ToGameObject();
            treadRight = tread.ToGameObject();
            treadLeft.name = "treadLeft";
            treadRight.name = "treadRight";

            turretGO.transform.parent = hullGO.transform;
            treadLeft.transform.parent = hullGO.transform;
            treadRight.transform.parent = hullGO.transform;

            var hullSprite = hullGO.GetComponent<SpriteRenderer>();
            hullSprite.sortingOrder = -2;

            var hullBounds = hullSprite.sprite.bounds;
            var pivotY = hullBounds.min.y + hull.turretPivotY / Consts.PixelsToUnits;

            turretGO.GetComponent<SpriteRenderer>().sortingOrder = -1;
            turretGO.gameObject.transform.localPosition = new Vector3(0, pivotY);

            treadLeft.gameObject.transform.Rotate(0, 0, 180);
            treadRight.gameObject.transform.Rotate(0, 0, 180);
            treadLeft.gameObject.transform.localPosition = new Vector3(hullBounds.min.x, 0);
            treadRight.gameObject.transform.localPosition = new Vector3(hullBounds.max.x, 0);
        }
    }

    void AddPlayerPlaneBehaviors(GameObject go, VehicleType vehicle)
    {
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput());
        behaviors.Add(ActorBehaviorFactory.Instance.faceForward);
        behaviors.Add(ActorBehaviorFactory.Instance.faceMouseOnFire);
        behaviors.Add(ActorBehaviorFactory.Instance.CreatePlayerfire(
                ActorBehaviorFactory.Instance.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO))
        );

        go.GetComponent<Actor>().behavior = behaviors;
    }
    void AddPlayerTankBehaviors(TankSpawnHelper tankHelper)
    {
        var bf =ActorBehaviorFactory.Instance;

        // hull
        var behaviors = new CompositeBehavior();
        behaviors.Add(new PlayerInput());
        behaviors.Add(bf.faceForward);
        behaviors.Add(bf.CreatePlayerfire(bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO)));
        behaviors.Add(bf.CreateTankTreadAnimator(tankHelper.treadLeft, tankHelper.treadRight));
        tankHelper.hullGO.GetComponent<Actor>().behavior = behaviors;

        // turret
        tankHelper.turretGO.GetComponent<Actor>().behavior = new CompositeBehavior(
            bf.faceMouse,
            bf.CreatePlayerfire(bf.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO))
        );

    }
    void InitPlayer(GameObject go, VehicleType vehicle)
    {
        go.name += " player";
        go.transform.localPosition = Vector3.zero;
        go.layer = (int)Consts.Layer.FRIENDLY;

        player = go;

        Camera.main.GetComponent<CameraFollow>().Target = go;

        GlobalGameEvent.Instance.FirePlayerSpawned(go);
    }

    public void HandleCollision(ContactPoint2D contact)
    {
        if (contact.collider.gameObject == Main.Instance.game.player)
        {
            var boom = effects.GetRandomSmallExplosion().ToGameObject();
            boom.transform.localPosition = contact.point;
        }

        var go = contact.collider.gameObject;
        if (go.layer == (int)Consts.Layer.MOB)
        {
            //--_liveEnemies;
            //GameObject.Destroy(go);
        }

        if (_liveEnemies == 0)
        {
            StartNextWave();
        }
    }

}

public sealed class Level
{
    public sealed class Squad
    {
        public readonly string enemyID;
        public readonly int count;

        public Squad(string enemy, int count) { this.enemyID = enemy; this.count = count; }

    }
    public sealed class Wave
    {
        public readonly IList<Squad> squads;

        public Wave(IList<Squad> squads) { this.squads = squads; }
    }

    readonly IList<Wave> waves;
    int nextWave = 0;
    public Level(IList<Wave> waves) { this.waves = waves; }

    public Wave NextWave()
    {
        return nextWave <= waves.Count ? waves[nextWave++] : null;
    }
    public int numWaves { get { return waves.Count; } }
}
