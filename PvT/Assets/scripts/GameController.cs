//#define DEBUG_AMMO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class GameController
{
    public GameObject player { get; private set; }
    public Loader loader { get; private set; }
    public Effects effects { get; private set; }

    readonly GameObject ammoParent;
    public GameController(Loader loader)
    {
        Debug.Log("GameState constructor " + GetHashCode());

        this.loader = loader;
        this.effects = new Effects(loader);
        this.ammoParent = GameObject.Find("_ammoParent");

        GlobalGameEvent.Instance.MapReady += OnMapReady;
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
            var player = playerVehicle.Spawn();
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

    void SpawnMob(VehicleType vehicle)
    {
        var go = vehicle.Spawn();
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

    public GameObject SpawnAmmo(Actor launcher, VehicleType type, WorldObjectType.Weapon weapon, Consts.Layer layer)
    {
        var go = type.Spawn();
        go.transform.parent = ammoParent.transform;

        var body = go.GetComponent<Rigidbody2D>();
        body.drag = 0;

        var sprite = go.GetComponent<SpriteRenderer>();
#if DEBUG_AMMO
        sprite.sortingOrder = 10;
#else
        sprite.sortingOrder = Consts.AMMO_SORT_ORDER;
#endif
        var ammo = go.GetComponent<Actor>();
        ammo.timeToLive = 2;
        ammo.collisionDamage = weapon.damage;

        Consts.Sneeze(launcher.transform, ammo.transform, weapon.offset, weapon.angle);
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

        if (launcher.worldObject is TankPartType)
        {
            // it's a turret
            SpawnMuzzleFlash(ammo);
        }
        return go;
    }

    void SpawnMuzzleFlash(Actor launcher)
    {
        var flash = effects.GetRandomMuzzleFlash().ToRawGameObject();
        flash.transform.position = launcher.transform.position;
        flash.transform.rotation = launcher.transform.rotation;
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

            hullGO = hull.Spawn();
            turretGO = turret.Spawn();

            treadLeft = tread.ToRawGameObject();
            treadRight = tread.ToRawGameObject();
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

        //KAI: cheeze
        if (vehicle.weapons[0].type == "SHIELD")
        {
            behaviors.Add(ActorBehaviorFactory.Instance.CreateShield());
        }
        else
        {
            behaviors.Add(ActorBehaviorFactory.Instance.CreatePlayerfire(
                    ActorBehaviorFactory.Instance.CreateAutofire(new RateLimiter(0.5f), Consts.Layer.FRIENDLY_AMMO))
            );
        }

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

    //KAI: use global game event
    public void HandleCollision(ContactPoint2D contact)
    {
        var colliderActor = contact.collider.GetComponent<Actor>();
        var otherColliderActor = contact.otherCollider.GetComponent<Actor>();

        //Debug.Log(string.Format("{0} layer {1} <=> {2} layer {3}", colliderActor.name, colliderActor.gameObject.layer, otherColliderActor.name, otherColliderActor.gameObject.layer));

        if (colliderActor != null && otherColliderActor != null &&
            colliderActor.gameObject.layer != otherColliderActor.gameObject.layer)
        {
            if (colliderActor.gameObject.layer > otherColliderActor.gameObject.layer)
            {
                // this prevents dual collision sparks when two things collide
                var boom = effects.GetRandomSmallExplosion().ToRawGameObject();
                boom.transform.localPosition = contact.point;
            }

            colliderActor.TakeDamage(otherColliderActor.collisionDamage * Random.Range(0.9f, 1.1f));
        }
    }
    //KAI: use GlobalGameEvent
    public void HandleActorDeath(Actor actor)
    {
        var enemy = actor.gameObject.layer == (int)Consts.Layer.MOB;
        if (actor.gameObject.layer == (int)Consts.Layer.FRIENDLY || enemy)
        {
            var asplode = effects.GetVehicleExplosion().ToRawGameObject();
            asplode.transform.position = actor.transform.position;

            if (enemy)
            {
                --_liveEnemies;
                if (_liveEnemies == 0)
                {
                    StartNextWave();
                }
            }
        }
        GameObject.Destroy(actor.gameObject);
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
